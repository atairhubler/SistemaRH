namespace SistemaRH.Application.DTOs;

public class FuncionarioDeFeriasNoMesDto
{
    public int FuncionarioId { get; set; }
    public string FuncionarioNome { get; set; } = "";
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
}
