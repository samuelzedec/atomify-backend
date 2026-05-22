using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Infrastructure.Persistence.Extensions;

public static class DbContextOptionsBuilderExtensions
{
    extension(DbContextOptionsBuilder builder)
    {
        public DbContextOptionsBuilder ConfigureDatabaseOptions<TContext>(
            IConfiguration configuration, 
            string schema) where TContext : DbContext
        {
            ConfigureNpgsql<TContext>(builder, configuration, schema);
            return builder;
        }
    }

    extension<TContext>(DbContextOptionsBuilder<TContext> builder)
        where TContext : DbContext
    {
        public DbContextOptionsBuilder<TContext> ConfigureDatabaseOptions(
            IConfiguration configuration,
            string schema)
        {
            ConfigureNpgsql<TContext>(builder, configuration, schema);
            return builder;
        }
    }

    private static void ConfigureNpgsql<TContext>(
        DbContextOptionsBuilder builder,
        IConfiguration configuration,
        string schema)
        where TContext : DbContext
    {
        builder.UseNpgsql(
            configuration.GetConnectionString("DefaultConnection"),
            npgsql => npgsql
                .MigrationsAssembly(typeof(TContext).Assembly.FullName)
                .MigrationsHistoryTable("__ef_migrations_history", schema));
    }
}