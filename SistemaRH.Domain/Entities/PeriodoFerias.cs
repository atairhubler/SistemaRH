using SistemaRH.Domain.Enums;

namespace SistemaRH.Domain.Entities;

public class PeriodoFerias
{
    public int Id { get; set; }
    public int FeriasId { get; set; }
    public int? PeriodoAquisitivoId { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public int Dias { get; set; }
    public StatusFeria Status { get; set; }
    public bool Remunerada { get; set; } = true;
    public TipoUsoFerias TipoUso { get; set; } = TipoUsoFerias.Gozo;
    public DateTime DataCriacao { get; set; }

    // Navegação
    public Ferias Ferias { get; set; }
    public PeriodoAquisitivo? PeriodoAquisitivo { get; set; }
}
