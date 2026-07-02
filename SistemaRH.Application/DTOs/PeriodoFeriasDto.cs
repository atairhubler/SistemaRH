using SistemaRH.Domain.Enums;

namespace SistemaRH.Application.DTOs;

public class PeriodoFeriasDto
{
    public int Id { get; set; }
    public int FeriasId { get; set; }
    public int? PeriodoAquisitivoId { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public int Dias { get; set; }
    public TipoUsoFerias TipoUso { get; set; }
    public bool Remunerada { get; set; }
    public StatusFeria Status { get; set; }
}
