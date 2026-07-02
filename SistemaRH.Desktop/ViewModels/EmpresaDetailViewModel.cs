using System.Windows.Input;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Desktop.Services;

namespace SistemaRH.Desktop.ViewModels;

public class EmpresaDetailViewModel : BaseViewModel
{
    private readonly IEmpresaService _empresaService;
    private readonly IDialogService _dialogService;
    private readonly int _id;
    private readonly bool _isEdit;

    public event Func<Task>? Salvo;

    private string _razaoSocial = "";
    private string _cnpj = "";
    private string _inscricaoEstadual = "";
    private string _endereco = "";
    private string _cidade = "";
    private string _estado = "";
    private string _cep = "";
    private string _telefone = "";
    private string _email = "";

    public string RazaoSocial { get => _razaoSocial; set => SetProperty(ref _razaoSocial, value); }
    public string Cnpj { get => _cnpj; set => SetProperty(ref _cnpj, value); }
    public string InscricaoEstadual { get => _inscricaoEstadual; set => SetProperty(ref _inscricaoEstadual, value); }
    public string Endereco { get => _endereco; set => SetProperty(ref _endereco, value); }
    public string Cidade { get => _cidade; set => SetProperty(ref _cidade, value); }
    public string Estado { get => _estado; set => SetProperty(ref _estado, value); }
    public string Cep { get => _cep; set => SetProperty(ref _cep, value); }
    public string Telefone { get => _telefone; set => SetProperty(ref _telefone, value); }
    public string Email { get => _email; set => SetProperty(ref _email, value); }

    public string Titulo => _isEdit ? "Editar Empresa" : "Nova Empresa";

    public ICommand SalvarCommand { get; }
    public ICommand CancelarCommand { get; }

    // Construtor para nova empresa
    public EmpresaDetailViewModel(IEmpresaService empresaService, IDialogService dialogService)
    {
        _empresaService = empresaService;
        _dialogService = dialogService;
        _isEdit = false;

        SalvarCommand = new RelayCommand(_ => _ = SalvarAsync(), _ => ValidarCampos());
        CancelarCommand = new RelayCommand(_ => Cancelar());
    }

    // Construtor para editar empresa existente
    public EmpresaDetailViewModel(IEmpresaService empresaService, IDialogService dialogService, EmpresaDto empresa)
        : this(empresaService, dialogService)
    {
        _id = empresa.Id;
        _isEdit = true;

        RazaoSocial = empresa.RazaoSocial;
        Cnpj = empresa.Cnpj;
        InscricaoEstadual = empresa.InscricaoEstadual;
        Endereco = empresa.Endereco;
        Cidade = empresa.Cidade;
        Estado = empresa.Estado;
        Cep = empresa.Cep;
        Telefone = empresa.Telefone;
        Email = empresa.Email;
    }

    private bool ValidarCampos()
    {
        return !string.IsNullOrWhiteSpace(RazaoSocial) &&
               !string.IsNullOrWhiteSpace(Cnpj);
    }

    private async Task SalvarAsync()
    {
        if (!ValidarCampos())
        {
            await _dialogService.ShowErrorAsync("Validação", "Razão Social e CNPJ são obrigatórios.");
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Salvando...";

            var dto = new EmpresaDto
            {
                RazaoSocial = RazaoSocial,
                Cnpj = Cnpj,
                InscricaoEstadual = InscricaoEstadual,
                Endereco = Endereco,
                Cidade = Cidade,
                Estado = Estado,
                Cep = Cep,
                Telefone = Telefone,
                Email = Email
            };

            if (_isEdit)
                await _empresaService.UpdateAsync(_id, dto);
            else
                await _empresaService.AddAsync(dto);

            if (Salvo != null)
                await Salvo.Invoke();

            await _dialogService.ShowInfoAsync("Sucesso", $"Empresa '{RazaoSocial}' salva com sucesso!");
            FecharJanela?.Invoke();
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

    private void Cancelar()
    {
        FecharJanela?.Invoke();
    }

    public Action? FecharJanela { get; set; }
}
