using Microsoft.EntityFrameworkCore;
using SistemaRH.Domain.Entities;
using SistemaRH.Domain.Enums;

namespace SistemaRH.Data;

public class RhDbContext : DbContext
{
    public DbSet<Empresa> Empresas { get; set; }
    public DbSet<Funcionario> Funcionarios { get; set; }
    public DbSet<FuncionarioCLT> FuncionariosCLT { get; set; }
    public DbSet<FuncionarioPJ> FuncionariosPJ { get; set; }
    public DbSet<ContratoPJ> ContratosPJ { get; set; }
    public DbSet<RpaNfPJ> RpasNfsPJ { get; set; }
    public DbSet<Ferias> Ferias { get; set; }
    public DbSet<PeriodoFerias> PeriodosFerias { get; set; }
    public DbSet<SalarioHistorico> SalarioHistorico { get; set; }
    public DbSet<Atestado> Atestados { get; set; }

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
            .HasValue<FuncionarioPJ>(TipoFuncionario.PJ);

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
    }
}
