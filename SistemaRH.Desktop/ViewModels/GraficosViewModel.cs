using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Desktop.Services;
using SistemaRH.Desktop.Views.Controls;
using WpfApp = System.Windows.Application;

namespace SistemaRH.Desktop.ViewModels;

public class GraficosViewModel : BaseViewModel
{
    private static readonly EmpresaDto TodasAsEmpresas = new() { Id = 0, RazaoSocial = "Todas as Empresas" };

    private readonly IRelatorioService _relatorioService;
    private readonly IEmpresaService _empresaService;
    private readonly IDialogService _dialogService;

    private List<TendenciaMensalItemDto> _tendenciaAtual = new();

    private ObservableCollection<EmpresaDto> _empresas = new();
    private EmpresaDto? _empresaSelecionada;
    private OpcaoPeriodo _periodoSelecionado;
    private bool _mostrarQuadro = true;
    private bool _mostrarCusto = true;
    private bool _mostrarCLT = true;
    private bool _mostrarPJ = true;
    private bool _mostrarEstagiario = true;
    private List<ChartSerie> _seriesQuadro = new();
    private List<ChartSerie> _seriesCusto = new();
    private List<string> _rotulosTendencia = new();

    public ObservableCollection<EmpresaDto> Empresas
    {
        get => _empresas;
        set => SetProperty(ref _empresas, value);
    }

    public EmpresaDto? EmpresaSelecionada
    {
        get => _empresaSelecionada;
        set
        {
            if (SetProperty(ref _empresaSelecionada, value))
                _ = AtualizarTendenciaAsync();
        }
    }

    public List<OpcaoPeriodo> OpcoesPeriodo { get; } = new()
    {
        new() { Meses = 3, Rotulo = "Últimos 3 meses" },
        new() { Meses = 6, Rotulo = "Últimos 6 meses" },
        new() { Meses = 12, Rotulo = "Últimos 12 meses" },
        new() { Meses = 24, Rotulo = "Últimos 24 meses" }
    };

    public OpcaoPeriodo PeriodoSelecionado
    {
        get => _periodoSelecionado;
        set
        {
            if (SetProperty(ref _periodoSelecionado, value))
                _ = AtualizarTendenciaAsync();
        }
    }

    public bool MostrarQuadro { get => _mostrarQuadro; set => SetProperty(ref _mostrarQuadro, value); }
    public bool MostrarCusto { get => _mostrarCusto; set => SetProperty(ref _mostrarCusto, value); }

    public bool MostrarCLT
    {
        get => _mostrarCLT;
        set { if (SetProperty(ref _mostrarCLT, value)) ReconstruirSeries(); }
    }

    public bool MostrarPJ
    {
        get => _mostrarPJ;
        set { if (SetProperty(ref _mostrarPJ, value)) ReconstruirSeries(); }
    }

    public bool MostrarEstagiario
    {
        get => _mostrarEstagiario;
        set { if (SetProperty(ref _mostrarEstagiario, value)) ReconstruirSeries(); }
    }

    public List<ChartSerie> SeriesQuadro
    {
        get => _seriesQuadro;
        set => SetProperty(ref _seriesQuadro, value);
    }

    public List<ChartSerie> SeriesCusto
    {
        get => _seriesCusto;
        set => SetProperty(ref _seriesCusto, value);
    }

    public List<string> RotulosTendencia
    {
        get => _rotulosTendencia;
        set => SetProperty(ref _rotulosTendencia, value);
    }

    public ICommand LoadedCommand { get; }

    public GraficosViewModel(
        IRelatorioService relatorioService,
        IEmpresaService empresaService,
        IDialogService dialogService)
    {
        _relatorioService = relatorioService;
        _empresaService = empresaService;
        _dialogService = dialogService;

        _periodoSelecionado = OpcoesPeriodo[1];

        LoadedCommand = new RelayCommand(_ => _ = CarregarAsync());
    }

    public async Task CarregarAsync()
    {
        try
        {
            IsLoading = true;

            var lista = await _empresaService.GetAllAsync();
            var combinado = new ObservableCollection<EmpresaDto> { TodasAsEmpresas };
            foreach (var empresa in lista) combinado.Add(empresa);
            Empresas = combinado;
            _empresaSelecionada = TodasAsEmpresas;
            OnPropertyChanged(nameof(EmpresaSelecionada));

            await AtualizarTendenciaAsync();
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

    private async Task AtualizarTendenciaAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Carregando gráficos...";

            var empresaId = EmpresaSelecionada?.Id ?? 0;
            var meses = PeriodoSelecionado?.Meses ?? 6;

            _tendenciaAtual = await _relatorioService.GetTendenciaMensalAsync(empresaId, meses);
            RotulosTendencia = _tendenciaAtual.Select(t => t.Rotulo).ToList();
            ReconstruirSeries();

            StatusMessage = "Gráficos atualizados";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro ao carregar gráficos: {ex.Message}";
            await _dialogService.ShowErrorAsync("Erro", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ReconstruirSeries()
    {
        var corCLT = ObterBrush("AccentGreenBorder");
        var corPJ = ObterBrush("AccentPinkBorder");
        var corEstagiario = ObterBrush("AccentBlueBorder");

        var seriesQuadro = new List<ChartSerie>();
        if (MostrarCLT)
            seriesQuadro.Add(new() { Nome = "CLT", Cor = corCLT, Valores = _tendenciaAtual.Select(t => (double)t.TotalCLT).ToList() });
        if (MostrarPJ)
            seriesQuadro.Add(new() { Nome = "PJ", Cor = corPJ, Valores = _tendenciaAtual.Select(t => (double)t.TotalPJ).ToList() });
        if (MostrarEstagiario)
            seriesQuadro.Add(new() { Nome = "Estagiário", Cor = corEstagiario, Valores = _tendenciaAtual.Select(t => (double)t.TotalEstagiario).ToList() });
        SeriesQuadro = seriesQuadro;

        var seriesCusto = new List<ChartSerie>();
        if (MostrarCLT)
            seriesCusto.Add(new() { Nome = "Custo CLT", Cor = corCLT, Valores = _tendenciaAtual.Select(t => (double)t.CustoCLT).ToList() });
        if (MostrarPJ)
            seriesCusto.Add(new() { Nome = "Custo PJ", Cor = corPJ, Valores = _tendenciaAtual.Select(t => (double)t.CustoPJ).ToList() });
        SeriesCusto = seriesCusto;
    }

    private static Brush ObterBrush(string chave) =>
        WpfApp.Current.Resources[chave] as Brush ?? Brushes.Gray;
}
