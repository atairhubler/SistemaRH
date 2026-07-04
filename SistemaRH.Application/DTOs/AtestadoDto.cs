namespace SistemaRH.Application.DTOs;

public class AtestadoDto
{
    public int Id { get; set; }
    public int FuncionarioId { get; set; }
    public DateTime DataAtestado { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public int DiasFaltados { get; set; }
    public string Tipo { get; set; } = "";
    public string Descricao { get; set; } = "";
    public string Cid { get; set; } = "";
    public string Observacoes { get; set; } = "";
    public string NomeArquivo { get; set; } = "";
    public DateTime DataCriacao { get; set; }
}
