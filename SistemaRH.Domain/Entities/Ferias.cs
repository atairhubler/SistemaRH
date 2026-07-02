namespace SistemaRH.Domain.Entities;

public class Ferias
{
    public int Id { get; set; }
    public int FuncionarioId { get; set; }
    public DateTime? DataAdmissao { get; set; }
    public int DiasDisponiveis { get; set; }
    public int DiasUtilizados { get; set; }
    public int DiasRemunerados { get; set; }
    public int DiasNaoRemunerados { get; set; }
    public DateTime? DataUltimaRenovacao { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }

    // Navegação
    public Funcionario Funcionario { get; set; }
    public ICollection<PeriodoFerias> Periodos { get; set; } = new List<PeriodoFerias>();
    public ICollection<PeriodoAquisitivo> PeriodosAquisitivos { get; set; } = new List<PeriodoAquisitivo>();
}
