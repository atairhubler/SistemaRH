using SistemaRH.Application.DTOs;
using SistemaRH.Domain.Enums;

namespace SistemaRH.Application.Interfaces;

public interface IFeriasService
{
    Task<bool> EhElegivelAsync(int funcionarioId);
    Task GarantirPeriodosAsync(int funcionarioId);
    Task<List<PeriodoAquisitivoDto>> GetPeriodosAquisitivosAsync(int funcionarioId);
    Task<List<PeriodoFeriasDto>> GetUsosDoPeriodoAsync(int periodoAquisitivoId);
    Task<PeriodoFeriasDto> AdicionarUsoAsync(int periodoAquisitivoId, DateTime inicio, DateTime fim, TipoUsoFerias tipoUso, bool remunerada);
    Task RemoverUsoAsync(int usoId);
    Task<CalculoLiquidoFeriasDto> CalcularLiquidoFeriasAsync(int funcionarioId, int diasGozo, int diasAbono, int numeroDependentes);
    Task<List<FuncionarioDeFeriasNoMesDto>> GetFuncionariosDeFeriasNoMesAsync(int ano, int mes);
}
