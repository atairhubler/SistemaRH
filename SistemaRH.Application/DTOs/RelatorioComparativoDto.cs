namespace SistemaRH.Application.DTOs;

public class RelatorioComparativoDto
{
    public int Mes { get; set; }
    public int Ano { get; set; }
    public int QuantidadeCLT { get; set; }
    public int QuantidadePJ { get; set; }
    public decimal CustosCLT { get; set; }
    public decimal CustosPJ { get; set; }
    public decimal CustoTotal => CustosCLT + CustosPJ;
    public decimal DiferencaAbsoluta => CustosCLT - CustosPJ;
    public decimal PercentualPJ => CustoTotal > 0 ? (CustosPJ / CustoTotal) * 100 : 0;
    public decimal CustoMedioCLT => QuantidadeCLT > 0 ? CustosCLT / QuantidadeCLT : 0;
    public decimal CustoMedioPJ => QuantidadePJ > 0 ? CustosPJ / QuantidadePJ : 0;
    public bool MaisCustosoEhCLT => DiferencaAbsoluta > 0;
    public DateTime DataGeracao { get; set; }
}
