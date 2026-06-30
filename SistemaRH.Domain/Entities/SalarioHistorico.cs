namespace SistemaRH.Domain.Entities;

public class SalarioHistorico
{
    public int Id { get; set; }
    public int FuncionarioId { get; set; }
    public DateTime DataVigencia { get; set; }
    public decimal SalarioBruto { get; set; }
    public decimal DescontoINSS { get; set; }
    public decimal DescontoIRPF { get; set; }
    public decimal DescontoOutros { get; set; }
    public decimal SalarioLiquido { get; set; }
    public string Observacoes { get; set; }
    public DateTime DataCriacao { get; set; }

    // Navegação
    public FuncionarioCLT Funcionario { get; set; }
}
