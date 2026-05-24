using BuildingBlocks.Infrastructure.Persistence.Models;
using BuildingBlocks.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Infrastructure.Persistence.Mappings;

public abstract class EntityDataTypeConfiguration<T> : IEntityTypeConfiguration<T>
    where T : EntityData
{
    /// <summary>
    /// Configura o mapeamento do tipo de entidade fornecido para o modelo de banco de dados.
    /// Este método define as propriedades, chaves e filtros padrão para a entidade base,
    /// permitindo extensões adicionais em classes derivadas.
    /// </summary>
    /// <param name="builder">
    /// O construtor de configuração do tipo de entidade, usado para definir o mapeamento do modelo.
    /// </param>
    public void Configure(EntityTypeBuilder<T> builder)
    {
        builder.ToTable(GetTableName());

        builder.HasKey(x => x.Id)
            .HasName($"pk_{GetTableName()}_id");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamptz")
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("timestamptz")
            .HasColumnName("modified_at");

        ConfigureEntity(builder);
    }

    /// <summary>
    /// Obtém o nome da tabela no banco de dados para o tipo mapeado.
    /// </summary>
    /// <returns>
    /// O nome da tabela correspondente.
    /// </returns>
    protected abstract string GetTableName();

    /// <summary>
    /// Configura propriedades adicionais ou relações específicas para a entidade do tipo fornecido.
    /// Este método deve ser implementado em classes derivadas para personalizar o mapeamento.
    /// </summary>
    /// <param name="builder">
    /// O construtor de configuração do tipo de entidade.
    /// </param>
    protected abstract void ConfigureEntity(EntityTypeBuilder<T> builder);

    /// <summary>
    /// Aplica um filtro global de consulta do Entity Framework para excluir automaticamente
    /// entidades que foram marcadas como excluídas logicamente (soft delete).
    /// </summary>
    /// <remarks>
    /// Garante que, por padrão, as consultas não retornem registros em que
    /// <see cref="ISoftDeletable.DeletedAt"/> possua valor.
    /// O filtro pode ser sobrescrito ou complementado nas configurações específicas da entidade, se necessário.
    /// </remarks>
    /// <typeparam name="TSoftDeletable">
    /// O tipo da entidade que implementa <see cref="ISoftDeletable"/> e suporta exclusão lógica.
    /// </typeparam>
    /// <param name="builder">
    /// O construtor de configuração do Entity Framework para o tipo da entidade.
    /// </param>
    protected static void ApplySoftDeleteQueryFilter<TSoftDeletable>(EntityTypeBuilder<TSoftDeletable> builder)
        where TSoftDeletable : EntityData, ISoftDeletable
        => builder.HasQueryFilter(x => !x.DeletedAt.HasValue);
}