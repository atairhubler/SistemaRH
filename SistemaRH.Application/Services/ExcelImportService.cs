using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Data;
using SistemaRH.Domain.Enums;

namespace SistemaRH.Application.Services;

public class ExcelImportService : IExcelImportService
{
    private const string AgilTelecomCnpj = "08.340.286/0002-47";
    private const string AgilTelecomRazaoSocial = "Agil Telecom";
    private const int PrimeiraLinhaDados = 6;

    private readonly RhDbContext _context;
    private readonly IEmpresaService _empresaService;
    private readonly IFuncionarioService _funcionarioService;
    private readonly IHistoricoSalarialService _historicoService;

    public ExcelImportService(
        RhDbContext context,
        IEmpresaService empresaService,
        IFuncionarioService funcionarioService,
        IHistoricoSalarialService historicoService)
    {
        _context = context;
        _empresaService = empresaService;
        _funcionarioService = funcionarioService;
        _historicoService = historicoService;
    }

    public async Task<ImportResultDto> ImportarAgilTelecomAsync(string caminhoArquivo)
    {
        var resultado = new ImportResultDto();
        var empresaId = await ObterOuCriarAgilTelecomAsync();

        using var workbook = new XLWorkbook(caminhoArquivo);

        var abaPj = EncontrarAba(workbook, "PJ");
        if (abaPj != null)
            await ImportarPjAsync(abaPj, empresaId, resultado);
        else
            resultado.Avisos.Add("Aba 'PJ' não encontrada na planilha.");

        var abaClt = EncontrarAba(workbook, "CLT");
        if (abaClt != null)
            await ImportarCltAsync(abaClt, empresaId, resultado);
        else
            resultado.Avisos.Add("Aba 'CLT' não encontrada na planilha.");

        var abaEstagiario = EncontrarAba(workbook, "ESTAGIÁRIO", "ESTAGIARIO");
        if (abaEstagiario != null)
            await ImportarEstagiarioAsync(abaEstagiario, empresaId, resultado);
        else
            resultado.Avisos.Add("Aba 'ESTAGIÁRIO' não encontrada na planilha.");

        return resultado;
    }

    private static IXLWorksheet? EncontrarAba(XLWorkbook workbook, params string[] nomes) =>
        workbook.Worksheets.FirstOrDefault(w =>
            nomes.Any(n => string.Equals(w.Name.Trim(), n, StringComparison.OrdinalIgnoreCase)));

    private async Task<int> ObterOuCriarAgilTelecomAsync()
    {
        var empresas = await _empresaService.GetAllAsync();
        var existente = empresas.FirstOrDefault(e => e.Cnpj == AgilTelecomCnpj);
        if (existente != null) return existente.Id;

        var criada = await _empresaService.AddAsync(new EmpresaDto
        {
            RazaoSocial = AgilTelecomRazaoSocial,
            Cnpj = AgilTelecomCnpj
        });
        return criada.Id;
    }

