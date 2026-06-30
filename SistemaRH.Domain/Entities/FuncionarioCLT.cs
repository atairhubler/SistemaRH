using SistemaRH.Domain.Enums;

namespace SistemaRH.Domain.Entities;

public class FuncionarioCLT : Funcionario
{
    public string Cpf { get; set; }
    public DateTime DataNascimento { get; set; }
    public string Cargo { get; set; }
    public string Departamento { get; set; }
    public DateTime DataAdmissao { get; set; }
    public DateTime? DataDemissao { get; set; }
    public string Ctps { get; set; }
    public string PisPassep { get; set; }
    public decimal SalarioBruto { get; set; }

    // Navegação
    public ICollection<SalarioHistorico> HistoricoSalario { get; set; } = new List<SalarioHistorico>();
}
