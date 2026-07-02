using SistemaRH.Application.DTOs;

namespace SistemaRH.Application.Interfaces;

public interface IFuncionarioService
{
    Task<IEnumerable<FuncionarioDto>> GetAllAsync();
    Task<IEnumerable<FuncionarioCLTDto>> GetAllCltAsync();
    Task<IEnumerable<FuncionarioPJDto>> GetAllPJAsync();
    Task<IEnumerable<FuncionarioEstagiarioDto>> GetAllEstagiarioAsync();
    Task<FuncionarioDto> GetByIdAsync(int id);
    Task<FuncionarioCLTDto> AddCltAsync(FuncionarioCLTDto dto);
    Task<FuncionarioPJDto> AddPJAsync(FuncionarioPJDto dto);
    Task<FuncionarioEstagiarioDto> AddEstagiarioAsync(FuncionarioEstagiarioDto dto);
    Task<FuncionarioCLTDto> UpdateCltAsync(int id, FuncionarioCLTDto dto);
    Task<FuncionarioPJDto> UpdatePJAsync(int id, FuncionarioPJDto dto);
    Task<FuncionarioEstagiarioDto> UpdateEstagiarioAsync(int id, FuncionarioEstagiarioDto dto);
    Task<bool> AtivarAsync(int id);
    Task<bool> DesativarAsync(int id);
    Task<IEnumerable<FuncionarioDto>> GetByEmpresaAsync(int empresaId);
}
