using SistemaRH.Application.DTOs;

namespace SistemaRH.Application.Interfaces;

public interface IContratoPJService
{
    Task<ContratoPJDto> CriarAsync(ContratoPJDto dto);
    Task<ContratoPJDto> GetByIdAsync(int id);
    Task<IEnumerable<ContratoPJDto>> GetByFuncionarioAsync(int funcionarioPJId);
    Task<IEnumerable<ContratoPJDto>> GetByEmpresaAsync(int empresaId);
    Task<ContratoPJDto> AtualizarAsync(int id, ContratoPJDto dto);
    Task<bool> DeletarAsync(int id);
    Task<decimal> CalcularCustoMensalAsync(int contratoId, int mes, int ano);
}
