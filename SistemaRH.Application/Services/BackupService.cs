using System.IO.Compression;
using SistemaRH.Application.Interfaces;

namespace SistemaRH.Application.Services;

public class BackupService : IBackupService
{
    private readonly string _appDataDir;
    private readonly string _backupDirectory;
    private readonly string _databasePath;
    private readonly string _configPath;
    private readonly string _tabelasPath;

    public BackupService()
    {
        _appDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SistemaRH");

        _databasePath = Path.Combine(_appDataDir, "sistemarh.db");
        _configPath   = Path.Combine(_appDataDir, "config.json");
        _tabelasPath  = Path.Combine(_appDataDir, "tabelas_folha.json");

        _backupDirectory = Path.Combine(_appDataDir, "Backups");
    }

    public async Task<bool> CreateBackupAsync(string caminhoDestino)
    {
        try
        {
            if (!File.Exists(_databasePath))
                return false;

            Directory.CreateDirectory(Path.GetDirectoryName(caminhoDestino) ?? _backupDirectory);
            await Task.Run(() => CriarZip(caminhoDestino));
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RestoreBackupAsync(string caminhoArquivo)
    {
        try
        {
            if (!File.Exists(caminhoArquivo))
                return false;

            // Backup de segurança do estado atual antes de substituir
            Directory.CreateDirectory(_backupDirectory);
            var seguranca = Path.Combine(_backupDirectory,
                $"sistemarh_antes_restauracao_{DateTime.Now:yyyyMMdd_HHmmss}.zip");
            await Task.Run(() => CriarZip(seguranca));

            await Task.Run(() =>
            {
                var ext = Path.GetExtension(caminhoArquivo).ToLowerInvariant();
                if (ext == ".zip")
                {
                    using var zip = ZipFile.OpenRead(caminhoArquivo);
                    foreach (var entry in zip.Entries)
                    {
                        if (string.Equals(entry.Name, "sistemarh.db", StringComparison.OrdinalIgnoreCase))
                        {
                            // O EF Core mantém o arquivo aberto — salva em local temporário
                            // e o App.xaml.cs aplica a substituição antes de abrir o banco.
                            entry.ExtractToFile(_databasePath + ".restore", overwrite: true);
                        }
                        else
                        {
                            var destino = Path.Combine(_appDataDir, entry.Name);
                            entry.ExtractToFile(destino, overwrite: true);
                        }
                    }
                }
                else // .db — formato antigo, restaura só o banco
                {
                    CopiarComPartilha(caminhoArquivo, _databasePath + ".restore");
                }

                // Marcador: na próxima inicialização, aplica o .restore antes do EF Core abrir o banco
                File.WriteAllText(Path.Combine(_appDataDir, "pending_restore.flag"), "1");
            });

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CreateAutoBackupAsync(DateTime agora)
    {
        try
        {
            if (!File.Exists(_databasePath)) return false;
            Directory.CreateDirectory(_backupDirectory);
            var zipPath = Path.Combine(_backupDirectory,
                $"sistemarh_auto_{agora:yyyyMMdd_HHmmss}.zip");
            await Task.Run(() => CriarZip(zipPath));
            LimparBackupsAntigosAuto();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public Task<string> GetBackupDirectoryAsync()
    {
        return Task.FromResult(_backupDirectory);
    }

    private void LimparBackupsAntigosAuto()
    {
        var arquivos = Directory.GetFiles(_backupDirectory, "sistemarh_auto_*.zip")
            .OrderByDescending(f => f)
            .Skip(6);
        foreach (var f in arquivos)
            try { File.Delete(f); } catch { }
    }

    // Abre cada arquivo com FileShare.ReadWrite para não conflitar com o
    // processo do SQLite que mantém o .db aberto enquanto a aplicação roda.
    private void CriarZip(string zipPath)
    {
        using var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
        AdicionarEntrada(zip, _databasePath, "sistemarh.db");
        AdicionarEntrada(zip, _configPath,   "config.json");
        AdicionarEntrada(zip, _tabelasPath,  "tabelas_folha.json");
    }

    private static void AdicionarEntrada(ZipArchive zip, string caminho, string nomeEntrada)
    {
        if (!File.Exists(caminho)) return;
        var entry = zip.CreateEntry(nomeEntrada, CompressionLevel.Optimal);
        using var entryStream = entry.Open();
        using var fs = new FileStream(caminho, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        fs.CopyTo(entryStream);
    }

    private static void CopiarComPartilha(string origem, string destino)
    {
        using var fs = new FileStream(origem, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var fd = new FileStream(destino, FileMode.Create, FileAccess.Write, FileShare.None);
        fs.CopyTo(fd);
    }
}
