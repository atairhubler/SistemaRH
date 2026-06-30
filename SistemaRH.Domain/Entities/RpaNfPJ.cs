using SistemaRH.Domain.Enums;

namespace SistemaRH.Domain.Entities;

public class RpaNfPJ
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
    public StatusPagamentoPJ StatusPagamento { get; set; } = StatusPagamentoPJ.Pendente;
    public DateTime? DataPagamento { get; set; }
    public byte[] Arquivo { get; set; }
    public string NomeArquivo { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }

    // Navegação
    public ContratoPJ Contrato { get; set; }
    public FuncionarioPJ FuncionarioPJ { get; set; }
    public Empresa Empresa { get; set; }
}
