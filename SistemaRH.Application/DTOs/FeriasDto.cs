namespace SistemaRH.Application.DTOs;

public class FeriasDto
{
    public int Id { get; set; }
    public int FuncionarioId { get; set; }
    public int DiasDisponiveis { get; set; }
    public int DiasUtilizados { get; set; }
    public int DiasRemunerados { get; set; }
    public int DiasNaoRemunerados { get; set; }
    public int SaldoDisponivel => DiasDisponiveis - DiasUtilizados;
    public DateTime? DataUltimaRenovacao { get; set; }
}
