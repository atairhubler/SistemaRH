using SistemaRH.Domain.Enums;

namespace SistemaRH.Application.DTOs;

public class CampoPersonalizadoDto
{
    public int Id { get; set; }
    public string Rotulo { get; set; } = "";
    public TipoCampoPersonalizado Tipo { get; set; }
    public string Opcoes { get; set; } = "";
    public bool Obrigatorio { get; set; }
    public int Ordem { get; set; }
    public TipoFuncionario? AplicavelA { get; set; }
    public bool Ativo { get; set; } = true;

    public string[] OpcoesLista => (Opcoes ?? "")
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public string AplicavelATexto => AplicavelA?.ToString() ?? "Todos os tipos";
}
