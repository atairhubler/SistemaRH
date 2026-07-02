using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SistemaRH.Domain.Entities;
using SistemaRH.Domain.Enums;

namespace SistemaRH.Data;

public class RhDbContext : DbContext
{
    public DbSet<Empresa> Empresas { get; set; }
    public DbSet<Funcionario> Funcionarios { get; set; }
    public DbSet<FuncionarioCLT> FuncionariosCLT { get; set; }
    public DbSet<FuncionarioPJ> FuncionariosPJ { get; set; }
    public DbSet<FuncionarioEstagiario> FuncionariosEstagiario { get; set; }
    public DbSet<ContratoPJ> ContratosPJ { get; set; }
    public DbSet<RpaNfPJ> RpasNfsPJ { get; set; }
    public DbSet<Ferias> Ferias { get; set; }
    public DbSet<PeriodoFerias> PeriodosFerias { get; set; }
    public DbSet<SalarioHistorico> SalarioHistorico { get; set; }
    public DbSet<Atestado> Atestados { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<LogAuditoria> LogsAuditoria { get; set; }

    public RhDbContext(DbContextOptions<RhDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SistemaRH", "sistemarh.db");

            Directory.CreateDirectory(Path.GetDirectoryName(dbPath) ?? "");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // TPH (Table Per Hierarchy) - Discriminator
        modelBuilder.Entity<Funcionario>()
            .HasDiscriminator<TipoFuncionario>("TipoFuncionario")
            .HasValue<FuncionarioCLT>(TipoFuncionario.CLT)
            .HasValue<FuncionarioPJ>(TipoFuncionario.PJ)
            .HasValue<FuncionarioEstagiario>(TipoFuncionario.Estagiario);

        // FuncionarioCLT e FuncionarioEstagiario compartilham as colunas dos campos em comum
        // (TPH) — é preciso configurar o MESMO HasColumnName nos dois lados, senão o EF Core
        // entende que são colunas diferentes e renomeia uma delas (ex.: "FuncionarioCLT_Cpf").
        modelBuilder.Entity<FuncionarioCLT>(e =>
        {
            e.Property(f => f.Cpf).HasColumnName("Cpf");
            e.Property(f => f.DataNascimento).HasColumnName("DataNascimento");
            e.Property(f => f.Cargo).HasColumnName("Cargo");
            e.Property(f => f.Departamento).HasColumnName("Departamento");
            e.Property(f => f.DataAdmissao).HasColumnName("DataAdmissao");
            e.Property(f => f.DataDemissao).HasColumnName("DataDemissao");
        });

        modelBuilder.Entity<FuncionarioEstagiario>(e =>
        {
            e.Property(f => f.Cpf).HasColumnName("Cpf");
            e.Property(f => f.DataNascimento).HasColumnName("DataNascimento");
            e.Property(f => f.Cargo).HasColumnName("Cargo");
            e.Property(f => f.Departamento).HasColumnName("Departamento");
            e.Property(f => f.DataAdmissao).HasColumnName("DataAdmissao");
            e.Property(f => f.DataDemissao).HasColumnName("DataDemissao");
        });

        // Relacionamentos Empresa -> Funcionario
        modelBuilder.Entity<Funcionario>()
            .HasOne(f => f.Empresa)
            .WithMany(e => e.Funcionarios)
            .HasForeignKey(f => f.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamentos Funcionario -> Ferias
        modelBuilder.Entity<Funcionario>()
            .HasOne(f => f.Ferias)
            .WithOne(fe => fe.Funcionario)
            .HasForeignKey<Ferias>(fe => fe.FuncionarioId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamentos Funcionario -> Atestado
        modelBuilder.Entity<Funcionario>()
            .HasMany(f => f.Atestados)
            .WithOne(a => a.Funcionario)
            .HasForeignKey(a => a.FuncionarioId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamentos FuncionarioCLT -> SalarioHistorico
        modelBuilder.Entity<FuncionarioCLT>()
            .HasMany(c => c.HistoricoSalario)
            .WithOne(s => s.Funcionario)
            .HasForeignKey(s => s.FuncionarioId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamentos FuncionarioPJ -> ContratoPJ
        modelBuilder.Entity<FuncionarioPJ>()
            .HasMany(p => p.Contratos)
            .WithOne(c => c.FuncionarioPJ)
            .HasForeignKey(c => c.FuncionarioPJId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamentos FuncionarioPJ -> RpaNfPJ
        modelBuilder.Entity<FuncionarioPJ>()
            .HasMany(p => p.RpasNfs)
            .WithOne(r => r.FuncionarioPJ)
            .HasForeignKey(r => r.FuncionarioPJId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamentos Ferias -> PeriodoFerias
        modelBuilder.Entity<Ferias>()
            .HasMany(f => f.Periodos)
            .WithOne(p => p.Ferias)
            .HasForeignKey(p => p.FeriasId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamentos ContratoPJ -> RpaNfPJ
        modelBuilder.Entity<ContratoPJ>()
            .HasMany(c => c.RpasNfs)
            .WithOne(r => r.Contrato)
            .HasForeignKey(r => r.ContratoPJId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamentos Empresa -> ContratoPJ
        modelBuilder.Entity<Empresa>()
            .HasMany(e => e.Funcionarios)
            .WithOne(f => f.Empresa)
            .HasForeignKey(f => f.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices de performance
        modelBuilder.Entity<Funcionario>()
            .HasIndex(f => new { f.EmpresaId, f.Tipo });

        modelBuilder.Entity<Funcionario>()
            .HasIndex(f => f.Tipo);

        modelBuilder.Entity<RpaNfPJ>()
            .HasIndex(r => new { r.Mes, r.Ano });

        modelBuilder.Entity<RpaNfPJ>()
            .HasIndex(r => r.StatusPagamento);

        modelBuilder.Entity<SalarioHistorico>()
            .HasIndex(s => s.FuncionarioId);

        modelBuilder.Entity<Ferias>()
            .HasIndex(f => f.FuncionarioId);

        modelBuilder.Entity<Atestado>()
            .HasIndex(a => new { a.FuncionarioId, a.DataInicio, a.DataFim });

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.NomeUsuario)
            .IsUnique();

        modelBuilder.Entity<LogAuditoria>()
            .HasIndex(l => l.DataHora);
    }

    private static readonly HashSet<string> CamposIgnoradosNoLog = new() { "Id", "DataAtualizacao" };

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        RegistrarLogsPendentes();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        RegistrarLogsPendentes();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void RegistrarLogsPendentes()
    {
        var logs = ConstruirLogsAuditoria();
        if (logs.Count > 0)
            LogsAuditoria.AddRange(logs);
    }

    private List<LogAuditoria> ConstruirLogsAuditoria()
    {
        var logs = new List<LogAuditoria>();
        var usuario = SessaoAtual.UsuarioAtual ?? "Desconhecido";

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is LogAuditoria) continue;
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted)) continue;

            string acao;
            string? valoresAntes = null;
            string? valoresDepois = null;

            if (entry.State == EntityState.Added)
            {
                acao = "Criação";
                valoresDepois = FormatarValores(entry, usarValorAtual: true);
            }
            else if (entry.State == EntityState.Deleted)
            {
                acao = "Exclusão";
                valoresAntes = FormatarValores(entry, usarValorAtual: false);
            }
            else
            {
                var propsModificadas = entry.Properties
                    .Where(p => p.IsModified && !CamposIgnoradosNoLog.Contains(p.Metadata.Name))
                    .ToList();
                if (propsModificadas.Count == 0) continue;

                var statusProp = propsModificadas.FirstOrDefault(p => p.Metadata.Name == nameof(Funcionario.Status));
                if (statusProp != null && entry.Entity is Funcionario)
                    acao = Equals(statusProp.CurrentValue, StatusFuncionario.Ativo) ? "Ativação" : "Desativação";
                else
                    acao = "Edição";

                valoresAntes = string.Join("\n", propsModificadas.Select(p => $"{p.Metadata.Name}: {p.OriginalValue}"));
                valoresDepois = string.Join("\n", propsModificadas.Select(p => $"{p.Metadata.Name}: {p.CurrentValue}"));
            }

            var nomeEntidade = NomeAmigavel(entry.Entity.GetType());
            var nomeExibicao = NomeExibicao(entry.Entity);
            var descricao = string.IsNullOrEmpty(nomeExibicao)
                ? $"{nomeEntidade} — {acao}"
                : $"{nomeEntidade} '{nomeExibicao}' — {acao}";

            logs.Add(new LogAuditoria
            {
                DataHora = DateTime.Now,
                Usuario = usuario,
                Acao = acao,
                Entidade = nomeEntidade,
                Descricao = descricao,
                ValoresAntes = valoresAntes,
                ValoresDepois = valoresDepois
            });
        }

        return logs;
    }

    private static string FormatarValores(EntityEntry entry, bool usarValorAtual)
    {
        var linhas = entry.Properties
            .Where(p => !CamposIgnoradosNoLog.Contains(p.Metadata.Name))
            .Select(p => $"{p.Metadata.Name}: {(usarValorAtual ? p.CurrentValue : p.OriginalValue)}");
        return string.Join("\n", linhas);
    }

    private static string NomeAmigavel(Type tipo) => tipo.Name switch
    {
        nameof(FuncionarioCLT) or nameof(FuncionarioPJ) or nameof(FuncionarioEstagiario) => "Funcionário",
        nameof(Empresa) => "Empresa",
        nameof(Ferias) => "Férias",
        nameof(PeriodoFerias) => "Período de Férias",
        nameof(Atestado) => "Atestado",
        nameof(ContratoPJ) => "Contrato PJ",
        nameof(RpaNfPJ) => "RPA/NF",
        nameof(SalarioHistorico) => "Histórico Salarial",
        nameof(Usuario) => "Usuário",
        _ => tipo.Name
    };

    private static string NomeExibicao(object entidade) => entidade switch
    {
        Funcionario f => f.Nome ?? "",
        Empresa e => e.RazaoSocial ?? "",
        Usuario u => u.NomeUsuario ?? "",
        _ => ""
    };
}
