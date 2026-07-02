namespace SistemaRH.Domain.Entities;

public class PeriodoAquisitivo
{
    public int Id { get; set; }
    public int FeriasId { get; set; }
    public int NumeroPeriodo { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public DateTime DataLimiteUso { get; set; }
    public int DiasDireito { get; set; } = 30;
    public DateTime DataCriacao { get; set; }

    // Navegação
    public Ferias Ferias { get; set; }
    public ICollection<PeriodoFerias> Usos { get; set; } = new List<PeriodoFerias>();
}
