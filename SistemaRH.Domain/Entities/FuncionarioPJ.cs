using SistemaRH.Domain.Enums;

namespace SistemaRH.Domain.Entities;

public class FuncionarioPJ : Funcionario
{
    public string Cnpj { get; set; }
    public string RazaoSocial { get; set; }
    public bool TemDireitoFerias { get; set; }
    public bool FeriasRemuneradas { get; set; }

    public DateTime DataNascimento { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public string ValidadeContrato { get; set; }
    public string TipoServico { get; set; }
    public bool EmiteNF { get; set; }
    public string DiaSolicitacaoNF { get; set; }
    public decimal ValorServico { get; set; }
    public decimal ValorContratado { get; set; }
    public string TelefoneAgil { get; set; }
    public string Departamento { get; set; }

    // Navegação
    public ICollection<ContratoPJ> Contratos { get; set; } = new List<ContratoPJ>();
    public ICollection<RpaNfPJ> RpasNfs { get; set; } = new List<RpaNfPJ>();
}
