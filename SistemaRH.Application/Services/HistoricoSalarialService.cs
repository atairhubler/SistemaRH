using Microsoft.EntityFrameworkCore;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Data;
using SistemaRH.Domain.Entities;

namespace SistemaRH.Application.Services;

public class HistoricoSalarialService : IHistoricoSalarialService
{
    private readonly RhDbContext _context;

    public HistoricoSalarialService(RhDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SalarioHistoricoDto>> GetByFuncionarioAsync(int funcionarioId)
    {
        var historico = await _context.SalarioHistorico
            .Where(s => s.FuncionarioId == funcionarioId)
            .OrderByDescending(s => s.DataVigencia)
            .ToListAsync();

        return historico.Select(ToDto);
    }

    public async Task<SalarioHistoricoDto> AdicionarAsync(int funcionarioId, DateTime dataVigencia, decimal valor, string motivo, string? observacoes)
    {
        var funcionario = await _context.Funcionarios.FindAsync(funcionarioId)
            ?? throw new KeyNotFoundException($"Funcionário com ID {funcionarioId} não encontrado");

        switch (funcionario)
        {
            case FuncionarioCLT clt:
                clt.SalarioBruto = valor;
                break;
            case FuncionarioPJ pj:
                pj.ValorServico = valor;
                break;
            case FuncionarioEstagiario est:
                est.Bolsa = valor;
                break;
        }
        funcionario.DataAtualizacao = DateTime.Now;

        var registro = new SalarioHistorico
        {
            FuncionarioId = funcionarioId,
            DataVigencia = dataVigencia,
            SalarioBruto = valor,
            Motivo = motivo,
            Observacoes = observacoes ?? "",
            DataCriacao = DateTime.Now
        };

        _context.SalarioHistorico.Add(registro);
        await _context.SaveChangesAsync();

        return ToDto(registro);
    }

    private static SalarioHistoricoDto ToDto(SalarioHistorico s) => new()
    {
        Id = s.Id,
        FuncionarioId = s.FuncionarioId,
        DataVigencia = s.DataVigencia,
        SalarioBruto = s.SalarioBruto,
        Motivo = s.Motivo ?? "",
        Observacoes = s.Observacoes ?? ""
    };
}
