using System.Globalization;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Data;
using ClosedXML.Excel;

namespace SistemaRH.Application.Services;

public class RelatorioService : IRelatorioService
{
    private readonly RhDbContext _context;
    private readonly IMapper _mapper;

    public RelatorioService(RhDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<RelatorioComparativoDto> GerarRelatorioCustosAsync(int empresaId, int mes, int ano)
    {
        var cltList = await _context.FuncionariosCLT
            .Where(c => c.EmpresaId == empresaId && c.Status == Domain.Enums.StatusFuncionario.Ativo)
            .ToListAsync();

        var pjList = await _context.FuncionariosPJ
            .Where(p => p.EmpresaId == empresaId && p.Status == Domain.Enums.StatusFuncionario.Ativo)
            .ToListAsync();

        // Custo CLT
        decimal custoCLT = 0;
        foreach (var clt in cltList)
        {
            // Buscar salário vigente no mês
            var salario = await _context.SalarioHistorico
                .Where(s => s.FuncionarioId == clt.Id &&
                           s.DataVigencia.Year == ano &&
                           s.DataVigencia.Month <= mes)
                .OrderByDescending(s => s.DataVigencia)
                .FirstOrDefaultAsync();

            if (salario != null)
                custoCLT += salario.SalarioBruto;
            else
                custoCLT += clt.SalarioBruto;
        }

        // Custo PJ
        decimal custoPJ = 0;
        foreach (var pj in pjList)
        {
            var rpasNfs = await _context.RpasNfsPJ
                .Where(r => r.FuncionarioPJId == pj.Id &&
                           r.Mes == mes &&
                           r.Ano == ano)
                .ToListAsync();

            custoPJ += rpasNfs.Sum(r => r.ValorBruto);
        }

        // Encargos CLT (28% estimado)
        var custoCLTComEncargos = custoCLT * 1.28m;

        var relatorio = new RelatorioComparativoDto
        {
            Mes = mes,
            Ano = ano,
            QuantidadeCLT = cltList.Count,
            QuantidadePJ = pjList.Count,
            CustosCLT = custoCLTComEncargos,
            CustosPJ = custoPJ,
            DataGeracao = DateTime.Now
        };

        return relatorio;
    }

    public async Task<byte[]> ExportarRelatorioExcelAsync(RelatorioComparativoDto relatorio)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Comparativo");

        // Cabeçalho
        ws.Cell("A1").Value = $"Relatório Comparativo CLT vs PJ - {relatorio.Mes:00}/{relatorio.Ano}";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 14;

        ws.Cell("A2").Value = $"Data Geração: {relatorio.DataGeracao:dd/MM/yyyy HH:mm:ss}";

        // Resumo
        int row = 4;
        ws.Cell($"A{row}").Value = "RESUMO";
        ws.Cell($"A{row}").Style.Font.Bold = true;

        row++;
        ws.Cell($"A{row}").Value = "Métrica";
        ws.Cell($"B{row}").Value = "Valor";
        ws.Row(row).Style.Font.Bold = true;

        row++;
        ws.Cell($"A{row}").Value = "Total Funcionários CLT";
        ws.Cell($"B{row}").Value = relatorio.QuantidadeCLT;

        row++;
        ws.Cell($"A{row}").Value = "Custo Total CLT (com encargos)";
        ws.Cell($"B{row}").Value = relatorio.CustosCLT;
        ws.Cell($"B{row}").Style.NumberFormat.Format = "R$ #,##0.00";

        row++;
        ws.Cell($"A{row}").Value = "Total Funcionários PJ";
        ws.Cell($"B{row}").Value = relatorio.QuantidadePJ;

        row++;
        ws.Cell($"A{row}").Value = "Custo Total PJ";
        ws.Cell($"B{row}").Value = relatorio.CustosPJ;
        ws.Cell($"B{row}").Style.NumberFormat.Format = "R$ #,##0.00";

        row += 2;
        ws.Cell($"A{row}").Value = "ANÁLISE";
        ws.Cell($"A{row}").Style.Font.Bold = true;

        row++;
        ws.Cell($"A{row}").Value = "Custo Total";
        ws.Cell($"B{row}").Value = relatorio.CustoTotal;
        ws.Cell($"B{row}").Style.NumberFormat.Format = "R$ #,##0.00";
        ws.Cell($"B{row}").Style.Font.Bold = true;

        row++;
        ws.Cell($"A{row}").Value = "Custo Médio CLT";
        ws.Cell($"B{row}").Value = relatorio.CustoMedioCLT;
        ws.Cell($"B{row}").Style.NumberFormat.Format = "R$ #,##0.00";

        row++;
        ws.Cell($"A{row}").Value = "Custo Médio PJ";
        ws.Cell($"B{row}").Value = relatorio.CustoMedioPJ;
        ws.Cell($"B{row}").Style.NumberFormat.Format = "R$ #,##0.00";

        row++;
        ws.Cell($"A{row}").Value = "Diferença Absoluta";
        ws.Cell($"B{row}").Value = relatorio.DiferencaAbsoluta;
        ws.Cell($"B{row}").Style.NumberFormat.Format = "R$ #,##0.00";

        row++;
        ws.Cell($"A{row}").Value = "Percentual PJ";
        ws.Cell($"B{row}").Value = relatorio.PercentualPJ / 100;
        ws.Cell($"B{row}").Style.NumberFormat.Format = "0.00%";

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<List<TendenciaMensalItemDto>> GetTendenciaMensalAsync(int empresaId, int meses = 6)
    {
        var cltQuery = _context.FuncionariosCLT.AsQueryable();
        var pjQuery = _context.FuncionariosPJ.AsQueryable();
        var estagiarioQuery = _context.FuncionariosEstagiario.AsQueryable();

        if (empresaId > 0)
        {
            cltQuery = cltQuery.Where(c => c.EmpresaId == empresaId);
            pjQuery = pjQuery.Where(p => p.EmpresaId == empresaId);
            estagiarioQuery = estagiarioQuery.Where(e => e.EmpresaId == empresaId);
        }

        var cltList = await cltQuery.ToListAsync();
        var pjList = await pjQuery.ToListAsync();
        var estagiarioList = await estagiarioQuery.ToListAsync();

        var cltIds = cltList.Select(c => c.Id).ToList();
        var pjIds = pjList.Select(p => p.Id).ToList();

        var historicoPorFuncionario = (await _context.SalarioHistorico
                .Where(s => cltIds.Contains(s.FuncionarioId))
                .ToListAsync())
            .GroupBy(s => s.FuncionarioId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(s => s.DataVigencia).ToList());

        var rpasNfs = await _context.RpasNfsPJ
            .Where(r => pjIds.Contains(r.FuncionarioPJId))
            .ToListAsync();

        var hoje = DateTime.Now;
        var meseAno = Enumerable.Range(0, meses)
            .Select(i => hoje.AddMonths(-(meses - 1 - i)))
            .Select(d => (d.Year, d.Month))
            .ToList();

        var resultado = new List<TendenciaMensalItemDto>();
        var ptBR = new CultureInfo("pt-BR");

        foreach (var (ano, mes) in meseAno)
        {
            var fimDoMes = new DateTime(ano, mes, DateTime.DaysInMonth(ano, mes));

            var cltAtivos = cltList
                .Where(c => c.DataAdmissao <= fimDoMes && (c.DataDemissao == null || c.DataDemissao >= fimDoMes))
                .ToList();
            var pjAtivos = pjList
                .Where(p => (p.DataInicio == null || p.DataInicio <= fimDoMes) && (p.DataFim == null || p.DataFim >= fimDoMes))
                .ToList();
            var estagiarioAtivos = estagiarioList
                .Where(e => e.DataAdmissao <= fimDoMes && (e.DataDemissao == null || e.DataDemissao >= fimDoMes))
                .ToList();

            decimal custoCLT = 0;
            foreach (var clt in cltAtivos)
            {
                if (historicoPorFuncionario.TryGetValue(clt.Id, out var historico))
                {
                    var vigente = historico.FirstOrDefault(h => h.DataVigencia <= fimDoMes);
                    custoCLT += vigente?.SalarioBruto ?? clt.SalarioBruto;
                }
                else
                {
                    custoCLT += clt.SalarioBruto;
                }
            }

            var custoPJ = rpasNfs.Where(r => r.Mes == mes && r.Ano == ano).Sum(r => r.ValorBruto);

            resultado.Add(new TendenciaMensalItemDto
            {
                Mes = mes,
                Ano = ano,
                Rotulo = new DateTime(ano, mes, 1).ToString("MMM/yy", ptBR),
                TotalCLT = cltAtivos.Count,
                TotalPJ = pjAtivos.Count,
                TotalEstagiario = estagiarioAtivos.Count,
                CustoCLT = custoCLT,
                CustoPJ = custoPJ
            });
        }

        return resultado;
    }
}
