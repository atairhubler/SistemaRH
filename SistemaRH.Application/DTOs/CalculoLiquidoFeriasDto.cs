namespace SistemaRH.Application.DTOs;

public class CalculoLiquidoFeriasDto
{
    public decimal SalarioBruto { get; set; }
    public int DiasGozo { get; set; }
    public int DiasAbono { get; set; }
    public int NumeroDependentes { get; set; }

    public decimal ValorFerias { get; set; }
    public decimal TercoFerias { get; set; }
    public decimal BrutoFerias { get; set; }

    public decimal ValorAbono { get; set; }
    public decimal TercoAbono { get; set; }
    public decimal BrutoAbono { get; set; }

    public decimal DescontoINSS { get; set; }
    public decimal DescontoIRRF { get; set; }

    public decimal Liquido { get; set; }
}
