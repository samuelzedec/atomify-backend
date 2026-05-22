using BuildingBlocks.Infrastructure.Persistence.Factories;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence.Factories;

file sealed class IdentityDbContextFactory
    : DbContextFactory<IdentityDbContext>
{
    protected override string UserSecretsId
        => "72886f70-a8e3-4d08-b562-a028ff3bed82";

    protected override string Schema
        => "identity";

    protected override IdentityDbContext CreateContext(DbContextOptions<IdentityDbContext> options)
        => new(options);
}