namespace SistemaRH.Application.Interfaces;

public interface IGoogleDriveService
{
    Task<bool> BackupDatabaseAsync(string filePath, string backupName);
    Task<bool> AuthenticateAsync();
    Task<bool> IsAuthenticatedAsync();
}
