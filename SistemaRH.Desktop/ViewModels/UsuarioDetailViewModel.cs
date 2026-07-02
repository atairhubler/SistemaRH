using System.Windows.Input;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Desktop.Services;

namespace SistemaRH.Desktop.ViewModels;

public class UsuarioDetailViewModel : BaseViewModel
{
    private readonly IUsuarioService _usuarioService;
    private readonly IDialogService _dialogService;

    private int _id;
    private bool _isEdit;
    private bool _ativoAtual;

    private string _nomeUsuario = "";
    private string _senha = "";
    private string _confirmarSenha = "";

    public string Titulo => _isEdit ? "Editar Usuário" : "Novo Usuário";
    public string DicaSenha => _isEdit ? "Deixe em branco para manter a senha atual" : "";

    public string NomeUsuario { get => _nomeUsuario; set => SetProperty(ref _nomeUsuario, value); }
    public string Senha { get => _senha; set => SetProperty(ref _senha, value); }
    public string ConfirmarSenha { get => _confirmarSenha; set => SetProperty(ref _confirmarSenha, value); }

    public ICommand SalvarCommand { get; }
    public ICommand CancelarCommand { get; }

    public UsuarioDetailViewModel(IUsuarioService usuarioService, IDialogService dialogService)
    {
        _usuarioService = usuarioService;
        _dialogService = dialogService;

        SalvarCommand = new RelayCommand(_ => _ = SalvarAsync());
        CancelarCommand = new RelayCommand(_ => Cancelar());
    }

    public void PrepararNovo()
    {
        _id = 0;
        _isEdit = false;
        _ativoAtual = true;
        NomeUsuario = ""; Senha = ""; ConfirmarSenha = "";
        OnPropertyChanged(nameof(Titulo));
        OnPropertyChanged(nameof(DicaSenha));
    }

    public void PrepararEdicao(UsuarioDto dto)
    {
        _id = dto.Id;
        _isEdit = true;
        _ativoAtual = dto.Ativo;
        NomeUsuario = dto.NomeUsuario;
        Senha = ""; ConfirmarSenha = "";
        OnPropertyChanged(nameof(Titulo));
        OnPropertyChanged(nameof(DicaSenha));
    }

    private bool ValidarCampos()
    {
        if (string.IsNullOrWhiteSpace(NomeUsuario)) return false;
        if (!_isEdit && string.IsNullOrWhiteSpace(Senha)) return false;
        if (!string.IsNullOrEmpty(Senha) && Senha != ConfirmarSenha) return false;
        return true;
    }

    private async Task SalvarAsync()
    {
        if (!ValidarCampos())
        {
            await _dialogService.ShowErrorAsync("Validação",
                "Informe o nome de usuário e, se for um usuário novo, a senha. Senha e confirmação precisam ser iguais.");
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Salvando usuário...";

            if (_isEdit)
                await _usuarioService.UpdateAsync(_id, NomeUsuario, string.IsNullOrWhiteSpace(Senha) ? null : Senha, _ativoAtual);
            else
                await _usuarioService.AddAsync(NomeUsuario, Senha);

            var acao = _isEdit ? "atualizado" : "cadastrado";
            await _dialogService.ShowInfoAsync("Sucesso", $"Usuário '{NomeUsuario}' {acao} com sucesso!");
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
}
