namespace SistemaRH.Desktop.Services;

public interface IDialogService
{
    Task ShowInfoAsync(string titulo, string mensagem);
    Task ShowErrorAsync(string titulo, string mensagem);
    Task<bool> ShowConfirmAsync(string titulo, string mensagem);
    Task<string?> SelectTipoFuncionarioAsync();
    Task<string?> SalvarArquivoAsync(string filtro = "Arquivos Excel|*.xlsx", string nomeArquivo = "");
    Task<string?> AbrirArquivoAsync(string filtro = "Todos os arquivos|*.*");
}
