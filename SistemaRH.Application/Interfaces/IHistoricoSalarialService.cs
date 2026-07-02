using SistemaRH.Application.DTOs;

namespace SistemaRH.Application.Interfaces;

public interface IHistoricoSalarialService
{
    Task<IEnumerable<SalarioHistoricoDto>> GetByFuncionarioAsync(int funcionarioId);
    Task<SalarioHistoricoDto> AdicionarAsync(int funcionarioId, DateTime dataVigencia, decimal valor, string motivo, string? observacoes);
}
