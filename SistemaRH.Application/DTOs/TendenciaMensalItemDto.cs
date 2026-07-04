namespace SistemaRH.Application.DTOs;

public class TendenciaMensalItemDto
{
    public int Mes { get; set; }
    public int Ano { get; set; }
    public string Rotulo { get; set; } = "";
    public int TotalCLT { get; set; }
    public int TotalPJ { get; set; }
    public int TotalEstagiario { get; set; }
    public decimal CustoCLT { get; set; }
    public decimal CustoPJ { get; set; }
}
