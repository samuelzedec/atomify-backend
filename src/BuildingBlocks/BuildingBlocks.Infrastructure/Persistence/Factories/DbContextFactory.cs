using BuildingBlocks.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Infrastructure.Persistence.Factories;

public abstract class DbContextFactory<TContext>
    : IDesignTimeDbContextFactory<TContext> where TContext : DbContext
{
    /// <summary>
    /// Obtém o identificador exclusivo usado para acessar os segredos do usuário definidos no código.
    /// </summary>
    /// <remarks>
    /// Este valor é utilizado para associar as configurações de segredos do usuário ao componente atual,
    /// permitindo o carregamento de credenciais ou outras informações sensíveis armazenadas de forma local
    /// no ambiente de desenvolvimento. Ele deve ser configurado corretamente para habilitar a funcionalidade
    /// de gerenciamento seguro de segredos.
    /// </remarks>
    protected abstract string UserSecretsId { get; }

    /// <summary>
    /// Obtém o nome do esquema utilizado para configurar a tabela de histórico de migrações no banco de dados.
    /// </summary>
    /// <remarks>
    /// O valor retorna uma string que representa o esquema usado como parte da configuração do banco de dados.
    /// Ele é utilizado para organizar as tabelas dentro do banco de dados e determinar qual esquema será aplicado
    /// à tabela de histórico de migrações do Entity Framework.
    /// </remarks>
    protected abstract string Schema { get; }

    /// <summary>
    /// Cria uma instância do contexto de banco de dados do tipo especificado.
    /// </summary>
    /// <param name="options">
    /// Objeto contendo as opções de configuração para o contexto do banco de dados.
    /// </param>
    /// <returns>
    /// Uma instância de <typeparamref name="TContext"/> inicializada com as opções fornecidas.
    /// </returns>
    protected abstract TContext CreateContext(DbContextOptions<TContext> options);

    public TContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets(UserSecretsId)
            .Build();

        var builder = new DbContextOptionsBuilder<TContext>()
            .ConfigureDatabaseOptions(configuration, Schema);

        return CreateContext(builder.Options);
    }
}