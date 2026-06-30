using System.IO;
using System.Text.Json;

namespace SistemaRH.Desktop.Services;

public class ConfiguracaoService
{
    private readonly string _path;
    private AppConfig _config;

    public ConfiguracaoService()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SistemaRH");
        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, "config.json");
        _config = Carregar();
    }

    public bool ModoEscuro
    {
        get => _config.ModoEscuro;
        set { _config.ModoEscuro = value; Salvar(); }
    }

    public string TemaCor
    {
        get => _config.TemaCor;
        set { _config.TemaCor = value; Salvar(); }
    }

    public DateTime? UltimoBackupAutomatico
    {
        get => _config.UltimoBackupAutomatico;
        set { _config.UltimoBackupAutomatico = value; Salvar(); }
    }

    private AppConfig Carregar()
    {
        if (!File.Exists(_path)) return new AppConfig();
        try { return JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(_path)) ?? new AppConfig(); }
        catch { return new AppConfig(); }
    }

    private void Salvar() => File.WriteAllText(_path, JsonSerializer.Serialize(_config));
}

public class AppConfig
{
    public bool ModoEscuro { get; set; } = false;
    public string TemaCor { get; set; } = "Azul";
    public DateTime? UltimoBackupAutomatico { get; set; } = null;
}
