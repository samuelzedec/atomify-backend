using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

namespace Admin.Api.Setups;

internal static class PipelineSetup
{
    extension(WebApplication app)
    {
        public void Configure()
        {
            app.UseRateLimiter();
            app.ConfigureApiDocumentation();
            app.ConfigureHealthCheck();
            app.UseExceptionHandler();
        }

        private void ConfigureApiDocumentation()
        {
            if (!app.Environment.IsDevelopment())
                return;

            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options.Title = "Atomify Admin API";
                options.WithClassicLayout();
            });
        }

        private void ConfigureHealthCheck()
        {
            app.MapHealthChecks("/health",
                new HealthCheckOptions { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });
        }
    }
}