using SistemaRH.Application.DTOs;
using SistemaRH.Domain.Enums;

namespace SistemaRH.Application.Interfaces;

public interface ICampoPersonalizadoService
{
    Task<IEnumerable<CampoPersonalizadoDto>> GetAllAsync();
    Task<IEnumerable<CampoPersonalizadoDto>> GetAplicaveisAsync(TipoFuncionario tipo);
    Task<CampoPersonalizadoDto> GetByIdAsync(int id);
    Task<CampoPersonalizadoDto> AddAsync(CampoPersonalizadoDto dto);
    Task<CampoPersonalizadoDto> UpdateAsync(int id, CampoPersonalizadoDto dto);
    Task<bool> DeleteAsync(int id);
}
