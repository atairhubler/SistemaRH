using SistemaRH.Application.DTOs;

namespace SistemaRH.Application.Interfaces;

public interface IAtestadoService
{
    Task<IEnumerable<AtestadoDto>> GetByFuncionarioAsync(int funcionarioId);
    Task<AtestadoDto> AdicionarAsync(AtestadoDto dto, Stream arquivo, string nomeArquivo);
    Task RemoverAsync(int id);
    Task<(byte[] Arquivo, string NomeArquivo)> ExportarArquivoAsync(int id);
}
