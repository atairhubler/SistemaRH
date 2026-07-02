using AutoMapper;
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
    private readonly IMapper _mapper;

    public FeriasService(RhDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<FeriasDto> CalcularDireitoAsync(int funcionarioId)
    {
        var ferias = await _context.Ferias
            .FirstOrDefaultAsync(f => f.FuncionarioId == funcionarioId)
            ?? throw new KeyNotFoundException($"Férias do funcionário {funcionarioId} não encontradas");

        var funcionario = await _context.Funcionarios.FindAsync(funcionarioId)
            ?? throw new KeyNotFoundException($"Funcionário {funcionarioId} não encontrado");

        var diasDisponiveis = funcionario switch
        {
            FuncionarioCLT clt => CalcularDiasCLT(clt),
            FuncionarioPJ pj => CalcularDiasPJ(pj),
            _ => throw new InvalidOperationException("Tipo desconhecido")
        };

        ferias.DiasDisponiveis = diasDisponiveis;
        await _context.SaveChangesAsync();

        return _mapper.Map<FeriasDto>(ferias);
    }

    public async Task<bool> AdicionarPeriodoFeriasAsync(int feriasId, DateTime inicio, DateTime fim, bool remunerada)
    {
        var ferias = await _context.Ferias
            .Include(f => f.Periodos)
            .FirstOrDefaultAsync(f => f.Id == feriasId)
            ?? throw new KeyNotFoundException($"Férias {feriasId} não encontrada");

        if (!ValidarPeriodo(inicio, fim, ferias))
            return false;

        var dias = (int)(fim.Date - inicio.Date).TotalDays + 1;

        var periodo = new PeriodoFerias
        {
            FeriasId = feriasId,
            DataInicio = inicio,
            DataFim = fim,
            Dias = dias,
            Status = StatusFeria.Planejada,
            Remunerada = remunerada,
            DataCriacao = DateTime.Now
        };

        ferias.Periodos.Add(periodo);
        ferias.DiasUtilizados += dias;

        if (remunerada)
            ferias.DiasRemunerados += dias;
        else
            ferias.DiasNaoRemunerados += dias;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<object>> GetPeriodosAsync(int feriasId)
    {
        return await _context.PeriodosFerias
            .Where(p => p.FeriasId == feriasId)
            .OrderBy(p => p.DataInicio)
            .Select(p => new
            {
                p.Id,
                p.DataInicio,
                p.DataFim,
                p.Dias,
                p.Status,
                p.Remunerada
            })
            .ToListAsync();
    }

    public async Task<FeriasDto> GetByFuncionarioAsync(int funcionarioId)
    {
        var ferias = await _context.Ferias
            .FirstOrDefaultAsync(f => f.FuncionarioId == funcionarioId)
            ?? throw new KeyNotFoundException($"Férias do funcionário {funcionarioId} não encontradas");

        return _mapper.Map<FeriasDto>(ferias);
    }

    public async Task<bool> RemoverPeriodoAsync(int periodoId)
    {
        var periodo = await _context.PeriodosFerias.FindAsync(periodoId);
        if (periodo == null)
            return false;

        var ferias = await _context.Ferias.FindAsync(periodo.FeriasId);
        if (ferias != null)
        {
            ferias.DiasUtilizados -= periodo.Dias;
            if (periodo.Remunerada)
                ferias.DiasRemunerados -= periodo.Dias;
            else
                ferias.DiasNaoRemunerados -= periodo.Dias;
        }

        _context.PeriodosFerias.Remove(periodo);
        await _context.SaveChangesAsync();
        return true;
    }

    private int CalcularDiasCLT(FuncionarioCLT clt)
    {
        var mesesTrabalhados = CalcularMesesTrabalhados(clt.DataAdmissao, DateTime.Now);

        if (mesesTrabalhados < 12)
            return 0;

        // CLT: 30 dias obrigatório
        return 30;
    }

    private int CalcularDiasPJ(FuncionarioPJ pj)
    {
        // PJ: depende do contrato
        return pj.TemDireitoFerias ? 30 : 0;
    }

    private bool ValidarPeriodo(DateTime inicio, DateTime fim, Ferias ferias)
    {
        // Não pode sobrepor períodos existentes
        var temSobreposicao = ferias.Periodos.Any(p =>
            (inicio >= p.DataInicio && inicio <= p.DataFim) ||
            (fim >= p.DataInicio && fim <= p.DataFim));

        if (temSobreposicao)
            return false;

        // Mínimo 1 dia
        if ((fim.Date - inicio.Date).TotalDays < 0)
            return false;

        // Saldo disponível
        var diasSolicitados = (int)(fim.Date - inicio.Date).TotalDays + 1;
        if (ferias.DiasDisponiveis - ferias.DiasUtilizados < diasSolicitados)
            return false;

        return true;
    }

    private int CalcularMesesTrabalhados(DateTime dataAdmissao, DateTime dataFim)
    {
        var anos = (dataFim.Year - dataAdmissao.Year) * 12;
        var meses = dataFim.Month - dataAdmissao.Month;
        return anos + meses;
    }
}
