using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Data;
using SistemaRH.Domain.Entities;
using SistemaRH.Domain.Enums;

namespace SistemaRH.Application.Services;

public class ContratoPJService : IContratoPJService
{
    private readonly RhDbContext _context;
    private readonly IMapper _mapper;

    public ContratoPJService(RhDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ContratoPJDto> CriarAsync(ContratoPJDto dto)
    {
        var contrato = new ContratoPJ
        {
            FuncionarioPJId = dto.FuncionarioPJId,
            EmpresaId = dto.EmpresaId,
            TipoContrato = dto.TipoContrato,
            DataInicio = dto.DataInicio,
            DataFim = dto.DataFim,
            ValorProjeto = dto.ValorProjeto,
            ValorHora = dto.ValorHora,
            ValorFixo = dto.ValorFixo,
            HorasMensais = dto.HorasMensais,
            Descricao = dto.Descricao,
            RpaBrancoConhecimento = dto.RpaBrancoConhecimento,
            DataCriacao = DateTime.Now
        };

        _context.ContratosPJ.Add(contrato);
        await _context.SaveChangesAsync();

        return _mapper.Map<ContratoPJDto>(contrato);
    }

    public async Task<ContratoPJDto> GetByIdAsync(int id)
    {
        var contrato = await _context.ContratosPJ.FindAsync(id)
            ?? throw new KeyNotFoundException($"Contrato {id} não encontrado");

        return _mapper.Map<ContratoPJDto>(contrato);
    }

    public async Task<IEnumerable<ContratoPJDto>> GetByFuncionarioAsync(int funcionarioPJId)
    {
        var contratos = await _context.ContratosPJ
            .Where(c => c.FuncionarioPJId == funcionarioPJId)
            .OrderByDescending(c => c.DataInicio)
            .ToListAsync();

        return _mapper.Map<List<ContratoPJDto>>(contratos);
    }

    public async Task<IEnumerable<ContratoPJDto>> GetByEmpresaAsync(int empresaId)
    {
        var contratos = await _context.ContratosPJ
            .Where(c => c.EmpresaId == empresaId)
            .OrderByDescending(c => c.DataInicio)
            .ToListAsync();

        return _mapper.Map<List<ContratoPJDto>>(contratos);
    }

    public async Task<ContratoPJDto> AtualizarAsync(int id, ContratoPJDto dto)
    {
        var contrato = await _context.ContratosPJ.FindAsync(id)
            ?? throw new KeyNotFoundException($"Contrato {id} não encontrado");

        contrato.TipoContrato = dto.TipoContrato;
        contrato.DataFim = dto.DataFim;
        contrato.ValorProjeto = dto.ValorProjeto;
        contrato.ValorHora = dto.ValorHora;
        contrato.ValorFixo = dto.ValorFixo;
        contrato.Descricao = dto.Descricao;
        contrato.DataAtualizacao = DateTime.Now;

        _context.ContratosPJ.Update(contrato);
        await _context.SaveChangesAsync();

        return _mapper.Map<ContratoPJDto>(contrato);
    }

    public async Task<bool> DeletarAsync(int id)
    {
        var contrato = await _context.ContratosPJ.FindAsync(id);
        if (contrato == null)
            return false;

        _context.ContratosPJ.Remove(contrato);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<decimal> CalcularCustoMensalAsync(int contratoId, int mes, int ano)
    {
        var contrato = await _context.ContratosPJ.FindAsync(contratoId)
            ?? throw new KeyNotFoundException($"Contrato {contratoId} não encontrado");

        return contrato.TipoContrato switch
        {
            TipoContratoPJ.Projeto => contrato.ValorProjeto ?? 0,
            TipoContratoPJ.Horista => CalcularHorista(contrato, mes, ano),
            TipoContratoPJ.Fixo => contrato.ValorFixo ?? 0,
            _ => 0
        };
    }

    private decimal CalcularHorista(ContratoPJ contrato, int mes, int ano)
    {
        // TODO: Buscar horas realizadas da RPA/NF
        var horasEstimadas = contrato.HorasMensais;
        return (contrato.ValorHora ?? 0) * horasEstimadas;
    }
}
