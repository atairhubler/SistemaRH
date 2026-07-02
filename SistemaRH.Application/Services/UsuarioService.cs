using Microsoft.EntityFrameworkCore;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Data;
using SistemaRH.Domain.Entities;

namespace SistemaRH.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly RhDbContext _context;

    public UsuarioService(RhDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UsuarioDto>> GetAllAsync()
    {
        var usuarios = await _context.Usuarios.OrderBy(u => u.NomeUsuario).ToListAsync();
        return usuarios.Select(ToDto);
    }

    public async Task<bool> ExisteAlgumUsuarioAsync() => await _context.Usuarios.AnyAsync();

    public async Task<UsuarioDto> AddAsync(string nomeUsuario, string senha)
    {
        var usuario = new Usuario
        {
            NomeUsuario = nomeUsuario,
            SenhaHash = SenhaHasher.Hash(senha),
            Ativo = true,
            DataCriacao = DateTime.Now
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return ToDto(usuario);
    }

    public async Task<UsuarioDto> UpdateAsync(int id, string nomeUsuario, string? novaSenha, bool ativo)
    {
        var usuario = await _context.Usuarios.FindAsync(id)
            ?? throw new KeyNotFoundException($"Usuário com ID {id} não encontrado");

        usuario.NomeUsuario = nomeUsuario;
        usuario.Ativo = ativo;
        if (!string.IsNullOrWhiteSpace(novaSenha))
            usuario.SenhaHash = SenhaHasher.Hash(novaSenha);
        usuario.DataAtualizacao = DateTime.Now;

        await _context.SaveChangesAsync();
        return ToDto(usuario);
    }

    public async Task<UsuarioDto?> ValidarCredenciaisAsync(string nomeUsuario, string senha)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.NomeUsuario == nomeUsuario);

        if (usuario == null || !usuario.Ativo) return null;
        if (!SenhaHasher.Verificar(senha, usuario.SenhaHash)) return null;

        return ToDto(usuario);
    }

    private static UsuarioDto ToDto(Usuario u) => new()
    {
        Id = u.Id,
        NomeUsuario = u.NomeUsuario,
        Ativo = u.Ativo,
        DataCriacao = u.DataCriacao,
        DataAtualizacao = u.DataAtualizacao
    };
}
