using BuildingBlocks.Infrastructure.Persistence.Extensions;
using Identity.Application;
using Identity.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Identity.Infrastructure;

public static class DependencyInjection
{
    extension(IHostApplicationBuilder builder)
    {
        public void ConfigureIdentityModule()
        {
            builder.ConfigureDbContext();
            builder.Services.ConfigureApplicationLayer();
        }

        private void ConfigureDbContext()
        {
            var isDevelopment = builder.Environment
                .IsDevelopment();

            builder.Services
                .AddDbContext<IdentityDbContext>(options => options
                    .ConfigureDatabaseOptions<IdentityDbContext>(builder.Configuration, "identity")
                    .EnableSensitiveDataLogging(isDevelopment)
                    .EnableDetailedErrors(isDevelopment));
        }
    }
}