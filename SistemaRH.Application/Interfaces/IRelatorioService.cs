using SistemaRH.Application.DTOs;

namespace SistemaRH.Application.Interfaces;

public interface IRelatorioService
{
    Task<RelatorioComparativoDto> GerarRelatorioCustosAsync(int empresaId, int mes, int ano);
    Task<byte[]> ExportarRelatorioExcelAsync(RelatorioComparativoDto relatorio);
    Task<List<TendenciaMensalItemDto>> GetTendenciaMensalAsync(int empresaId, int meses = 6);
}
