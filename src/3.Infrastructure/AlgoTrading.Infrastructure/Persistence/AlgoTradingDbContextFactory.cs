using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AlgoTrading.Infrastructure.Persistence;

/// <summary>
/// Design-time DbContext Factory for EF Core migrations
/// </summary>
public class AlgoTradingDbContextFactory : IDesignTimeDbContextFactory<AlgoTradingDbContext>
{
    public AlgoTradingDbContext CreateDbContext(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        // Get connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Database=algo_trading;Username=postgres;Password=postgres;Port=5432";

        // Build DbContextOptions
        var optionsBuilder = new DbContextOptionsBuilder<AlgoTradingDbContext>();
        optionsBuilder.UseNpgsql(connectionString,
            npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(AlgoTradingDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
            });

        return new AlgoTradingDbContext(optionsBuilder.Options);
    }
}
