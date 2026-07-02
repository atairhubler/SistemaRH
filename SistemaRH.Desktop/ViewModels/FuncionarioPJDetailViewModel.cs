using System.Collections.ObjectModel;
using System.Windows.Input;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Desktop.Services;

namespace SistemaRH.Desktop.ViewModels;

public class FuncionarioPJDetailViewModel : BaseViewModel
{
    private readonly IFuncionarioService _funcionarioService;
    private readonly IEmpresaService _empresaService;
    private readonly IDialogService _dialogService;

    private int _id;
    private bool _isEdit;

    private ObservableCollection<EmpresaDto> _empresas = new();
    private EmpresaDto? _empresaSelecionada;
    private string _razaoSocial = "";
    private string _cnpj = "";
    private string _email = "";
    private string _emailAgil = "";
    private string _contratante = "";
    private string _telefone = "";
    private string _telefoneAgil = "";
    private string _endereco = "";
    private string _complemento = "";
    private string _cidade = "";
    private string _estado = "";
    private string _cep = "";
    private decimal _valorContratado;
    private bool _temDireitoFerias;
    private bool _feriasRemuneradas;
    private DateTime _dataNascimento = DateTime.Now;
    private DateTime? _dataInicio;
    private DateTime? _dataFim;
    private string _validadeContrato = "";
    private string _tipoServico = "";
    private bool _emiteNF;
    private string _diaSolicitacaoNF = "";
    private decimal _valorServico;
    private string _departamento = "";
    private string _genero = "";
    private string _ramal = "";
    private string _dadosBancarios = "";
    private bool _comissionado;
    private string _observacoes = "";
    private decimal _ajudaDeCusto;

    public string Titulo => _isEdit ? "Editar Funcionário PJ" : "Novo Funcionário PJ";
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

    public string RazaoSocial { get => _razaoSocial; set => SetProperty(ref _razaoSocial, value); }
    public string CNPJ { get => _cnpj; set => SetProperty(ref _cnpj, FormatarCnpj(value)); }
    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string EmailAgil { get => _emailAgil; set => SetProperty(ref _emailAgil, value); }
    public string Contratante { get => _contratante; set => SetProperty(ref _contratante, value); }
    public string Telefone { get => _telefone; set => SetProperty(ref _telefone, FormatarTelefone(value)); }
    public string TelefoneAgil { get => _telefoneAgil; set => SetProperty(ref _telefoneAgil, FormatarTelefone(value)); }
    public string Endereco { get => _endereco; set => SetProperty(ref _endereco, value); }
    public string Complemento { get => _complemento; set => SetProperty(ref _complemento, value); }
    public string Cidade { get => _cidade; set => SetProperty(ref _cidade, value); }
    public string Estado { get => _estado; set => SetProperty(ref _estado, value); }
    public string Cep { get => _cep; set => SetProperty(ref _cep, value); }
    public decimal ValorContratado { get => _valorContratado; set => SetProperty(ref _valorContratado, value); }
    public bool TemDireitoFerias { get => _temDireitoFerias; set => SetProperty(ref _temDireitoFerias, value); }
    public bool FeriasRemuneradas { get => _feriasRemuneradas; set => SetProperty(ref _feriasRemuneradas, value); }
    public DateTime DataNascimento { get => _dataNascimento; set => SetProperty(ref _dataNascimento, value); }
    public DateTime? DataInicio { get => _dataInicio; set => SetProperty(ref _dataInicio, value); }
    public DateTime? DataFim { get => _dataFim; set => SetProperty(ref _dataFim, value); }
    public string ValidadeContrato { get => _validadeContrato; set => SetProperty(ref _validadeContrato, value); }
    public string TipoServico { get => _tipoServico; set => SetProperty(ref _tipoServico, value); }
    public bool EmiteNF { get => _emiteNF; set => SetProperty(ref _emiteNF, value); }
    public string DiaSolicitacaoNF { get => _diaSolicitacaoNF; set => SetProperty(ref _diaSolicitacaoNF, value); }
    public decimal ValorServico { get => _valorServico; set => SetProperty(ref _valorServico, value); }
    public string Departamento { get => _departamento; set => SetProperty(ref _departamento, value); }
    public string Genero { get => _genero; set => SetProperty(ref _genero, value); }
    public string Ramal { get => _ramal; set => SetProperty(ref _ramal, value); }
    public string DadosBancarios { get => _dadosBancarios; set => SetProperty(ref _dadosBancarios, value); }
    public bool Comissionado { get => _comissionado; set => SetProperty(ref _comissionado, value); }
    public string Observacoes { get => _observacoes; set => SetProperty(ref _observacoes, value); }
    public decimal AjudaDeCusto { get => _ajudaDeCusto; set => SetProperty(ref _ajudaDeCusto, value); }

    public ICommand SalvarCommand { get; }
    public ICommand CancelarCommand { get; }
    public ICommand AbrirHistoricoCommand { get; }

