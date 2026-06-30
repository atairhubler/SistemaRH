using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Data;
using SistemaRH.Domain.Entities;
using SistemaRH.Domain.Enums;

namespace SistemaRH.Application.Services;

public class FuncionarioService : IFuncionarioService
{
    private readonly RhDbContext _context;
    private readonly IMapper _mapper;

    public FuncionarioService(RhDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<FuncionarioDto>> GetAllAsync()
    {
        var funcionarios = await _context.Funcionarios.ToListAsync();
        var result = new List<FuncionarioDto>();

        foreach (var f in funcionarios)
        {
            result.Add(f switch
            {
                FuncionarioCLT clt => _mapper.Map<FuncionarioCLTDto>(clt),
                FuncionarioPJ pj => _mapper.Map<FuncionarioPJDto>(pj),
                _ => throw new InvalidOperationException("Tipo desconhecido")
            });
        }

        return result;
    }

    public async Task<IEnumerable<FuncionarioCLTDto>> GetAllCltAsync()
    {
        var clts = await _context.FuncionariosCLT.ToListAsync();
        return _mapper.Map<List<FuncionarioCLTDto>>(clts);
    }

    public async Task<IEnumerable<FuncionarioPJDto>> GetAllPJAsync()
    {
        var pjs = await _context.FuncionariosPJ.ToListAsync();
        return _mapper.Map<List<FuncionarioPJDto>>(pjs);
    }

    public async Task<FuncionarioDto> GetByIdAsync(int id)
    {
        var funcionario = await _context.Funcionarios.FindAsync(id);

        return funcionario switch
        {
            null => throw new KeyNotFoundException($"Funcionário com ID {id} não encontrado"),
            FuncionarioCLT clt => _mapper.Map<FuncionarioCLTDto>(clt),
            FuncionarioPJ pj => _mapper.Map<FuncionarioPJDto>(pj),
            _ => throw new InvalidOperationException("Tipo desconhecido")
        };
    }

    private async Task<int> ObterOuCriarEmpresaPadraoAsync(int empresaId)
    {
        if (empresaId > 0)
        {
            var existe = await _context.Empresas.AnyAsync(e => e.Id == empresaId);
            if (existe) return empresaId;
        }

        var empresa = await _context.Empresas.FirstOrDefaultAsync();
        if (empresa != null) return empresa.Id;

        var nova = new Empresa
        {
            RazaoSocial = "Empresa Principal",
            Cnpj = "00.000.000/0001-00",
            InscricaoEstadual = "",
            Endereco = "",
            Cidade = "",
            Estado = "",
            Cep = "",
            Telefone = "",
            Email = "",
            Ativa = true,
            DataCriacao = DateTime.Now
        };
        _context.Empresas.Add(nova);
        await _context.SaveChangesAsync();
        return nova.Id;
    }

    public async Task<FuncionarioCLTDto> AddCltAsync(FuncionarioCLTDto dto)
    {
        var empresaId = await ObterOuCriarEmpresaPadraoAsync(dto.EmpresaId);

        var clt = new FuncionarioCLT
        {
            Nome = dto.Nome,
            Cpf = dto.Cpf ?? "",
            Email = dto.Email ?? "",
            Telefone = dto.Telefone ?? "",
            Endereco = dto.Endereco ?? "",
            Cidade = dto.Cidade ?? "",
            Estado = dto.Estado ?? "",
            Cep = dto.Cep ?? "",
            Cargo = dto.Cargo ?? "",
            Departamento = dto.Departamento ?? "",
            DataAdmissao = dto.DataAdmissao,
            DataNascimento = dto.DataNascimento,
            Ctps = dto.Ctps ?? "",
            PisPassep = dto.PisPassep ?? "",
            SalarioBruto = dto.SalarioBruto,
            EmpresaId = empresaId,
            Status = StatusFuncionario.Ativo,
            Tipo = TipoFuncionario.CLT,
            DataCriacao = DateTime.Now
        };

        _context.FuncionariosCLT.Add(clt);

        // Usar navigation property — EF resolve o FK automaticamente após salvar clt
        var ferias = new Ferias
        {
            Funcionario = clt,
            DataAdmissao = clt.DataAdmissao,
            DiasDisponiveis = 30,
            DataCriacao = DateTime.Now
        };
        _context.Ferias.Add(ferias);

        await _context.SaveChangesAsync();
        return _mapper.Map<FuncionarioCLTDto>(clt);
    }

    public async Task<FuncionarioPJDto> AddPJAsync(FuncionarioPJDto dto)
    {
        var empresaId = await ObterOuCriarEmpresaPadraoAsync(dto.EmpresaId);

        var pj = new FuncionarioPJ
        {
            Nome = dto.Nome,
            Cnpj = dto.Cnpj ?? "",
            RazaoSocial = dto.RazaoSocial ?? dto.Nome ?? "",
            Email = dto.Email ?? "",
            Telefone = dto.Telefone ?? "",
            Endereco = dto.Endereco ?? "",
            Cidade = dto.Cidade ?? "",
            Estado = dto.Estado ?? "",
            Cep = dto.Cep ?? "",
            TemDireitoFerias = dto.TemDireitoFerias,
            FeriasRemuneradas = dto.FeriasRemuneradas,
            EmpresaId = empresaId,
            Status = StatusFuncionario.Ativo,
            Tipo = TipoFuncionario.PJ,
            DataCriacao = DateTime.Now
        };

        _context.FuncionariosPJ.Add(pj);

        // Usar navigation property — EF resolve o FK automaticamente após salvar pj
        var ferias = new Ferias
        {
            Funcionario = pj,
            DataAdmissao = DateTime.Now,
            DiasDisponiveis = pj.TemDireitoFerias ? 30 : 0,
            DataCriacao = DateTime.Now
        };
        _context.Ferias.Add(ferias);

        await _context.SaveChangesAsync();
        return _mapper.Map<FuncionarioPJDto>(pj);
    }

    public async Task<FuncionarioCLTDto> UpdateCltAsync(int id, FuncionarioCLTDto dto)
    {
        var clt = await _context.FuncionariosCLT.FindAsync(id)
            ?? throw new KeyNotFoundException($"CLT com ID {id} não encontrado");

        clt.Nome = dto.Nome;
        clt.Cpf = dto.Cpf ?? "";
        clt.Email = dto.Email ?? "";
        clt.Telefone = dto.Telefone ?? "";
        clt.Cargo = dto.Cargo ?? "";
        clt.Departamento = dto.Departamento ?? "";
        clt.SalarioBruto = dto.SalarioBruto;
        clt.DataAdmissao = dto.DataAdmissao;
        clt.Ctps = dto.Ctps ?? "";
        clt.PisPassep = dto.PisPassep ?? "";
        clt.EmpresaId = dto.EmpresaId;
        clt.DataAtualizacao = DateTime.Now;

        _context.FuncionariosCLT.Update(clt);
        await _context.SaveChangesAsync();

        return _mapper.Map<FuncionarioCLTDto>(clt);
    }

    public async Task<FuncionarioPJDto> UpdatePJAsync(int id, FuncionarioPJDto dto)
    {
        var pj = await _context.FuncionariosPJ.FindAsync(id)
            ?? throw new KeyNotFoundException($"PJ com ID {id} não encontrado");

        pj.Nome = dto.Nome ?? "";
        pj.RazaoSocial = dto.RazaoSocial ?? dto.Nome ?? "";
        pj.Cnpj = dto.Cnpj ?? "";
        pj.Email = dto.Email ?? "";
        pj.Telefone = dto.Telefone ?? "";
        pj.TemDireitoFerias = dto.TemDireitoFerias;
        pj.FeriasRemuneradas = dto.FeriasRemuneradas;
        pj.EmpresaId = dto.EmpresaId;
        pj.DataAtualizacao = DateTime.Now;

        _context.FuncionariosPJ.Update(pj);
        await _context.SaveChangesAsync();

        return _mapper.Map<FuncionarioPJDto>(pj);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var funcionario = await _context.Funcionarios.FindAsync(id);
        if (funcionario == null)
            return false;

        _context.Funcionarios.Remove(funcionario);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<FuncionarioDto>> GetByEmpresaAsync(int empresaId)
    {
        var funcionarios = await _context.Funcionarios
            .Where(f => f.EmpresaId == empresaId)
            .ToListAsync();

        var result = new List<FuncionarioDto>();

        foreach (var f in funcionarios)
        {
            result.Add(f switch
            {
                FuncionarioCLT clt => _mapper.Map<FuncionarioCLTDto>(clt),
                FuncionarioPJ pj => _mapper.Map<FuncionarioPJDto>(pj),
                _ => throw new InvalidOperationException("Tipo desconhecido")
            });
        }

        return result;
    }
}
