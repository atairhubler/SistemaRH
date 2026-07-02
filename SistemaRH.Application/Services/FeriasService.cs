using Microsoft.EntityFrameworkCore;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Data;
using SistemaRH.Domain.Entities;
using SistemaRH.Domain.Enums;

namespace SistemaRH.Application.Services;

public class FeriasService : IFeriasService
{
    private readonly RhDbContext _context;

    public FeriasService(RhDbContext context)
    {
        _context = context;
    }

    public async Task<bool> EhElegivelAsync(int funcionarioId)
    {
        var funcionario = await _context.Funcionarios.FindAsync(funcionarioId)
            ?? throw new KeyNotFoundException($"Funcionário {funcionarioId} não encontrado");

        return ObterMarcoInicial(funcionario) != null;
    }

    private static DateTime? ObterMarcoInicial(Funcionario funcionario) => funcionario switch
    {
        FuncionarioCLT clt => clt.DataAdmissao,
        FuncionarioPJ pj when pj.TemDireitoFerias => pj.DataInicio,
        _ => null
    };

    public async Task GarantirPeriodosAsync(int funcionarioId)
    {
        var funcionario = await _context.Funcionarios.FindAsync(funcionarioId)
            ?? throw new KeyNotFoundException($"Funcionário {funcionarioId} não encontrado");

        var marcoInicial = ObterMarcoInicial(funcionario);
        if (marcoInicial == null)
            return;

        var ferias = await _context.Ferias
            .Include(f => f.PeriodosAquisitivos)
            .FirstOrDefaultAsync(f => f.FuncionarioId == funcionarioId)
            ?? throw new KeyNotFoundException($"Férias do funcionário {funcionarioId} não encontradas");

        var periodos = ferias.PeriodosAquisitivos.OrderBy(p => p.NumeroPeriodo).ToList();
        var proximoInicio = periodos.Count > 0 ? periodos[^1].DataFim : marcoInicial.Value;
        var proximoNumero = periodos.Count + 1;
        var limite = DateTime.Now.AddYears(1);

        var novos = new List<PeriodoAquisitivo>();
        while (proximoInicio <= limite)
        {
            var fim = proximoInicio.AddYears(1);
            novos.Add(new PeriodoAquisitivo
            {
                FeriasId = ferias.Id,
                NumeroPeriodo = proximoNumero,
                DataInicio = proximoInicio,
                DataFim = fim,
                DataLimiteUso = fim.AddMonths(10),
                DiasDireito = 30,
                DataCriacao = DateTime.Now
            });
            proximoInicio = fim;
            proximoNumero++;
        }

        if (novos.Count > 0)
        {
            _context.PeriodosAquisitivos.AddRange(novos);
            await _context.SaveChangesAsync();
        }

        await AssociarUsosOrfaosAsync(ferias.Id);
    }

    // Autocura de PeriodoFerias cadastrados antes da existência de PeriodoAquisitivo
    // (ou importados sem essa associação) — evita depender de um backfill arriscado na migration.
    private async Task AssociarUsosOrfaosAsync(int feriasId)
    {
        var orfaos = await _context.PeriodosFerias
            .Where(p => p.FeriasId == feriasId && p.PeriodoAquisitivoId == null)
            .ToListAsync();

        if (orfaos.Count == 0)
            return;

        var periodos = await _context.PeriodosAquisitivos
            .Where(p => p.FeriasId == feriasId)
            .ToListAsync();

        var alterou = false;
        foreach (var uso in orfaos)
        {
            var periodo = periodos.FirstOrDefault(p => uso.DataInicio >= p.DataInicio && uso.DataInicio <= p.DataLimiteUso);
            if (periodo == null) continue;

            uso.PeriodoAquisitivoId = periodo.Id;
            alterou = true;
        }

        if (alterou)
            await _context.SaveChangesAsync();
    }

