using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SistemaRH.Application.DTOs;
using SistemaRH.Application.Interfaces;
using SistemaRH.Data;
using SistemaRH.Domain.Entities;

namespace SistemaRH.Application.Services;

public class AtestadoService : IAtestadoService
{
    private readonly RhDbContext _context;
    private readonly IMapper _mapper;

    public AtestadoService(RhDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AtestadoDto>> GetByFuncionarioAsync(int funcionarioId)
    {
        var atestados = await _context.Atestados
            .Where(a => a.FuncionarioId == funcionarioId)
            .OrderByDescending(a => a.DataInicio)
            .ToListAsync();

        return _mapper.Map<List<AtestadoDto>>(atestados);
    }

    public async Task<AtestadoDto> AdicionarAsync(AtestadoDto dto, Stream arquivo, string nomeArquivo)
    {
        using var memoryStream = new MemoryStream();
        await arquivo.CopyToAsync(memoryStream);

        var atestado = new Atestado
        {
            FuncionarioId = dto.FuncionarioId,
            DataAtestado = dto.DataAtestado,
            DataInicio = dto.DataInicio,
            DataFim = dto.DataFim,
            DiasFaltados = dto.DiasFaltados,
            Tipo = dto.Tipo,
            Descricao = dto.Descricao,
            Cid = dto.Cid,
            Observacoes = dto.Observacoes,
            Arquivo = memoryStream.ToArray(),
            NomeArquivo = nomeArquivo,
            DataCriacao = DateTime.Now
        };

        _context.Atestados.Add(atestado);
        await _context.SaveChangesAsync();

        return _mapper.Map<AtestadoDto>(atestado);
    }

    public async Task RemoverAsync(int id)
    {
        var atestado = await _context.Atestados.FindAsync(id)
            ?? throw new KeyNotFoundException($"Atestado {id} não encontrado");

        _context.Atestados.Remove(atestado);
        await _context.SaveChangesAsync();
    }

    public async Task<(byte[] Arquivo, string NomeArquivo)> ExportarArquivoAsync(int id)
    {
        var atestado = await _context.Atestados.FindAsync(id)
            ?? throw new KeyNotFoundException($"Atestado {id} não encontrado");

        if (atestado.Arquivo == null)
            throw new InvalidOperationException("Arquivo não disponível para download");

        return (atestado.Arquivo, atestado.NomeArquivo);
    }
}
