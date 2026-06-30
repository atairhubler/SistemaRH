namespace SistemaRH.Application.DTOs;

public class FuncionarioPJDto : FuncionarioDto
{
    public string Cnpj { get; set; }
    public string RazaoSocial { get; set; }
    public bool TemDireitoFerias { get; set; }
    public bool FeriasRemuneradas { get; set; }
}
