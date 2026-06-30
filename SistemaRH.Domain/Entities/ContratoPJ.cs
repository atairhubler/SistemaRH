using SistemaRH.Domain.Enums;

namespace SistemaRH.Domain.Entities;

public class ContratoPJ
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
    public int HorasMensais { get; set; } = 160;
    public string Descricao { get; set; }
    public string RpaBrancoConhecimento { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }

    // Navegação
    public FuncionarioPJ FuncionarioPJ { get; set; }
    public Empresa Empresa { get; set; }
    public ICollection<RpaNfPJ> RpasNfs { get; set; } = new List<RpaNfPJ>();
}
