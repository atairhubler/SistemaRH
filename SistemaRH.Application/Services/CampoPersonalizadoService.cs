using Microsoft.EntityFrameworkCore;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Data;
using SistemaRH.Domain.Entities;
using SistemaRH.Domain.Enums;

namespace SistemaRH.Application.Services;

public class CampoPersonalizadoService : ICampoPersonalizadoService
{
    private readonly RhDbContext _context;

    public CampoPersonalizadoService(RhDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CampoPersonalizadoDto>> GetAllAsync()
    {
        var campos = await _context.CamposPersonalizados
            .OrderBy(c => c.Ordem)
            .ToListAsync();

        return campos.Select(ToDto);
    }

    public async Task<IEnumerable<CampoPersonalizadoDto>> GetAplicaveisAsync(TipoFuncionario tipo)
    {
        var campos = await _context.CamposPersonalizados
            .Where(c => c.Ativo && (c.AplicavelA == null || c.AplicavelA == tipo))
            .OrderBy(c => c.Ordem)
            .ToListAsync();

        return campos.Select(ToDto);
    }

    public async Task<CampoPersonalizadoDto> GetByIdAsync(int id)
    {
        var campo = await _context.CamposPersonalizados.FindAsync(id)
            ?? throw new KeyNotFoundException($"Campo personalizado com ID {id} não encontrado");

        return ToDto(campo);
    }

    public async Task<CampoPersonalizadoDto> AddAsync(CampoPersonalizadoDto dto)
    {
        var campo = new CampoPersonalizado
        {
            Rotulo = dto.Rotulo,
            Tipo = dto.Tipo,
            Opcoes = dto.Opcoes ?? "",
            Obrigatorio = dto.Obrigatorio,
            Ordem = dto.Ordem,
            AplicavelA = dto.AplicavelA,
            Ativo = dto.Ativo
        };

        _context.CamposPersonalizados.Add(campo);
        await _context.SaveChangesAsync();
        return ToDto(campo);
    }

    public async Task<CampoPersonalizadoDto> UpdateAsync(int id, CampoPersonalizadoDto dto)
    {
        var campo = await _context.CamposPersonalizados.FindAsync(id)
            ?? throw new KeyNotFoundException($"Campo personalizado com ID {id} não encontrado");

        campo.Rotulo = dto.Rotulo;
        campo.Tipo = dto.Tipo;
        campo.Opcoes = dto.Opcoes ?? "";
        campo.Obrigatorio = dto.Obrigatorio;
        campo.Ordem = dto.Ordem;
        campo.AplicavelA = dto.AplicavelA;
        campo.Ativo = dto.Ativo;

        await _context.SaveChangesAsync();
        return ToDto(campo);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var campo = await _context.CamposPersonalizados.FindAsync(id);
        if (campo == null) return false;

        _context.CamposPersonalizados.Remove(campo);
        await _context.SaveChangesAsync();
        return true;
    }

    private static CampoPersonalizadoDto ToDto(CampoPersonalizado c) => new()
    {
        Id = c.Id,
        Rotulo = c.Rotulo ?? "",
        Tipo = c.Tipo,
        Opcoes = c.Opcoes ?? "",
        Obrigatorio = c.Obrigatorio,
        Ordem = c.Ordem,
        AplicavelA = c.AplicavelA,
        Ativo = c.Ativo
    };
}
