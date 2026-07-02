namespace SistemaRH.Application.DTOs;

public class FuncionarioEstagiarioDto : FuncionarioDto
{
    public string Cpf { get; set; }
    public string Rg { get; set; }
    public string Codigo { get; set; }
    public DateTime DataNascimento { get; set; }
    public string Cargo { get; set; }
    public string Departamento { get; set; }
    public DateTime DataAdmissao { get; set; }
    public DateTime? DataDemissao { get; set; }
    public decimal Bolsa { get; set; }
    public decimal ComplementoSalarial { get; set; }

    public decimal TotalAtual => Bolsa + ComplementoSalarial + AjudaDeCusto;
}
