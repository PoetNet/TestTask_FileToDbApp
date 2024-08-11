using Database;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<FileToDbAppDbContext>(optionsBuilder =>
        {
            optionsBuilder.UseSqlServer(connectionString,
                x => x.MigrationsAssembly("Database.Migrations"));
        });

        return services;
    }
}

