using SistemaRH.Domain.Enums;

namespace SistemaRH.Application.DTOs;

public class RpaNfDto
{
    public int Id { get; set; }
    public int ContratoPJId { get; set; }
    public int FuncionarioPJId { get; set; }
    public int EmpresaId { get; set; }
    public TipoRpaNf TipoDocumento { get; set; }
    public string NumeroDocumento { get; set; }
    public DateTime DataDocumento { get; set; }
    public DateTime DataVencimento { get; set; }
    public int Mes { get; set; }
    public int Ano { get; set; }
    public decimal ValorBruto { get; set; }
    public decimal ValorDescontos { get; set; }
    public decimal ValorLiquido { get; set; }
    public string DescricaoServico { get; set; }
    public string Observacoes { get; set; }
    public StatusPagamentoPJ StatusPagamento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public string NomeArquivo { get; set; }
}
