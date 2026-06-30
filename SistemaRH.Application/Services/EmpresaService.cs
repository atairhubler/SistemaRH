using Microsoft.EntityFrameworkCore;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Data;
using SistemaRH.Domain.Entities;

namespace SistemaRH.Application.Services;

public class EmpresaService : IEmpresaService
{
    private readonly RhDbContext _context;

    public EmpresaService(RhDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EmpresaDto>> GetAllAsync()
    {
        var empresas = await _context.Empresas
            .Include(e => e.Funcionarios)
            .ToListAsync();

        return empresas.Select(e => ToDto(e));
    }

    public async Task<EmpresaDto> GetByIdAsync(int id)
    {
        var empresa = await _context.Empresas
            .Include(e => e.Funcionarios)
            .FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new KeyNotFoundException($"Empresa com ID {id} não encontrada");

        return ToDto(empresa);
    }

    public async Task<EmpresaDto> AddAsync(EmpresaDto dto)
    {
        var empresa = new Empresa
        {
            RazaoSocial = dto.RazaoSocial,
            Cnpj = dto.Cnpj,
            InscricaoEstadual = dto.InscricaoEstadual ?? "",
            Endereco = dto.Endereco ?? "",
            Cidade = dto.Cidade ?? "",
            Estado = dto.Estado ?? "",
            Cep = dto.Cep ?? "",
            Telefone = dto.Telefone ?? "",
            Email = dto.Email ?? "",
            Ativa = true,
            DataCriacao = DateTime.Now
        };

        _context.Empresas.Add(empresa);
        await _context.SaveChangesAsync();
        return ToDto(empresa);
    }

    public async Task<EmpresaDto> UpdateAsync(int id, EmpresaDto dto)
    {
        var empresa = await _context.Empresas.FindAsync(id)
            ?? throw new KeyNotFoundException($"Empresa com ID {id} não encontrada");

        empresa.RazaoSocial = dto.RazaoSocial;
        empresa.Cnpj = dto.Cnpj;
        empresa.InscricaoEstadual = dto.InscricaoEstadual ?? "";
        empresa.Endereco = dto.Endereco ?? "";
        empresa.Cidade = dto.Cidade ?? "";
        empresa.Estado = dto.Estado ?? "";
        empresa.Cep = dto.Cep ?? "";
        empresa.Telefone = dto.Telefone ?? "";
        empresa.Email = dto.Email ?? "";
        empresa.DataAtualizacao = DateTime.Now;

        await _context.SaveChangesAsync();
        return ToDto(empresa);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var empresa = await _context.Empresas.FindAsync(id);
        if (empresa == null) return false;

        var temFuncionarios = await _context.Funcionarios.AnyAsync(f => f.EmpresaId == id);
        if (temFuncionarios)
            throw new InvalidOperationException("Não é possível excluir uma empresa com funcionários cadastrados.");

        _context.Empresas.Remove(empresa);
        await _context.SaveChangesAsync();
        return true;
    }

    private static EmpresaDto ToDto(Empresa e) => new()
    {
        Id = e.Id,
        RazaoSocial = e.RazaoSocial ?? "",
        Cnpj = e.Cnpj ?? "",
        InscricaoEstadual = e.InscricaoEstadual ?? "",
        Endereco = e.Endereco ?? "",
        Cidade = e.Cidade ?? "",
        Estado = e.Estado ?? "",
        Cep = e.Cep ?? "",
        Telefone = e.Telefone ?? "",
        Email = e.Email ?? "",
        Ativa = e.Ativa,
        DataCriacao = e.DataCriacao,
        TotalFuncionarios = e.Funcionarios?.Count ?? 0
    };
}