    private async Task ImportarPjAsync(IXLWorksheet aba, int empresaId, ImportResultDto resultado)
    {
        var ultimaLinha = aba.LastRowUsed()?.RowNumber() ?? PrimeiraLinhaDados;

        for (var linha = PrimeiraLinhaDados; linha <= ultimaLinha; linha++)
        {
            var nome = Texto(aba, linha, 2);
            if (string.IsNullOrWhiteSpace(nome)) continue;

            var cnpj = Texto(aba, linha, 14);

            var jaExiste = !string.IsNullOrWhiteSpace(cnpj)
                ? await _context.FuncionariosPJ.AnyAsync(p => p.EmpresaId == empresaId && p.Cnpj == cnpj)
                : await _context.FuncionariosPJ.AnyAsync(p => p.EmpresaId == empresaId && p.Nome == nome);

            if (jaExiste)
            {
                resultado.PjIgnorados++;
                continue;
            }

            var valorServico = Decimal(aba, linha, 27);

            var dto = new FuncionarioPJDto
            {
                Nome = nome,
                RazaoSocial = ValorOuFallback(Texto(aba, linha, 13), nome),
                Cnpj = cnpj,
                Contratante = Texto(aba, linha, 3),
                Email = Texto(aba, linha, 12),
                EmailEmpresa = Texto(aba, linha, 11),
                Telefone = Texto(aba, linha, 8),
                TelefoneAgil = Texto(aba, linha, 9),
                Endereco = Texto(aba, linha, 15),
                Genero = Texto(aba, linha, 5),
                Ramal = Texto(aba, linha, 10),
                DadosBancarios = Texto(aba, linha, 17),
                Comissionado = Booleano(Texto(aba, linha, 33)),
                Observacoes = Texto(aba, linha, 30),
                AjudaDeCusto = Decimal(aba, linha, 26),
                DataNascimento = Data(aba, linha, 16) ?? default,
                DataInicio = Data(aba, linha, 6),
                DataFim = Data(aba, linha, 7),
                ValidadeContrato = Texto(aba, linha, 18),
                TipoServico = Texto(aba, linha, 32),
                EmiteNF = Booleano(Texto(aba, linha, 28)),
                DiaSolicitacaoNF = Texto(aba, linha, 29),
                ValorServico = valorServico,
                ValorContratado = Decimal(aba, linha, 19),
                Departamento = Texto(aba, linha, 31),
                EmpresaId = empresaId,
                Status = InterpretarStatus(Texto(aba, linha, 4), StatusFuncionario.ContratoEncerrado)
            };

            var criado = await _funcionarioService.AddPJAsync(dto);
            if (valorServico > 0)
                await _historicoService.AdicionarAsync(criado.Id, DateTime.Now, valorServico, "Importação", "Valor importado da planilha");
            resultado.PjImportados++;
        }
    }

    private async Task ImportarCltAsync(IXLWorksheet aba, int empresaId, ImportResultDto resultado)
    {
        var ultimaLinha = aba.LastRowUsed()?.RowNumber() ?? PrimeiraLinhaDados;

        for (var linha = PrimeiraLinhaDados; linha <= ultimaLinha; linha++)
        {
            var nome = Texto(aba, linha, 3);
            if (string.IsNullOrWhiteSpace(nome)) continue;

            var cpf = Texto(aba, linha, 16);

            var jaExiste = !string.IsNullOrWhiteSpace(cpf)
                ? await _context.FuncionariosCLT.AnyAsync(c => c.EmpresaId == empresaId && c.Cpf == cpf)
                : await _context.FuncionariosCLT.AnyAsync(c => c.EmpresaId == empresaId && c.Nome == nome);

            if (jaExiste)
            {
                resultado.CltIgnorados++;
                continue;
            }

            var dataAdmissao = Data(aba, linha, 6);
            if (dataAdmissao == null)
                resultado.Avisos.Add($"CLT '{nome}': sem data de admissão na planilha, usado a data de hoje.");

            var salario = Decimal(aba, linha, 20);

            var dto = new FuncionarioCLTDto
            {
                Nome = nome,
                Cpf = cpf,
                Rg = Texto(aba, linha, 15),
                Codigo = Texto(aba, linha, 2),
                Contratante = Texto(aba, linha, 4),
                Email = Texto(aba, linha, 12),
                EmailEmpresa = Texto(aba, linha, 11),
                Telefone = Texto(aba, linha, 8),
                Endereco = Texto(aba, linha, 13),
                Genero = Texto(aba, linha, 10),
                Ramal = Texto(aba, linha, 9),
                DadosBancarios = Texto(aba, linha, 17),
                Comissionado = Booleano(Texto(aba, linha, 35)),
                Observacoes = Texto(aba, linha, 34),
                AjudaDeCusto = Decimal(aba, linha, 30),
                DataNascimento = Data(aba, linha, 14) ?? default,
                DataAdmissao = dataAdmissao ?? DateTime.Now,
                DataDemissao = Data(aba, linha, 7),
                Cargo = Texto(aba, linha, 33),
                Departamento = Texto(aba, linha, 32),
                SalarioBruto = salario,
                ComplementoSalarial = Decimal(aba, linha, 28),
                AuxilioEducacao = Decimal(aba, linha, 29),
                EmpresaId = empresaId,
                Status = InterpretarStatus(Texto(aba, linha, 5), StatusFuncionario.Demitido)
            };

            var criado = await _funcionarioService.AddCltAsync(dto);
            if (salario > 0)
                await _historicoService.AdicionarAsync(criado.Id, dto.DataAdmissao, salario, "Importação", "Salário importado da planilha");
            resultado.CltImportados++;
        }
    }

