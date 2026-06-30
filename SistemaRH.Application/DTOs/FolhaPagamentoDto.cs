namespace SistemaRH.Application.DTOs;

public class FolhaPagamentoDto
{
    public int FuncionarioId { get; set; }
    public string NomeFuncionario { get; set; } = "";
    public string Cargo { get; set; } = "";
    public int Mes { get; set; }
    public int Ano { get; set; }
    public decimal SalarioBruto { get; set; }
    public int NumeroDependentes { get; set; }
    public decimal DescontoINSS { get; set; }
    public decimal DescontoIRRF { get; set; }
    public decimal FGTS { get; set; }
    public decimal SalarioLiquido { get; set; }
    public decimal CustoTotalEmpresa { get; set; }

    public string Competencia => $"{Mes:D2}/{Ano}";
    public decimal TotalDescontos => DescontoINSS + DescontoIRRF;
}
