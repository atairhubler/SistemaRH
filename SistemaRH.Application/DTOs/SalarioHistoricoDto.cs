namespace SistemaRH.Application.DTOs;

public class SalarioHistoricoDto
{
    public int Id { get; set; }
    public int FuncionarioId { get; set; }
    public DateTime DataVigencia { get; set; }
    public decimal SalarioBruto { get; set; }
    public string Motivo { get; set; } = "";
    public string Observacoes { get; set; } = "";
}
