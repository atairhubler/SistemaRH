using SistemaRH.Application.DTOs;

namespace SistemaRH.Application.Interfaces;

public interface IFeriasService
{
    Task<FeriasDto> CalcularDireitoAsync(int funcionarioId);
    Task<bool> AdicionarPeriodoFeriasAsync(int feriasId, DateTime inicio, DateTime fim, bool remunerada);
    Task<IEnumerable<object>> GetPeriodosAsync(int feriasId);
    Task<FeriasDto> GetByFuncionarioAsync(int funcionarioId);
    Task<bool> RemoverPeriodoAsync(int periodoId);
}
