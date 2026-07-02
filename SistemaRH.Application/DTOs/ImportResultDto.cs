namespace SistemaRH.Application.DTOs;

public class ImportResultDto
{
    public int PjImportados { get; set; }
    public int PjIgnorados { get; set; }
    public int CltImportados { get; set; }
    public int CltIgnorados { get; set; }
    public int EstagiarioImportados { get; set; }
    public int EstagiarioIgnorados { get; set; }
    public List<string> Avisos { get; set; } = new();
}
