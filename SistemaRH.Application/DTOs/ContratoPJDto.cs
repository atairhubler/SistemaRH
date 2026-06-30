using SistemaRH.Domain.Enums;

namespace SistemaRH.Application.DTOs;

public class ContratoPJDto
{
    public int Id { get; set; }
    public int FuncionarioPJId { get; set; }
    public int EmpresaId { get; set; }
    public TipoContratoPJ TipoContrato { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public decimal? ValorProjeto { get; set; }
    public decimal? ValorHora { get; set; }
    public decimal? ValorFixo { get; set; }
    public int HorasMensais { get; set; }
    public string Descricao { get; set; }
    public string RpaBrancoConhecimento { get; set; }
}
