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
                FuncionarioEstagiario est => _mapper.Map<FuncionarioEstagiarioDto>(est),
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

    public async Task<IEnumerable<FuncionarioEstagiarioDto>> GetAllEstagiarioAsync()
    {
        var estagiarios = await _context.FuncionariosEstagiario.ToListAsync();
        return _mapper.Map<List<FuncionarioEstagiarioDto>>(estagiarios);
    }

    public async Task<FuncionarioDto> GetByIdAsync(int id)
    {
        var funcionario = await _context.Funcionarios.FindAsync(id);

        return funcionario switch
        {
            null => throw new KeyNotFoundException($"Funcionário com ID {id} não encontrado"),
            FuncionarioCLT clt => _mapper.Map<FuncionarioCLTDto>(clt),
            FuncionarioPJ pj => _mapper.Map<FuncionarioPJDto>(pj),
            FuncionarioEstagiario est => _mapper.Map<FuncionarioEstagiarioDto>(est),
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
            Rg = dto.Rg ?? "",
            Codigo = dto.Codigo ?? "",
            Email = dto.Email ?? "",
            EmailEmpresa = dto.EmailEmpresa ?? "",
            Contratante = dto.Contratante ?? "",
            Telefone = dto.Telefone ?? "",
            Endereco = dto.Endereco ?? "",
            Complemento = dto.Complemento ?? "",
            Cidade = dto.Cidade ?? "",
            Estado = dto.Estado ?? "",
            Cep = dto.Cep ?? "",
            Genero = dto.Genero ?? "",
            Ramal = dto.Ramal ?? "",
            DadosBancarios = dto.DadosBancarios ?? "",
            Comissionado = dto.Comissionado,
            Observacoes = dto.Observacoes ?? "",
            AjudaDeCusto = dto.AjudaDeCusto,
            CamposPersonalizadosJson = dto.CamposPersonalizadosJson ?? "{}",
            Cargo = dto.Cargo ?? "",
            Departamento = dto.Departamento ?? "",
            DataAdmissao = dto.DataAdmissao,
            DataDemissao = dto.DataDemissao,
            DataNascimento = dto.DataNascimento,
            Ctps = dto.Ctps ?? "",
            PisPassep = dto.PisPassep ?? "",
            SalarioBruto = dto.SalarioBruto,
            ComplementoSalarial = dto.ComplementoSalarial,
            AuxilioEducacao = dto.AuxilioEducacao,
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
            EmailEmpresa = dto.EmailEmpresa ?? "",
            Contratante = dto.Contratante ?? "",
            Telefone = dto.Telefone ?? "",
            TelefoneAgil = dto.TelefoneAgil ?? "",
            Endereco = dto.Endereco ?? "",
            Complemento = dto.Complemento ?? "",
            Cidade = dto.Cidade ?? "",
            Estado = dto.Estado ?? "",
            Cep = dto.Cep ?? "",
            Genero = dto.Genero ?? "",
            Ramal = dto.Ramal ?? "",
            DadosBancarios = dto.DadosBancarios ?? "",
            Comissionado = dto.Comissionado,
            Observacoes = dto.Observacoes ?? "",
            AjudaDeCusto = dto.AjudaDeCusto,
            TemDireitoFerias = dto.TemDireitoFerias,
            FeriasRemuneradas = dto.FeriasRemuneradas,
            DataNascimento = dto.DataNascimento,
            DataInicio = dto.DataInicio,
            DataFim = dto.DataFim,
            ValidadeContrato = dto.ValidadeContrato ?? "",
            TipoServico = dto.TipoServico ?? "",
            EmiteNF = dto.EmiteNF,
            DiaSolicitacaoNF = dto.DiaSolicitacaoNF ?? "",
            ValorServico = dto.ValorServico,
            ValorContratado = dto.ValorContratado,
            Departamento = dto.Departamento ?? "",
            CamposPersonalizadosJson = dto.CamposPersonalizadosJson ?? "{}",
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

    public async Task<FuncionarioEstagiarioDto> AddEstagiarioAsync(FuncionarioEstagiarioDto dto)
    {
        var empresaId = await ObterOuCriarEmpresaPadraoAsync(dto.EmpresaId);

        var estagiario = new FuncionarioEstagiario
        {
            Nome = dto.Nome,
            Cpf = dto.Cpf ?? "",
            Rg = dto.Rg ?? "",
            Codigo = dto.Codigo ?? "",
            Email = dto.Email ?? "",
            EmailEmpresa = dto.EmailEmpresa ?? "",
            Contratante = dto.Contratante ?? "",
            Telefone = dto.Telefone ?? "",
            Endereco = dto.Endereco ?? "",
            Complemento = dto.Complemento ?? "",
            Cidade = dto.Cidade ?? "",
            Estado = dto.Estado ?? "",
            Cep = dto.Cep ?? "",
            Genero = dto.Genero ?? "",
            Ramal = dto.Ramal ?? "",
            DadosBancarios = dto.DadosBancarios ?? "",
            Comissionado = dto.Comissionado,
            Observacoes = dto.Observacoes ?? "",
            AjudaDeCusto = dto.AjudaDeCusto,
            Cargo = dto.Cargo ?? "",
            Departamento = dto.Departamento ?? "",
            DataAdmissao = dto.DataAdmissao,
            DataDemissao = dto.DataDemissao,
            DataNascimento = dto.DataNascimento,
            Bolsa = dto.Bolsa,
            ComplementoSalarial = dto.ComplementoSalarial,
            CamposPersonalizadosJson = dto.CamposPersonalizadosJson ?? "{}",
            EmpresaId = empresaId,
            Status = StatusFuncionario.Ativo,
            Tipo = TipoFuncionario.Estagiario,
            DataCriacao = DateTime.Now
        };

        _context.FuncionariosEstagiario.Add(estagiario);

        var ferias = new Ferias
        {
            Funcionario = estagiario,
            DataAdmissao = estagiario.DataAdmissao,
            DiasDisponiveis = 30,
            DataCriacao = DateTime.Now
        };
        _context.Ferias.Add(ferias);

        await _context.SaveChangesAsync();
        return _mapper.Map<FuncionarioEstagiarioDto>(estagiario);
    }

    public async Task<FuncionarioCLTDto> UpdateCltAsync(int id, FuncionarioCLTDto dto)
    {
        var clt = await _context.FuncionariosCLT.FindAsync(id)
            ?? throw new KeyNotFoundException($"CLT com ID {id} não encontrado");

        clt.Nome = dto.Nome;
        clt.Cpf = dto.Cpf ?? "";
        clt.Rg = dto.Rg ?? "";
        clt.Codigo = dto.Codigo ?? "";
        clt.Email = dto.Email ?? "";
        clt.EmailEmpresa = dto.EmailEmpresa ?? "";
        clt.Contratante = dto.Contratante ?? "";
        clt.Telefone = dto.Telefone ?? "";
        clt.Endereco = dto.Endereco ?? "";
        clt.Complemento = dto.Complemento ?? "";
        clt.Cidade = dto.Cidade ?? "";
        clt.Estado = dto.Estado ?? "";
        clt.Cep = dto.Cep ?? "";
        clt.Genero = dto.Genero ?? "";
        clt.Ramal = dto.Ramal ?? "";
        clt.DadosBancarios = dto.DadosBancarios ?? "";
        clt.Comissionado = dto.Comissionado;
        clt.Observacoes = dto.Observacoes ?? "";
        clt.AjudaDeCusto = dto.AjudaDeCusto;
        clt.Cargo = dto.Cargo ?? "";
        clt.Departamento = dto.Departamento ?? "";
        clt.SalarioBruto = dto.SalarioBruto;
        clt.ComplementoSalarial = dto.ComplementoSalarial;
        clt.AuxilioEducacao = dto.AuxilioEducacao;
        clt.DataAdmissao = dto.DataAdmissao;
        clt.DataDemissao = dto.DataDemissao;
        clt.DataNascimento = dto.DataNascimento;
        clt.Ctps = dto.Ctps ?? "";
        clt.PisPassep = dto.PisPassep ?? "";
        clt.CamposPersonalizadosJson = dto.CamposPersonalizadosJson ?? "{}";
        clt.EmpresaId = dto.EmpresaId;
        clt.DataAtualizacao = DateTime.Now;

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
        pj.EmailEmpresa = dto.EmailEmpresa ?? "";
        pj.Contratante = dto.Contratante ?? "";
        pj.Telefone = dto.Telefone ?? "";
        pj.TelefoneAgil = dto.TelefoneAgil ?? "";
        pj.Endereco = dto.Endereco ?? "";
        pj.Complemento = dto.Complemento ?? "";
        pj.Cidade = dto.Cidade ?? "";
        pj.Estado = dto.Estado ?? "";
        pj.Cep = dto.Cep ?? "";
        pj.Genero = dto.Genero ?? "";
        pj.Ramal = dto.Ramal ?? "";
        pj.DadosBancarios = dto.DadosBancarios ?? "";
        pj.Comissionado = dto.Comissionado;
        pj.Observacoes = dto.Observacoes ?? "";
        pj.AjudaDeCusto = dto.AjudaDeCusto;
        pj.TemDireitoFerias = dto.TemDireitoFerias;
        pj.FeriasRemuneradas = dto.FeriasRemuneradas;
        pj.DataNascimento = dto.DataNascimento;
        pj.DataInicio = dto.DataInicio;
        pj.DataFim = dto.DataFim;
        pj.ValidadeContrato = dto.ValidadeContrato ?? "";
        pj.TipoServico = dto.TipoServico ?? "";
        pj.EmiteNF = dto.EmiteNF;
        pj.DiaSolicitacaoNF = dto.DiaSolicitacaoNF ?? "";
        pj.ValorServico = dto.ValorServico;
        pj.ValorContratado = dto.ValorContratado;
        pj.Departamento = dto.Departamento ?? "";
        pj.CamposPersonalizadosJson = dto.CamposPersonalizadosJson ?? "{}";
        pj.EmpresaId = dto.EmpresaId;
        pj.DataAtualizacao = DateTime.Now;

        await _context.SaveChangesAsync();

        return _mapper.Map<FuncionarioPJDto>(pj);
    }

    public async Task<FuncionarioEstagiarioDto> UpdateEstagiarioAsync(int id, FuncionarioEstagiarioDto dto)
    {
        var estagiario = await _context.FuncionariosEstagiario.FindAsync(id)
            ?? throw new KeyNotFoundException($"Estagiário com ID {id} não encontrado");

        estagiario.Nome = dto.Nome;
        estagiario.Cpf = dto.Cpf ?? "";
        estagiario.Rg = dto.Rg ?? "";
        estagiario.Codigo = dto.Codigo ?? "";
        estagiario.Email = dto.Email ?? "";
        estagiario.EmailEmpresa = dto.EmailEmpresa ?? "";
        estagiario.Contratante = dto.Contratante ?? "";
        estagiario.Telefone = dto.Telefone ?? "";
        estagiario.Endereco = dto.Endereco ?? "";
        estagiario.Complemento = dto.Complemento ?? "";
        estagiario.Cidade = dto.Cidade ?? "";
        estagiario.Estado = dto.Estado ?? "";
        estagiario.Cep = dto.Cep ?? "";
        estagiario.Genero = dto.Genero ?? "";
        estagiario.Ramal = dto.Ramal ?? "";
        estagiario.DadosBancarios = dto.DadosBancarios ?? "";
        estagiario.Comissionado = dto.Comissionado;
        estagiario.Observacoes = dto.Observacoes ?? "";
        estagiario.AjudaDeCusto = dto.AjudaDeCusto;
        estagiario.Cargo = dto.Cargo ?? "";
        estagiario.Departamento = dto.Departamento ?? "";
        estagiario.Bolsa = dto.Bolsa;
        estagiario.ComplementoSalarial = dto.ComplementoSalarial;
        estagiario.DataAdmissao = dto.DataAdmissao;
        estagiario.DataDemissao = dto.DataDemissao;
        estagiario.DataNascimento = dto.DataNascimento;
        estagiario.CamposPersonalizadosJson = dto.CamposPersonalizadosJson ?? "{}";
        estagiario.EmpresaId = dto.EmpresaId;
        estagiario.DataAtualizacao = DateTime.Now;

        await _context.SaveChangesAsync();

        return _mapper.Map<FuncionarioEstagiarioDto>(estagiario);
    }

    public async Task<bool> AtivarAsync(int id)
    {
        var funcionario = await _context.Funcionarios.FindAsync(id);
        if (funcionario == null) return false;

        funcionario.Status = StatusFuncionario.Ativo;
        funcionario.DataAtualizacao = DateTime.Now;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DesativarAsync(int id)
    {
        var funcionario = await _context.Funcionarios.FindAsync(id);
        if (funcionario == null) return false;

        funcionario.Status = funcionario is FuncionarioPJ
            ? StatusFuncionario.ContratoEncerrado
            : StatusFuncionario.Demitido;
        funcionario.DataAtualizacao = DateTime.Now;
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
                FuncionarioEstagiario est => _mapper.Map<FuncionarioEstagiarioDto>(est),
                _ => throw new InvalidOperationException("Tipo desconhecido")
            });
        }

        return result;
    }
}
