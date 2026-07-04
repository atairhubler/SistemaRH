using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Desktop.Services;

namespace SistemaRH.Desktop.ViewModels;

public class AtestadosViewModel : BaseViewModel
{
    private readonly IAtestadoService _atestadoService;
    private readonly IDialogService _dialogService;

    private int _funcionarioId;
    private ObservableCollection<AtestadoDto> _atestados = new();

    private DateTime _novaDataAtestado = DateTime.Now;
    private DateTime _novaDataInicio = DateTime.Now;
    private DateTime _novaDataFim = DateTime.Now;
    private string _novoTipo = "";
    private string _novoCid = "";
    private string _novaDescricao = "";
    private string _novaObservacao = "";
    private string? _caminhoArquivoSelecionado;
    private string _nomeArquivoSelecionado = "Nenhum arquivo selecionado";

    public ObservableCollection<AtestadoDto> Atestados
    {
        get => _atestados;
        set => SetProperty(ref _atestados, value);
    }

    public DateTime NovaDataAtestado { get => _novaDataAtestado; set => SetProperty(ref _novaDataAtestado, value); }

    public DateTime NovaDataInicio
    {
        get => _novaDataInicio;
        set { if (SetProperty(ref _novaDataInicio, value)) OnPropertyChanged(nameof(DiasFaltadosCalculado)); }
    }

    public DateTime NovaDataFim
    {
        get => _novaDataFim;
        set { if (SetProperty(ref _novaDataFim, value)) OnPropertyChanged(nameof(DiasFaltadosCalculado)); }
    }

    public int DiasFaltadosCalculado => Math.Max(0, (NovaDataFim.Date - NovaDataInicio.Date).Days + 1);

    public string NovoTipo { get => _novoTipo; set => SetProperty(ref _novoTipo, value); }
    public string NovoCid { get => _novoCid; set => SetProperty(ref _novoCid, value); }
    public string NovaDescricao { get => _novaDescricao; set => SetProperty(ref _novaDescricao, value); }
    public string NovaObservacao { get => _novaObservacao; set => SetProperty(ref _novaObservacao, value); }
    public string NomeArquivoSelecionado { get => _nomeArquivoSelecionado; set => SetProperty(ref _nomeArquivoSelecionado, value); }

    public List<string> OpcoesTipo { get; } = new()
    {
        "Atestado Médico",
        "Atestado Odontológico",
        "Declaração de Comparecimento",
        "Outro"
    };

    public ICommand SelecionarArquivoCommand { get; }
    public ICommand AdicionarCommand { get; }
    public ICommand RemoverCommand { get; }
    public ICommand BaixarCommand { get; }
    public ICommand FecharCommand { get; }

    public Action? FecharJanela { get; set; }

    public AtestadosViewModel(IAtestadoService atestadoService, IDialogService dialogService)
    {
        _atestadoService = atestadoService;
        _dialogService = dialogService;

        SelecionarArquivoCommand = new RelayCommand(_ => _ = SelecionarArquivoAsync());
        AdicionarCommand = new RelayCommand(_ => _ = AdicionarAsync());
        RemoverCommand = new RelayCommand(param => _ = RemoverAsync(param as AtestadoDto));
        BaixarCommand = new RelayCommand(param => _ = BaixarAsync(param as AtestadoDto));
        FecharCommand = new RelayCommand(_ => FecharJanela?.Invoke());
    }

    public void Preparar(int funcionarioId)
    {
        _funcionarioId = funcionarioId;
        NovaDataAtestado = DateTime.Now;
        NovaDataInicio = DateTime.Now;
        NovaDataFim = DateTime.Now;
        NovoTipo = OpcoesTipo[0];
        NovoCid = "";
        NovaDescricao = "";
        NovaObservacao = "";
        _caminhoArquivoSelecionado = null;
        NomeArquivoSelecionado = "Nenhum arquivo selecionado";
        _ = CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        try
        {
            IsLoading = true;
            var lista = await _atestadoService.GetByFuncionarioAsync(_funcionarioId);
            Atestados = new ObservableCollection<AtestadoDto>(lista);
            StatusMessage = $"{Atestados.Count} atestado(s)";
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Erro", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SelecionarArquivoAsync()
    {
        var caminho = await _dialogService.AbrirArquivoAsync("Documentos (PDF/Imagem)|*.pdf;*.jpg;*.jpeg;*.png;*.bmp");
        if (string.IsNullOrEmpty(caminho)) return;

        _caminhoArquivoSelecionado = caminho;
        NomeArquivoSelecionado = Path.GetFileName(caminho);
    }

    private async Task AdicionarAsync()
    {
        if (string.IsNullOrWhiteSpace(NovoTipo) || string.IsNullOrEmpty(_caminhoArquivoSelecionado))
        {
            await _dialogService.ShowErrorAsync("Validação", "Informe o tipo do atestado e selecione o arquivo (PDF ou foto).");
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Registrando atestado...";

            var dto = new AtestadoDto
            {
                FuncionarioId = _funcionarioId,
                DataAtestado = NovaDataAtestado,
                DataInicio = NovaDataInicio,
                DataFim = NovaDataFim,
                DiasFaltados = DiasFaltadosCalculado,
                Tipo = NovoTipo,
                Descricao = NovaDescricao,
                Cid = NovoCid,
                Observacoes = NovaObservacao
            };

            await using (var stream = File.OpenRead(_caminhoArquivoSelecionado))
            {
                await _atestadoService.AdicionarAsync(dto, stream, Path.GetFileName(_caminhoArquivoSelecionado));
            }

            NovoCid = "";
            NovaDescricao = "";
            NovaObservacao = "";
            _caminhoArquivoSelecionado = null;
            NomeArquivoSelecionado = "Nenhum arquivo selecionado";

            await CarregarAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro: {ex.Message}";
            await _dialogService.ShowErrorAsync("Erro", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RemoverAsync(AtestadoDto? atestado)
    {
        if (atestado == null) return;

        var confirma = await _dialogService.ShowConfirmAsync("Remover atestado", "Tem certeza que deseja remover este atestado?");
        if (!confirma) return;

        try
        {
            await _atestadoService.RemoverAsync(atestado.Id);
            await CarregarAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Erro", ex.Message);
        }
    }

    private async Task BaixarAsync(AtestadoDto? atestado)
    {
        if (atestado == null) return;

        try
        {
            var (arquivo, nomeArquivo) = await _atestadoService.ExportarArquivoAsync(atestado.Id);
            var extensao = Path.GetExtension(nomeArquivo);
            var filtro = string.IsNullOrEmpty(extensao) ? "Todos os arquivos|*.*" : $"Arquivo|*{extensao}";
            var caminho = await _dialogService.SalvarArquivoAsync(filtro, Path.GetFileNameWithoutExtension(nomeArquivo));
            if (string.IsNullOrEmpty(caminho)) return;

            await File.WriteAllBytesAsync(caminho, arquivo);
            await _dialogService.ShowInfoAsync("Sucesso", "Arquivo salvo com sucesso!");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Erro", ex.Message);
        }
    }
}
