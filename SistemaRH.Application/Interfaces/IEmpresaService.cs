using SistemaRH.Application.DTOs;

namespace SistemaRH.Application.Interfaces;

public interface IEmpresaService
{
    Task<IEnumerable<EmpresaDto>> GetAllAsync();
    Task<EmpresaDto> GetByIdAsync(int id);
    Task<EmpresaDto> AddAsync(EmpresaDto dto);
    Task<EmpresaDto> UpdateAsync(int id, EmpresaDto dto);
    Task<bool> DeleteAsync(int id);
}
