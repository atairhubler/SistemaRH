using SistemaRH.Application.Interfaces;
using SistemaRH.Data;

namespace SistemaRH.Desktop.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly IUsuarioService _usuarioService;

    private string _nomeUsuario = "";
    private string _mensagemErro = "";

    public string NomeUsuario { get => _nomeUsuario; set => SetProperty(ref _nomeUsuario, value); }
    public string MensagemErro { get => _mensagemErro; set => SetProperty(ref _mensagemErro, value); }

    public bool LoginBemSucedido { get; private set; }
    public Action? FecharJanela { get; set; }

    public LoginViewModel(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    public async Task EntrarAsync(string senha)
    {
        if (string.IsNullOrWhiteSpace(NomeUsuario) || string.IsNullOrWhiteSpace(senha))
        {
            MensagemErro = "Informe usuário e senha.";
            return;
        }

        var usuario = await _usuarioService.ValidarCredenciaisAsync(NomeUsuario.Trim(), senha);
        if (usuario == null)
        {
            MensagemErro = "Usuário ou senha inválidos.";
            return;
        }

        SessaoAtual.UsuarioAtual = usuario.NomeUsuario;
        LoginBemSucedido = true;
        FecharJanela?.Invoke();
    }
}
