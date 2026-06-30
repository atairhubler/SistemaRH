using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SistemaRH.Data;

public class RhDbContextFactory : IDesignTimeDbContextFactory<RhDbContext>
{
    public RhDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RhDbContext>();
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SistemaRH", "sistemarh.db");

        Directory.CreateDirectory(Path.GetDirectoryName(dbPath) ?? "");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");

        return new RhDbContext(optionsBuilder.Options);
    }
}
