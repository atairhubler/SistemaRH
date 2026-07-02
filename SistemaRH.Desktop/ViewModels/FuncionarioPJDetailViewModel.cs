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
    private string _telefone = "";
    private bool _temDireitoFerias;
    private bool _feriasRemuneradas;

    public string Titulo => _isEdit ? "Editar Funcionário PJ" : "Novo Funcionário PJ";

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
    public string Telefone { get => _telefone; set => SetProperty(ref _telefone, FormatarTelefone(value)); }
    public bool TemDireitoFerias { get => _temDireitoFerias; set => SetProperty(ref _temDireitoFerias, value); }
    public bool FeriasRemuneradas { get => _feriasRemuneradas; set => SetProperty(ref _feriasRemuneradas, value); }

    public ICommand SalvarCommand { get; }
    public ICommand CancelarCommand { get; }

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
    }

    public void PrepararNovo()
    {
        _id = 0;
        _isEdit = false;
        RazaoSocial = ""; CNPJ = ""; Email = ""; Telefone = "";
        TemDireitoFerias = false; FeriasRemuneradas = false;
        EmpresaSelecionada = null;
        OnPropertyChanged(nameof(Titulo));
        _ = CarregarEmpresasAsync();
    }

    public void PrepararEdicao(FuncionarioPJDto dto)
    {
        _id = dto.Id;
        _isEdit = true;
        RazaoSocial = dto.RazaoSocial ?? dto.Nome ?? "";
        CNPJ = dto.Cnpj ?? "";
        Email = dto.Email ?? "";
        Telefone = dto.Telefone ?? "";
        TemDireitoFerias = dto.TemDireitoFerias;
        FeriasRemuneradas = dto.FeriasRemuneradas;
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
                Telefone = Telefone,
                TemDireitoFerias = TemDireitoFerias,
                FeriasRemuneradas = FeriasRemuneradas,
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
