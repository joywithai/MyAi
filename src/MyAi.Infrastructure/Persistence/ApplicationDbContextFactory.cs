using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MyAi.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for `dotnet ef migrations add`.
/// Usage: dotnet ef migrations add Init -p src/MyAi.Infrastructure -s src/MyAi.Api
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=myai;Username=myai;Password=myai_secret");
        return new ApplicationDbContext(optionsBuilder.Options, new ServiceContainer());
    }

    private sealed class ServiceContainer : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}
