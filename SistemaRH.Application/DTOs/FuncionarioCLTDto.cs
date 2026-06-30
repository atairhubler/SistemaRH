namespace SistemaRH.Application.DTOs;

public class FuncionarioCLTDto : FuncionarioDto
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
}