    private async Task ImportarEstagiarioAsync(IXLWorksheet aba, int empresaId, ImportResultDto resultado)
    {
        var ultimaLinha = aba.LastRowUsed()?.RowNumber() ?? PrimeiraLinhaDados;

        for (var linha = PrimeiraLinhaDados; linha <= ultimaLinha; linha++)
        {
            var nome = Texto(aba, linha, 3);
            if (string.IsNullOrWhiteSpace(nome)) continue;

            var cpf = Texto(aba, linha, 16);

            var jaExiste = !string.IsNullOrWhiteSpace(cpf)
                ? await _context.FuncionariosEstagiario.AnyAsync(est => est.EmpresaId == empresaId && est.Cpf == cpf)
                : await _context.FuncionariosEstagiario.AnyAsync(est => est.EmpresaId == empresaId && est.Nome == nome);

            if (jaExiste)
            {
                resultado.EstagiarioIgnorados++;
                continue;
            }

            var dataAdmissao = Data(aba, linha, 6);
            if (dataAdmissao == null)
                resultado.Avisos.Add($"Estagiário '{nome}': sem data de admissão na planilha, usado a data de hoje.");

            var bolsa = Decimal(aba, linha, 20);

            var dto = new FuncionarioEstagiarioDto
            {
                Nome = nome,
                Cpf = cpf,
                Rg = Texto(aba, linha, 15),
                Codigo = Texto(aba, linha, 2),
                Contratante = Texto(aba, linha, 4),
                Email = Texto(aba, linha, 12),
                EmailEmpresa = Texto(aba, linha, 11),
                Telefone = Texto(aba, linha, 8),
                Endereco = Texto(aba, linha, 13),
                Genero = Texto(aba, linha, 10),
                Ramal = Texto(aba, linha, 9),
                DadosBancarios = Texto(aba, linha, 17),
                Comissionado = Booleano(Texto(aba, linha, 29)),
                Observacoes = Texto(aba, linha, 28),
                AjudaDeCusto = Decimal(aba, linha, 24),
                DataNascimento = Data(aba, linha, 14) ?? default,
                DataAdmissao = dataAdmissao ?? DateTime.Now,
                DataDemissao = Data(aba, linha, 7),
                Cargo = Texto(aba, linha, 27),
                Departamento = Texto(aba, linha, 26),
                Bolsa = bolsa,
                ComplementoSalarial = Decimal(aba, linha, 23),
                EmpresaId = empresaId,
                Status = InterpretarStatus(Texto(aba, linha, 5), StatusFuncionario.Demitido)
            };

            var criado = await _funcionarioService.AddEstagiarioAsync(dto);
            if (bolsa > 0)
                await _historicoService.AdicionarAsync(criado.Id, dto.DataAdmissao, bolsa, "Importação", "Bolsa importada da planilha");
            resultado.EstagiarioImportados++;
        }
    }

    private static StatusFuncionario InterpretarStatus(string? valor, StatusFuncionario statusInativo)
    {
        var upper = (valor ?? "").Trim().ToUpperInvariant();
        return upper.Contains("DESATIV") ? statusInativo : StatusFuncionario.Ativo;
    }

    private static bool Booleano(string? valor) =>
        (valor ?? "").Trim().ToUpperInvariant().Contains("SIM");

    private static string ValorOuFallback(string? valor, string? fallback) =>
        string.IsNullOrWhiteSpace(valor) ? (fallback ?? "") : valor;

    private static string Texto(IXLWorksheet aba, int linha, int coluna) =>
        aba.Cell(linha, coluna).GetString().Trim();

    private static DateTime? Data(IXLWorksheet aba, int linha, int coluna)
    {
        var cell = aba.Cell(linha, coluna);
        return cell.TryGetValue(out DateTime valor) ? valor : null;
    }

    private static decimal Decimal(IXLWorksheet aba, int linha, int coluna)
    {
        var cell = aba.Cell(linha, coluna);
        return cell.TryGetValue(out decimal valor) ? valor : 0m;
    }
}
