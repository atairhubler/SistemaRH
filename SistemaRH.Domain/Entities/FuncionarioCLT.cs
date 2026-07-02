using SistemaRH.Domain.Enums;

namespace SistemaRH.Domain.Entities;

public class FuncionarioCLT : Funcionario
{
    public string Cpf { get; set; }
    public string Rg { get; set; }
    public string Codigo { get; set; }
    public DateTime DataNascimento { get; set; }
    public string Cargo { get; set; }
    public string Departamento { get; set; }
    public DateTime DataAdmissao { get; set; }
    public DateTime? DataDemissao { get; set; }
    public string Ctps { get; set; }
    public string PisPassep { get; set; }
    public decimal SalarioBruto { get; set; }
    public decimal ComplementoSalarial { get; set; }
    public decimal AuxilioEducacao { get; set; }
}
