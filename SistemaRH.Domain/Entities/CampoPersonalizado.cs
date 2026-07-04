using SistemaRH.Domain.Enums;

namespace SistemaRH.Domain.Entities;

public class CampoPersonalizado
{
    public int Id { get; set; }
    public string Rotulo { get; set; }
    public TipoCampoPersonalizado Tipo { get; set; }
    public string Opcoes { get; set; }
    public bool Obrigatorio { get; set; }
    public int Ordem { get; set; }
    public TipoFuncionario? AplicavelA { get; set; }
    public bool Ativo { get; set; } = true;
}