    public FuncionarioPJDetailViewModel(
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
        RazaoSocial = ""; CNPJ = ""; Email = ""; EmailAgil = ""; Contratante = ""; Telefone = ""; TelefoneAgil = "";
        Endereco = ""; Complemento = ""; Cidade = ""; Estado = ""; Cep = "";
        TemDireitoFerias = false; FeriasRemuneradas = false;
        DataNascimento = DateTime.Now; DataInicio = null; DataFim = null;
        ValidadeContrato = ""; TipoServico = ""; EmiteNF = false; DiaSolicitacaoNF = "";
        ValorServico = 0; ValorContratado = 0; Departamento = "";
        Genero = ""; Ramal = ""; DadosBancarios = ""; Comissionado = false; Observacoes = ""; AjudaDeCusto = 0;
        EmpresaSelecionada = null;
        OnPropertyChanged(nameof(Titulo));
        OnPropertyChanged(nameof(PodeVerHistorico));
        _ = CarregarEmpresasAsync();
    }

    public void PrepararEdicao(FuncionarioPJDto dto)
    {
        _id = dto.Id;
        _isEdit = true;
        RazaoSocial = dto.RazaoSocial ?? dto.Nome ?? "";
        CNPJ = dto.Cnpj ?? "";
        Email = dto.Email ?? "";
        EmailAgil = dto.EmailAgil ?? "";
        Contratante = dto.Contratante ?? "";
        Telefone = dto.Telefone ?? "";
        TelefoneAgil = dto.TelefoneAgil ?? "";
        Endereco = dto.Endereco ?? "";
        Complemento = dto.Complemento ?? "";
        Cidade = dto.Cidade ?? "";
        Estado = dto.Estado ?? "";
        Cep = dto.Cep ?? "";
        TemDireitoFerias = dto.TemDireitoFerias;
        FeriasRemuneradas = dto.FeriasRemuneradas;
        DataNascimento = dto.DataNascimento;
        DataInicio = dto.DataInicio;
        DataFim = dto.DataFim;
        ValidadeContrato = dto.ValidadeContrato ?? "";
        TipoServico = dto.TipoServico ?? "";
        EmiteNF = dto.EmiteNF;
        DiaSolicitacaoNF = dto.DiaSolicitacaoNF ?? "";
        ValorServico = dto.ValorServico;
        ValorContratado = dto.ValorContratado;
        Departamento = dto.Departamento ?? "";
        Genero = dto.Genero ?? "";
        Ramal = dto.Ramal ?? "";
        DadosBancarios = dto.DadosBancarios ?? "";
        Comissionado = dto.Comissionado;
        Observacoes = dto.Observacoes ?? "";
        AjudaDeCusto = dto.AjudaDeCusto;
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
        return !string.IsNullOrWhiteSpace(RazaoSocial) &&
               !string.IsNullOrWhiteSpace(CNPJ) &&
               EmpresaSelecionada != null;
    }

    private async Task SalvarAsync()
    {
        if (!ValidarCampos())
        {
            await _dialogService.ShowErrorAsync("Validação", "Preencha os campos obrigatórios e selecione a empresa.");
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Salvando PJ...";

            var dto = new FuncionarioPJDto
            {
                Nome = RazaoSocial,
                RazaoSocial = RazaoSocial,
                Cnpj = CNPJ,
                Email = Email,
                EmailAgil = EmailAgil,
                Contratante = Contratante,
                Telefone = Telefone,
                TelefoneAgil = TelefoneAgil,
                Endereco = Endereco,
                Complemento = Complemento,
                Cidade = Cidade,
                Estado = Estado,
                Cep = Cep,
                TemDireitoFerias = TemDireitoFerias,
                FeriasRemuneradas = FeriasRemuneradas,
                DataNascimento = DataNascimento,
                DataInicio = DataInicio,
                DataFim = DataFim,
                ValidadeContrato = ValidadeContrato,
                TipoServico = TipoServico,
                EmiteNF = EmiteNF,
                DiaSolicitacaoNF = DiaSolicitacaoNF,
                ValorServico = ValorServico,
                ValorContratado = ValorContratado,
                Departamento = Departamento,
                Genero = Genero,
                Ramal = Ramal,
                DadosBancarios = DadosBancarios,
                Comissionado = Comissionado,
                Observacoes = Observacoes,
                AjudaDeCusto = AjudaDeCusto,
                EmpresaId = EmpresaSelecionada!.Id
            };

            if (_isEdit)
                await _funcionarioService.UpdatePJAsync(_id, dto);
            else
                await _funcionarioService.AddPJAsync(dto);

            var acao = _isEdit ? "atualizada" : "cadastrada";
            await _dialogService.ShowInfoAsync("Sucesso", $"{RazaoSocial} {acao} com sucesso!");
            StatusMessage = $"PJ {acao}";
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
        vm?.Preparar(_id, "Valor do Contrato");
        var dialog = new Views.HistoricoSalarialView();
        dialog.ShowDialog();
        if (vm != null) ValorServico = vm.ValorAtual;
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

    private static string FormatarCnpj(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.Length > 14) digits = digits[..14];

        return digits.Length switch
        {
            <= 2 => digits,
            <= 5 => $"{digits[0..2]}.{digits[2..]}",
            <= 8 => $"{digits[0..2]}.{digits[2..5]}.{digits[5..]}",
            <= 12 => $"{digits[0..2]}.{digits[2..5]}.{digits[5..8]}/{digits[8..]}",
            _ => $"{digits[0..2]}.{digits[2..5]}.{digits[5..8]}/{digits[8..12]}-{digits[12..]}"
        };
    }
}
