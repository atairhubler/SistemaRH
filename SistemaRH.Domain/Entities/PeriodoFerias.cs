using SistemaRH.Domain.Enums;

namespace SistemaRH.Domain.Entities;

public class PeriodoFerias
{
    public int Id { get; set; }
    public int FeriasId { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public int Dias { get; set; }
    public StatusFeria Status { get; set; }
    public bool Remunerada { get; set; } = true;
    public DateTime DataCriacao { get; set; }

    // Navegação
    public Ferias Ferias { get; set; }
}
