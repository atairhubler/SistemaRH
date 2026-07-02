using System.Collections.ObjectModel;
using System.Windows.Input;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Desktop.Services;

namespace SistemaRH.Desktop.ViewModels;

public class FuncionarioEstagiarioDetailViewModel : BaseViewModel
{
    private readonly IFuncionarioService _funcionarioService;
    private readonly IEmpresaService _empresaService;
    private readonly IDialogService _dialogService;

    private int _id;
    private bool _isEdit;

    private ObservableCollection<EmpresaDto> _empresas = new();
    private EmpresaDto? _empresaSelecionada;
    private string _nome = "";
    private string _cpf = "";
    private string _rg = "";
    private string _codigo = "";
    private string _email = "";
    private string _emailAgil = "";
    private string _contratante = "";
    private string _telefone = "";
    private string _endereco = "";
    private string _complemento = "";
    private string _cidade = "";
    private string _estado = "";
    private string _cep = "";
    private string _cargo = "";
    private string _departamento = "";
    private DateTime _dataAdmissao = DateTime.Now;
    private DateTime _dataNascimento = DateTime.Now;
    private DateTime? _dataDemissao;
    private decimal _bolsa;
    private decimal _complementoSalarial;
    private string _genero = "";
    private string _ramal = "";
    private string _dadosBancarios = "";
    private bool _comissionado;
    private string _observacoes = "";
    private decimal _ajudaDeCusto;

    public string Titulo => _isEdit ? "Editar Estagiário" : "Novo Estagiário";
    public bool PodeVerHistorico => _isEdit;

    public ObservableCollection<EmpresaDto> Empresas
    {
        get => _empresas;
        set => SetProperty(ref _empresas, value);
    }

    public EmpresaDto? EmpresaSelecionada
    {
        get => _empresaSelecionada;
        set => SetProperty(ref _empresaSelecionada, value);
    }

    public string Nome { get => _nome; set => SetProperty(ref _nome, value); }
    public string CPF { get => _cpf; set => SetProperty(ref _cpf, value); }
    public string RG { get => _rg; set => SetProperty(ref _rg, value); }
    public string Codigo { get => _codigo; set => SetProperty(ref _codigo, value); }
    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string EmailAgil { get => _emailAgil; set => SetProperty(ref _emailAgil, value); }
    public string Contratante { get => _contratante; set => SetProperty(ref _contratante, value); }
    public string Telefone { get => _telefone; set => SetProperty(ref _telefone, FormatarTelefone(value)); }
    public string Endereco { get => _endereco; set => SetProperty(ref _endereco, value); }
    public string Complemento { get => _complemento; set => SetProperty(ref _complemento, value); }
    public string Cidade { get => _cidade; set => SetProperty(ref _cidade, value); }
    public string Estado { get => _estado; set => SetProperty(ref _estado, value); }
    public string Cep { get => _cep; set => SetProperty(ref _cep, value); }
    public string Cargo { get => _cargo; set => SetProperty(ref _cargo, value); }
    public string Departamento { get => _departamento; set => SetProperty(ref _departamento, value); }
    public DateTime DataAdmissao { get => _dataAdmissao; set => SetProperty(ref _dataAdmissao, value); }
    public DateTime DataNascimento { get => _dataNascimento; set => SetProperty(ref _dataNascimento, value); }
    public DateTime? DataDemissao { get => _dataDemissao; set => SetProperty(ref _dataDemissao, value); }
    public decimal Bolsa
    {
        get => _bolsa;
        set { if (SetProperty(ref _bolsa, value)) OnPropertyChanged(nameof(TotalAtual)); }
    }
    public decimal ComplementoSalarial
    {
        get => _complementoSalarial;
        set { if (SetProperty(ref _complementoSalarial, value)) OnPropertyChanged(nameof(TotalAtual)); }
    }
    public decimal TotalAtual => Bolsa + ComplementoSalarial + AjudaDeCusto;
    public string Genero { get => _genero; set => SetProperty(ref _genero, value); }
    public string Ramal { get => _ramal; set => SetProperty(ref _ramal, value); }
    public string DadosBancarios { get => _dadosBancarios; set => SetProperty(ref _dadosBancarios, value); }
    public bool Comissionado { get => _comissionado; set => SetProperty(ref _comissionado, value); }
    public string Observacoes { get => _observacoes; set => SetProperty(ref _observacoes, value); }
    public decimal AjudaDeCusto
    {
        get => _ajudaDeCusto;
        set { if (SetProperty(ref _ajudaDeCusto, value)) OnPropertyChanged(nameof(TotalAtual)); }
    }

    public ICommand SalvarCommand { get; }
    public ICommand CancelarCommand { get; }
    public ICommand AbrirHistoricoCommand { get; }

    public FuncionarioEstagiarioDetailViewModel(
        IFuncionarioService funcionarioService,
        IEmpresaService empresaService,
        IDialogService dialogService)
    {
        _funcionarioService = funcionarioService;
        _empresaService = empresaService;
        _dialogService = dialogService;

        SalvarCommand = new RelayCommand(_ => _ = SalvarAsync());
        CancelarCommand = new RelayCommand(_ => Cancelar());
        AbrirHistoricoCommand = new RelayCommand(_ => AbrirHistorico());
    }

