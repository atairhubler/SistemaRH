using System.Windows;
using Microsoft.Win32;
using SistemaRH.Desktop.Views;

namespace SistemaRH.Desktop.Services;

public class DialogService : IDialogService
{
    public Task ShowInfoAsync(string titulo, string mensagem)
    {
        MessageBox.Show(mensagem, titulo, MessageBoxButton.OK, MessageBoxImage.Information);
        return Task.CompletedTask;
    }

    public Task ShowErrorAsync(string titulo, string mensagem)
    {
        MessageBox.Show(mensagem, titulo, MessageBoxButton.OK, MessageBoxImage.Error);
        return Task.CompletedTask;
    }

    public Task<bool> ShowConfirmAsync(string titulo, string mensagem)
    {
        var result = MessageBox.Show(mensagem, titulo, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return Task.FromResult(result == MessageBoxResult.Yes);
    }

    public Task<string?> SelectTipoFuncionarioAsync()
    {
        var dialog = new SelectTipoFuncionarioDialog();
        var result = dialog.ShowDialog();
        return Task.FromResult(result == true ? dialog.SelectedTipo : null);
    }

    public Task<string?> SalvarArquivoAsync(string filtro = "Arquivos Excel|*.xlsx", string nomeArquivo = "")
    {
        var ext = filtro.Contains("*.")
            ? "." + filtro.Split("*.")[1].Split('|')[0]
            : ".xlsx";

        var dialog = new SaveFileDialog
        {
            Filter = filtro,
            DefaultExt = ext,
            FileName = string.IsNullOrEmpty(nomeArquivo)
                ? $"Funcionarios_{DateTime.Now:yyyyMMdd_HHmmss}"
                : nomeArquivo
        };

        var result = dialog.ShowDialog();
        return Task.FromResult(result == true ? dialog.FileName : null);
    }

    public Task<string?> AbrirArquivoAsync(string filtro = "Todos os arquivos|*.*")
    {
        var dialog = new OpenFileDialog { Filter = filtro };
        var result = dialog.ShowDialog();
        return Task.FromResult(result == true ? dialog.FileName : null);
    }
}