    public async Task<List<PeriodoAquisitivoDto>> GetPeriodosAquisitivosAsync(int funcionarioId)
    {
        var ferias = await _context.Ferias
            .Include(f => f.PeriodosAquisitivos)
                .ThenInclude(p => p.Usos)
            .FirstOrDefaultAsync(f => f.FuncionarioId == funcionarioId)
            ?? throw new KeyNotFoundException($"Férias do funcionário {funcionarioId} não encontradas");

        var hoje = DateTime.Now.Date;

        return ferias.PeriodosAquisitivos
            .OrderBy(p => p.NumeroPeriodo)
            .Select(p =>
            {
                var diasUsufruidos = p.Usos.Where(u => u.TipoUso == TipoUsoFerias.Gozo).Sum(u => u.Dias);
                var diasAbono = p.Usos.Where(u => u.TipoUso == TipoUsoFerias.AbonoPecuniario).Sum(u => u.Dias);

                var status = hoje < p.DataFim
                    ? StatusPeriodoAquisitivo.EmAquisicao
                    : diasUsufruidos >= p.DiasDireito
                        ? StatusPeriodoAquisitivo.Regularizado
                        : hoje > p.DataLimiteUso
                            ? StatusPeriodoAquisitivo.Vencido
                            : StatusPeriodoAquisitivo.Pendente;

                return new PeriodoAquisitivoDto
                {
                    Id = p.Id,
                    FeriasId = p.FeriasId,
                    NumeroPeriodo = p.NumeroPeriodo,
                    DataInicio = p.DataInicio,
                    DataFim = p.DataFim,
                    DataLimiteUso = p.DataLimiteUso,
                    DiasDireito = p.DiasDireito,
                    DiasUsufruidos = diasUsufruidos,
                    DiasAbono = diasAbono,
                    Status = status
                };
            })
            .ToList();
    }

    public async Task<List<PeriodoFeriasDto>> GetUsosDoPeriodoAsync(int periodoAquisitivoId)
    {
        return await _context.PeriodosFerias
            .Where(p => p.PeriodoAquisitivoId == periodoAquisitivoId)
            .OrderBy(p => p.DataInicio)
            .Select(p => new PeriodoFeriasDto
            {
                Id = p.Id,
                FeriasId = p.FeriasId,
                PeriodoAquisitivoId = p.PeriodoAquisitivoId,
                DataInicio = p.DataInicio,
                DataFim = p.DataFim,
                Dias = p.Dias,
                TipoUso = p.TipoUso,
                Remunerada = p.Remunerada,
                Status = p.Status
            })
            .ToListAsync();
    }

    public async Task<PeriodoFeriasDto> AdicionarUsoAsync(int periodoAquisitivoId, DateTime inicio, DateTime fim, TipoUsoFerias tipoUso, bool remunerada)
    {
        var periodo = await _context.PeriodosAquisitivos
            .Include(p => p.Usos)
            .FirstOrDefaultAsync(p => p.Id == periodoAquisitivoId)
            ?? throw new KeyNotFoundException($"Período aquisitivo {periodoAquisitivoId} não encontrado");

        var dias = (int)(fim.Date - inicio.Date).TotalDays + 1;

        ValidarUso(periodo, inicio, fim, dias, tipoUso);

        var uso = new PeriodoFerias
        {
            FeriasId = periodo.FeriasId,
            PeriodoAquisitivoId = periodo.Id,
            DataInicio = inicio,
            DataFim = fim,
            Dias = dias,
            TipoUso = tipoUso,
            Status = StatusFeria.Planejada,
            Remunerada = remunerada,
            DataCriacao = DateTime.Now
        };

        _context.PeriodosFerias.Add(uso);
        await _context.SaveChangesAsync();

        return new PeriodoFeriasDto
        {
            Id = uso.Id,
            FeriasId = uso.FeriasId,
            PeriodoAquisitivoId = uso.PeriodoAquisitivoId,
            DataInicio = uso.DataInicio,
            DataFim = uso.DataFim,
            Dias = uso.Dias,
            TipoUso = uso.TipoUso,
            Remunerada = uso.Remunerada,
            Status = uso.Status
        };
    }

