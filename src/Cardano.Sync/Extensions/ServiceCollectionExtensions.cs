using Cardano.Sync.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Cardano.Sync.Data;

namespace Cardano.Sync.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCardanoIndexer<T>(this IServiceCollection services, IConfiguration configuration) where T : CardanoDbContext
    {
        services.AddDbContextFactory<T>(options =>
        {
            options
            .UseNpgsql(
                configuration.GetConnectionString("CardanoContext"),
                    x =>
                    {
                        x.CommandTimeout(60);
                        x.MigrationsHistoryTable(
                            "__EFMigrationsHistory",
                            configuration.GetConnectionString("CardanoContextSchema")
                        );
                    }
                );
        });

        services.AddHostedService<CardanoIndexWorker<T>>();

        return services;
    }
}