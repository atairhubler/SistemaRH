using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;

namespace SistemaRH.Application.Services;

public class FolhaPagamentoService : IFolhaPagamentoService
{
    public FolhaPagamentoDto Calcular(FuncionarioCLTDto funcionario, int numeroDependentes, int mes, int ano, TabelasFolhaConfig? tabelas = null)
    {
        tabelas ??= TabelasFolhaConfig.Padrao2024();

        var bruto = funcionario.SalarioBruto;
        var inss = CalcularINSS(bruto, tabelas);
        var baseIRRF = Math.Max(0, bruto - inss - (numeroDependentes * tabelas.DeducaoPorDependente));
        var irrf = CalcularIRRF(baseIRRF, tabelas);
        var fgts = Math.Round(bruto * 0.08m, 2);
        var liquido = bruto - inss - irrf;

        return new FolhaPagamentoDto
        {
            FuncionarioId = funcionario.Id,
            NomeFuncionario = funcionario.Nome ?? "",
            Cargo = funcionario.Cargo ?? "",
            Mes = mes,
            Ano = ano,
            SalarioBruto = bruto,
            NumeroDependentes = numeroDependentes,
            DescontoINSS = inss,
            DescontoIRRF = irrf,
            FGTS = fgts,
            SalarioLiquido = liquido,
            CustoTotalEmpresa = bruto + fgts
        };
    }

    // INSS progressivo: cada faixa incide sobre a parcela do salário dentro do intervalo
    internal static decimal CalcularINSS(decimal salario, TabelasFolhaConfig tabelas)
    {
        decimal inss = 0;
        decimal limiteAnterior = 0;
        foreach (var faixa in tabelas.FaixasINSS.OrderBy(f => f.LimiteSuperior))
        {
            if (salario <= limiteAnterior) break;
            var parcela = Math.Min(salario, faixa.LimiteSuperior) - limiteAnterior;
            inss += parcela * faixa.Aliquota;
            limiteAnterior = faixa.LimiteSuperior;
        }
        return Math.Round(inss, 2);
    }

    // IRRF tabela simplificada: alíquota sobre a base inteira menos dedução fixa
    internal static decimal CalcularIRRF(decimal baseCalculo, TabelasFolhaConfig tabelas)
    {
        var faixas = tabelas.FaixasIRRF.OrderBy(f => f.LimiteSuperior).ToList();
        foreach (var faixa in faixas)
        {
            if (baseCalculo <= faixa.LimiteSuperior)
                return Math.Max(0, Math.Round(baseCalculo * faixa.Aliquota - faixa.Deducao, 2));
        }
        var ultima = faixas.Last();
        return Math.Max(0, Math.Round(baseCalculo * ultima.Aliquota - ultima.Deducao, 2));
    }
}
