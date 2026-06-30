using SistemaRH.Application.DTOs;

namespace SistemaRH.Application.Interfaces;

public interface IRpaNfService
{
    Task<RpaNfDto> CriarRpaAsync(RpaNfDto dto, Stream arquivo);
    Task<RpaNfDto> GetByIdAsync(int id);
    Task<IEnumerable<RpaNfDto>> GetByCompetenciaAsync(int empresaId, int mes, int ano);
    Task<IEnumerable<RpaNfDto>> GetByFuncionarioAsync(int funcionarioPJId);
    Task<IEnumerable<RpaNfDto>> GetByStatusAsync(int statusPagamento);
    Task<bool> AtualizarStatusAsync(int id, int novoStatus);
    Task<byte[]> ExportarPdfAsync(int rpaNfId);
}
