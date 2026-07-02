namespace SistemaRH.Application.DTOs;

public class FuncionarioPJDto : FuncionarioDto
{
    public string Cnpj { get; set; }
    public string RazaoSocial { get; set; }
    public bool TemDireitoFerias { get; set; }
    public bool FeriasRemuneradas { get; set; }

    public DateTime DataNascimento { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public string ValidadeContrato { get; set; }
    public string TipoServico { get; set; }
    public bool EmiteNF { get; set; }
    public string DiaSolicitacaoNF { get; set; }
    public decimal ValorServico { get; set; }
    public decimal ValorContratado { get; set; }
    public string TelefoneAgil { get; set; }
    public string Departamento { get; set; }
}