    public void PrepararNovo()
    {
        _id = 0;
        _isEdit = false;
        Nome = ""; CPF = ""; RG = ""; Codigo = ""; Email = ""; EmailAgil = ""; Contratante = ""; Telefone = "";
        Endereco = ""; Complemento = ""; Cidade = ""; Estado = ""; Cep = "";
        Cargo = ""; Departamento = "";
        Genero = ""; Ramal = ""; DadosBancarios = ""; Comissionado = false; Observacoes = ""; AjudaDeCusto = 0;
        Bolsa = 0; ComplementoSalarial = 0;
        DataAdmissao = DateTime.Now;
        DataNascimento = DateTime.Now;
        DataDemissao = null;
        EmpresaSelecionada = null;
        OnPropertyChanged(nameof(Titulo));
        OnPropertyChanged(nameof(PodeVerHistorico));
        _ = CarregarEmpresasAsync();
    }

    public void PrepararEdicao(FuncionarioEstagiarioDto dto)
    {
        _id = dto.Id;
        _isEdit = true;
        Nome = dto.Nome ?? "";
        CPF = dto.Cpf ?? "";
        RG = dto.Rg ?? "";
        Codigo = dto.Codigo ?? "";
        Email = dto.Email ?? "";
        EmailAgil = dto.EmailAgil ?? "";
        Contratante = dto.Contratante ?? "";
        Telefone = dto.Telefone ?? "";
        Endereco = dto.Endereco ?? "";
        Complemento = dto.Complemento ?? "";
        Cidade = dto.Cidade ?? "";
        Estado = dto.Estado ?? "";
        Cep = dto.Cep ?? "";
        Cargo = dto.Cargo ?? "";
        Departamento = dto.Departamento ?? "";
        Genero = dto.Genero ?? "";
        Ramal = dto.Ramal ?? "";
        DadosBancarios = dto.DadosBancarios ?? "";
        Comissionado = dto.Comissionado;
        Observacoes = dto.Observacoes ?? "";
        AjudaDeCusto = dto.AjudaDeCusto;
        Bolsa = dto.Bolsa;
        ComplementoSalarial = dto.ComplementoSalarial;
        DataAdmissao = dto.DataAdmissao;
        DataNascimento = dto.DataNascimento;
        DataDemissao = dto.DataDemissao;
        OnPropertyChanged(nameof(Titulo));
        OnPropertyChanged(nameof(PodeVerHistorico));
        _ = CarregarEmpresasAsync(dto.EmpresaId);
    }

    private async Task CarregarEmpresasAsync(int empresaIdParaSelecionar = 0)
    {
        try
        {
            var lista = await _empresaService.GetAllAsync();
            Empresas = new ObservableCollection<EmpresaDto>(lista);

            if (empresaIdParaSelecionar > 0)
                EmpresaSelecionada = Empresas.FirstOrDefault(e => e.Id == empresaIdParaSelecionar);
            else if (Empresas.Count == 1)
                EmpresaSelecionada = Empresas[0];
        }
        catch { }
    }

    private bool ValidarCampos()
    {
        return !string.IsNullOrWhiteSpace(Nome) &&
               !string.IsNullOrWhiteSpace(CPF) &&
               !string.IsNullOrWhiteSpace(Cargo) &&
               EmpresaSelecionada != null;
    }

    private async Task SalvarAsync()
    {
        if (!ValidarCampos())
        {
            await _dialogService.ShowErrorAsync("Validação", "Preencha todos os campos obrigatórios e selecione a empresa.");
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Salvando estagiário...";

            var dto = new FuncionarioEstagiarioDto
            {
                Nome = Nome,
                Cpf = CPF,
                Rg = RG,
                Codigo = Codigo,
                Email = Email,
                EmailAgil = EmailAgil,
                Contratante = Contratante,
                Telefone = Telefone,
                Endereco = Endereco,
                Complemento = Complemento,
                Cidade = Cidade,
                Estado = Estado,
                Cep = Cep,
                Cargo = Cargo,
                Departamento = Departamento,
                DataAdmissao = DataAdmissao,
                DataNascimento = DataNascimento,
                DataDemissao = DataDemissao,
                Bolsa = Bolsa,
                ComplementoSalarial = ComplementoSalarial,
                Genero = Genero,
                Ramal = Ramal,
                DadosBancarios = DadosBancarios,
                Comissionado = Comissionado,
                Observacoes = Observacoes,
                AjudaDeCusto = AjudaDeCusto,
                EmpresaId = EmpresaSelecionada!.Id
            };

            if (_isEdit)
                await _funcionarioService.UpdateEstagiarioAsync(_id, dto);
            else
                await _funcionarioService.AddEstagiarioAsync(dto);

            var acao = _isEdit ? "atualizado" : "cadastrado";
            await _dialogService.ShowInfoAsync("Sucesso", $"{Nome} {acao} com sucesso!");
            StatusMessage = $"Estagiário {acao}";
            FecharJanela?.Invoke();
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

    private void AbrirHistorico()
    {
        if (!_isEdit || _id == 0) return;
        var vm = App.ServiceProvider.GetService(typeof(HistoricoSalarialViewModel)) as HistoricoSalarialViewModel;
        vm?.Preparar(_id, "Bolsa");
        var dialog = new Views.HistoricoSalarialView();
        dialog.ShowDialog();
        if (vm != null) Bolsa = vm.ValorAtual;
    }

    private void Cancelar() => FecharJanela?.Invoke();

    public Action? FecharJanela { get; set; }

    private static string FormatarTelefone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        var digits = new string(value.Where(char.IsDigit).ToArray());
        return digits.Length switch
        {
            11 => $"({digits[0..2]}) {digits[2..7]}-{digits[7..11]}",
            10 => $"({digits[0..2]}) {digits[2..6]}-{digits[6..10]}",
            _ => value
        };
    }
}
