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
}
