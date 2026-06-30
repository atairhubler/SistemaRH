using SistemaRH.Domain.Enums;

namespace SistemaRH.Domain.Entities;

public class FuncionarioPJ : Funcionario
{
    public string Cnpj { get; set; }
    public string RazaoSocial { get; set; }
    public bool TemDireitoFerias { get; set; }
    public bool FeriasRemuneradas { get; set; }

    // Navegação
    public ICollection<ContratoPJ> Contratos { get; set; } = new List<ContratoPJ>();
    public ICollection<RpaNfPJ> RpasNfs { get; set; } = new List<RpaNfPJ>();
}
