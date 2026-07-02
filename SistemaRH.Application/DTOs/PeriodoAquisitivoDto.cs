using SistemaRH.Domain.Enums;

namespace SistemaRH.Application.DTOs;

public class PeriodoAquisitivoDto
{
    public int Id { get; set; }
    public int FeriasId { get; set; }
    public int NumeroPeriodo { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public DateTime DataLimiteUso { get; set; }
    public int DiasDireito { get; set; }
    public int DiasUsufruidos { get; set; }
    public int DiasAbono { get; set; }
    public StatusPeriodoAquisitivo Status { get; set; }

    public int DiasPendentes => Math.Max(0, DiasDireito - DiasUsufruidos - DiasAbono);
}
