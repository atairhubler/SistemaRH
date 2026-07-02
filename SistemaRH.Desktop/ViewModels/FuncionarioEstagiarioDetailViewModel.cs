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
    private string _email = "";
    private string _telefone = "";
    private string _cargo = "";
    private string _departamento = "";
    private DateTime _dataAdmissao = DateTime.Now;
    private decimal _bolsa;

    public string Titulo => _isEdit ? "Editar Estagiário" : "Novo Estagiário";

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
    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string Telefone { get => _telefone; set => SetProperty(ref _telefone, FormatarTelefone(value)); }
    public string Cargo { get => _cargo; set => SetProperty(ref _cargo, value); }
    public string Departamento { get => _departamento; set => SetProperty(ref _departamento, value); }
    public DateTime DataAdmissao { get => _dataAdmissao; set => SetProperty(ref _dataAdmissao, value); }
    public decimal Bolsa { get => _bolsa; set => SetProperty(ref _bolsa, value); }

    public ICommand SalvarCommand { get; }
    public ICommand CancelarCommand { get; }

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
    }

    public void PrepararNovo()
    {
        _id = 0;
        _isEdit = false;
        Nome = ""; CPF = ""; RG = ""; Email = ""; Telefone = "";
        Cargo = ""; Departamento = "";
        Bolsa = 0;
        DataAdmissao = DateTime.Now;
        EmpresaSelecionada = null;
        OnPropertyChanged(nameof(Titulo));
        _ = CarregarEmpresasAsync();
    }

    public void PrepararEdicao(FuncionarioEstagiarioDto dto)
    {
        _id = dto.Id;
        _isEdit = true;
        Nome = dto.Nome ?? "";
        CPF = dto.Cpf ?? "";
        RG = dto.Rg ?? "";
        Email = dto.Email ?? "";
        Telefone = dto.Telefone ?? "";
        Cargo = dto.Cargo ?? "";
        Departamento = dto.Departamento ?? "";
        Bolsa = dto.Bolsa;
        DataAdmissao = dto.DataAdmissao;
        OnPropertyChanged(nameof(Titulo));
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
                Email = Email,
                Telefone = Telefone,
                Cargo = Cargo,
                Departamento = Departamento,
                DataAdmissao = DataAdmissao,
                Bolsa = Bolsa,
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
