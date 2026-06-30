using SistemaRH.Application.DTOs;

namespace SistemaRH.Application.Interfaces;

public interface IFolhaPagamentoService
{
    FolhaPagamentoDto Calcular(FuncionarioCLTDto funcionario, int numeroDependentes, int mes, int ano, TabelasFolhaConfig? tabelas = null);
}
