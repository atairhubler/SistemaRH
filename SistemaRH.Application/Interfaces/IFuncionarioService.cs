using SistemaRH.Application.DTOs;

namespace SistemaRH.Application.Interfaces;

public interface IFuncionarioService
{
    Task<IEnumerable<FuncionarioDto>> GetAllAsync();
    Task<IEnumerable<FuncionarioCLTDto>> GetAllCltAsync();
    Task<IEnumerable<FuncionarioPJDto>> GetAllPJAsync();
    Task<FuncionarioDto> GetByIdAsync(int id);
    Task<FuncionarioCLTDto> AddCltAsync(FuncionarioCLTDto dto);
    Task<FuncionarioPJDto> AddPJAsync(FuncionarioPJDto dto);
    Task<FuncionarioCLTDto> UpdateCltAsync(int id, FuncionarioCLTDto dto);
    Task<FuncionarioPJDto> UpdatePJAsync(int id, FuncionarioPJDto dto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<FuncionarioDto>> GetByEmpresaAsync(int empresaId);
}
