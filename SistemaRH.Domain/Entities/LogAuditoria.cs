namespace SistemaRH.Domain.Entities;

public class LogAuditoria
{
    public int Id { get; set; }
    public DateTime DataHora { get; set; }
    public string Usuario { get; set; }
    public string Acao { get; set; }
    public string Entidade { get; set; }
    public string Descricao { get; set; }
    public string? ValoresAntes { get; set; }
    public string? ValoresDepois { get; set; }
}
