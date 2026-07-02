using SistemaRH.Application.DTOs;

namespace SistemaRH.Application.Interfaces;

public interface IExcelImportService
{
    Task<ImportResultDto> ImportarAgilTelecomAsync(string caminhoArquivo);
}
