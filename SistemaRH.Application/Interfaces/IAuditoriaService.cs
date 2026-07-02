using SistemaRH.Application.DTOs;

namespace SistemaRH.Application.Interfaces;

public interface IAuditoriaService
{
    Task RegistrarAsync(string acao, string entidade, string descricao, string? valoresAntes = null, string? valoresDepois = null);
    Task<IEnumerable<LogAuditoriaDto>> GetAllAsync();
}