    // Regras da CLT: fracionamento em até 3 partes (uma com >=14 dias quando fracionado,
    // as demais com >=5 dias), abono pecuniário máximo de 10 dias por período aquisitivo.
    private static void ValidarUso(PeriodoAquisitivo periodo, DateTime inicio, DateTime fim, int dias, TipoUsoFerias tipoUso)
    {
        if (fim.Date < inicio.Date)
            throw new InvalidOperationException("A data final não pode ser anterior à data inicial.");

        var sobrepoe = periodo.Usos.Any(u => inicio.Date <= u.DataFim.Date && fim.Date >= u.DataInicio.Date);
        if (sobrepoe)
            throw new InvalidOperationException("Já existe um período de uso cadastrado nesse intervalo de datas.");

        var diasGozoExistentes = periodo.Usos.Where(u => u.TipoUso == TipoUsoFerias.Gozo).Sum(u => u.Dias);
        var diasAbonoExistentes = periodo.Usos.Where(u => u.TipoUso == TipoUsoFerias.AbonoPecuniario).Sum(u => u.Dias);

        if (tipoUso == TipoUsoFerias.AbonoPecuniario && diasAbonoExistentes + dias > 10)
            throw new InvalidOperationException("O abono pecuniário não pode superar 10 dias por período aquisitivo.");

        if (diasGozoExistentes + diasAbonoExistentes + dias > periodo.DiasDireito)
            throw new InvalidOperationException($"Saldo insuficiente: o período tem direito a {periodo.DiasDireito} dias e já foram utilizados/abonados {diasGozoExistentes + diasAbonoExistentes}.");

        if (tipoUso != TipoUsoFerias.Gozo)
            return;

        if (dias < 5)
            throw new InvalidOperationException("Cada período de férias gozadas deve ter no mínimo 5 dias.");

        var totalFracoes = periodo.Usos.Count(u => u.TipoUso == TipoUsoFerias.Gozo) + 1;
        if (totalFracoes > 3)
            throw new InvalidOperationException("Um período aquisitivo não pode ser dividido em mais de 3 partes de gozo.");

        if (totalFracoes <= 1)
            return;

        var maiorFracao = Math.Max(dias, periodo.Usos.Where(u => u.TipoUso == TipoUsoFerias.Gozo).Select(u => u.Dias).DefaultIfEmpty(0).Max());
        if (maiorFracao < 14)
            throw new InvalidOperationException("Ao fracionar as férias, pelo menos uma das partes deve ter no mínimo 14 dias.");
    }

    public async Task RemoverUsoAsync(int usoId)
    {
        var uso = await _context.PeriodosFerias.FindAsync(usoId)
            ?? throw new KeyNotFoundException($"Uso de férias {usoId} não encontrado");

        _context.PeriodosFerias.Remove(uso);
        await _context.SaveChangesAsync();
    }

    public async Task<CalculoLiquidoFeriasDto> CalcularLiquidoFeriasAsync(int funcionarioId, int diasGozo, int diasAbono, int numeroDependentes)
    {
        var clt = await _context.FuncionariosCLT.FindAsync(funcionarioId)
            ?? throw new InvalidOperationException("O cálculo de valor líquido de férias (INSS/IRRF) só está disponível para funcionários CLT.");

        var tabelas = TabelasFolhaConfig.Padrao2024();
        var salarioBruto = clt.SalarioBruto;

        var valorFerias = Math.Round(salarioBruto / 30m * diasGozo, 2);
        var tercoFerias = Math.Round(valorFerias / 3m, 2);
        var brutoFerias = valorFerias + tercoFerias;

        var valorAbono = Math.Round(salarioBruto / 30m * diasAbono, 2);
        var tercoAbono = Math.Round(valorAbono / 3m, 2);
        var brutoAbono = valorAbono + tercoAbono;

        // Abono pecuniário é isento de INSS/IRRF por lei — a base de desconto considera só o valor de gozo.
        var inss = FolhaPagamentoService.CalcularINSS(brutoFerias, tabelas);
        var baseIrrf = Math.Max(0, brutoFerias - inss - (numeroDependentes * tabelas.DeducaoPorDependente));
        var irrf = FolhaPagamentoService.CalcularIRRF(baseIrrf, tabelas);

        var liquido = brutoFerias - inss - irrf + brutoAbono;

        return new CalculoLiquidoFeriasDto
        {
            SalarioBruto = salarioBruto,
            DiasGozo = diasGozo,
            DiasAbono = diasAbono,
            NumeroDependentes = numeroDependentes,
            ValorFerias = valorFerias,
            TercoFerias = tercoFerias,
            BrutoFerias = brutoFerias,
            ValorAbono = valorAbono,
            TercoAbono = tercoAbono,
            BrutoAbono = brutoAbono,
            DescontoINSS = inss,
            DescontoIRRF = irrf,
            Liquido = liquido
        };
    }

    public async Task<List<FuncionarioDeFeriasNoMesDto>> GetFuncionariosDeFeriasNoMesAsync(int ano, int mes)
    {
        var inicioMes = new DateTime(ano, mes, 1);
        var fimMes = inicioMes.AddMonths(1).AddDays(-1);

        var usos = await _context.PeriodosFerias
            .Include(p => p.Ferias)
                .ThenInclude(f => f.Funcionario)
            .Where(p => p.TipoUso == TipoUsoFerias.Gozo && p.DataInicio <= fimMes && p.DataFim >= inicioMes)
            .ToListAsync();

        return usos.Select(p => new FuncionarioDeFeriasNoMesDto
        {
            FuncionarioId = p.Ferias.FuncionarioId,
            FuncionarioNome = p.Ferias.Funcionario.Nome,
            DataInicio = p.DataInicio,
            DataFim = p.DataFim
        }).ToList();
    }
}
