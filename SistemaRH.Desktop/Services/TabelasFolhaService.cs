using System.IO;
using System.Text.Json;
using SistemaRH.Application.DTOs;

namespace SistemaRH.Desktop.Services;

public class TabelasFolhaService
{
    private readonly string _path;
    private TabelasFolhaConfig _config;

    public TabelasFolhaService()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SistemaRH");
        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, "tabelas_folha.json");
        _config = Carregar();
    }

    public TabelasFolhaConfig GetTabelas() => _config;

    public void Salvar(TabelasFolhaConfig config)
    {
        _config = config;
        File.WriteAllText(_path, JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
    }

    public void RestaurarPadrao() => Salvar(TabelasFolhaConfig.Padrao2024());

    private TabelasFolhaConfig Carregar()
    {
        if (!File.Exists(_path)) return TabelasFolhaConfig.Padrao2024();
        try { return JsonSerializer.Deserialize<TabelasFolhaConfig>(File.ReadAllText(_path)) ?? TabelasFolhaConfig.Padrao2024(); }
        catch { return TabelasFolhaConfig.Padrao2024(); }
    }
}
