namespace SistemaRH.Application.Interfaces;

public interface IBackupService
{
    Task<bool> CreateBackupAsync(string caminhoDestino);
    Task<bool> CreateAutoBackupAsync(DateTime agora);
    Task<bool> RestoreBackupAsync(string caminhoArquivo);
    Task<string> GetBackupDirectoryAsync();
}
