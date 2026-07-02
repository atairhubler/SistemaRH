using Microsoft.EntityFrameworkCore;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Data;
using SistemaRH.Domain.Entities;

namespace SistemaRH.Application.Services;

public class AuditoriaService : IAuditoriaService
{
    private readonly RhDbContext _context;

    public AuditoriaService(RhDbContext context)
    {
        _context = context;
    }

    public async Task RegistrarAsync(string acao, string entidade, string descricao, string? valoresAntes = null, string? valoresDepois = null)
    {
        _context.LogsAuditoria.Add(new LogAuditoria
        {
            DataHora = DateTime.Now,
            Usuario = SessaoAtual.UsuarioAtual ?? "Desconhecido",
            Acao = acao,
            Entidade = entidade,
            Descricao = descricao,
            ValoresAntes = valoresAntes,
            ValoresDepois = valoresDepois
        });
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<LogAuditoriaDto>> GetAllAsync()
    {
        var logs = await _context.LogsAuditoria
            .OrderByDescending(l => l.DataHora)
            .ToListAsync();

        return logs.Select(l => new LogAuditoriaDto
        {
            Id = l.Id,
            DataHora = l.DataHora,
            Usuario = l.Usuario,
            Acao = l.Acao,
            Entidade = l.Entidade,
            Descricao = l.Descricao,
            ValoresAntes = l.ValoresAntes,
            ValoresDepois = l.ValoresDepois
        });
    }
}
