using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Data;
using SistemaRH.Domain.Entities;

namespace SistemaRH.Application.Services;

public class RpaNfService : IRpaNfService
{
    private readonly RhDbContext _context;
    private readonly IMapper _mapper;

    public RpaNfService(RhDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<RpaNfDto> CriarRpaAsync(RpaNfDto dto, Stream arquivo)
    {
        using var memoryStream = new MemoryStream();
        await arquivo.CopyToAsync(memoryStream);

        var rpaNf = new RpaNfPJ
        {
            ContratoPJId = dto.ContratoPJId,
            FuncionarioPJId = dto.FuncionarioPJId,
            EmpresaId = dto.EmpresaId,
            TipoDocumento = dto.TipoDocumento,
            NumeroDocumento = dto.NumeroDocumento,
            DataDocumento = dto.DataDocumento,
            DataVencimento = dto.DataVencimento,
            Mes = dto.Mes,
            Ano = dto.Ano,
            ValorBruto = dto.ValorBruto,
            ValorDescontos = CalcularDescontos(dto.ValorBruto),
            ValorLiquido = dto.ValorBruto - CalcularDescontos(dto.ValorBruto),
            DescricaoServico = dto.DescricaoServico,
            Observacoes = dto.Observacoes,
            Arquivo = memoryStream.ToArray(),
            NomeArquivo = dto.NomeArquivo,
            DataCriacao = DateTime.Now
        };

        _context.RpasNfsPJ.Add(rpaNf);
        await _context.SaveChangesAsync();

        return _mapper.Map<RpaNfDto>(rpaNf);
    }

    public async Task<RpaNfDto> GetByIdAsync(int id)
    {
        var rpaNf = await _context.RpasNfsPJ.FindAsync(id)
            ?? throw new KeyNotFoundException($"RPA/NF {id} não encontrado");

        return _mapper.Map<RpaNfDto>(rpaNf);
    }

    public async Task<IEnumerable<RpaNfDto>> GetByCompetenciaAsync(int empresaId, int mes, int ano)
    {
        var rpasNfs = await _context.RpasNfsPJ
            .Where(r => r.EmpresaId == empresaId && r.Mes == mes && r.Ano == ano)
            .OrderByDescending(r => r.DataDocumento)
            .ToListAsync();

        return _mapper.Map<List<RpaNfDto>>(rpasNfs);
    }

    public async Task<IEnumerable<RpaNfDto>> GetByFuncionarioAsync(int funcionarioPJId)
    {
        var rpasNfs = await _context.RpasNfsPJ
            .Where(r => r.FuncionarioPJId == funcionarioPJId)
            .OrderByDescending(r => r.DataDocumento)
            .ToListAsync();

        return _mapper.Map<List<RpaNfDto>>(rpasNfs);
    }

    public async Task<IEnumerable<RpaNfDto>> GetByStatusAsync(int statusPagamento)
    {
        var rpasNfs = await _context.RpasNfsPJ
            .Where(r => (int)r.StatusPagamento == statusPagamento)
            .OrderByDescending(r => r.DataVencimento)
            .ToListAsync();

        return _mapper.Map<List<RpaNfDto>>(rpasNfs);
    }

    public async Task<bool> AtualizarStatusAsync(int id, int novoStatus)
    {
        var rpaNf = await _context.RpasNfsPJ.FindAsync(id);
        if (rpaNf == null)
            return false;

        rpaNf.StatusPagamento = (Domain.Enums.StatusPagamentoPJ)novoStatus;
        rpaNf.DataAtualizacao = DateTime.Now;

        if (novoStatus == 2) // Pago
            rpaNf.DataPagamento = DateTime.Now;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<byte[]> ExportarPdfAsync(int rpaNfId)
    {
        var rpaNf = await _context.RpasNfsPJ.FindAsync(rpaNfId)
            ?? throw new KeyNotFoundException($"RPA/NF {rpaNfId} não encontrado");

        if (rpaNf.Arquivo == null)
            throw new InvalidOperationException("Arquivo não disponível para download");

        return rpaNf.Arquivo;
    }

    private decimal CalcularDescontos(decimal valorBruto)
    {
        // PJ: INSS autônomo simplificado (11%)
        return valorBruto * 0.11m;
    }
}
