namespace BuildingBlocks.SharedKernel.Abstractions;

/// <summary>
/// Representa uma entidade base abstrata que fornece propriedades e funcionalidades comuns
/// para outros objetos dentro do sistema.
/// </summary>
/// <remarks>
/// Esta classe contém propriedades e métodos que gerenciam os atributos de identificação,
/// criação, atualização e exclusão de entidades no contexto da aplicação.
/// </remarks>
public abstract class Entity : IEquatable<Entity>
{
    private readonly List<IDomainEvent> _events = [];

    /// <summary>
    /// Obtém o identificador único da entidade.
    /// </summary>
    /// <remarks>
    /// O identificador é gerado de forma automática e imutável utilizando a versão 7 do UUID.
    /// Ele é utilizado para distinguir de forma única cada instância de entidade.
    /// </remarks>
    public Guid Id { get; protected init; } = Guid.CreateVersion7();

    /// <summary>
    /// Obtém a data e hora em que a entidade foi criada.
    /// </summary>
    /// <remarks>
    /// Esta propriedade é definida automaticamente no momento da criação da entidade,
    /// utilizando o horário UTC. Ela é imutável, servindo como um registro temporal
    /// consistente para fins de auditoria e rastreamento.
    /// </remarks>
    public DateTimeOffset CreatedAt { get; protected init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Obtém ou define a data e hora da última atualização da entidade.
    /// </summary>
    /// <remarks>
    /// Esta propriedade armazena o momento em que a entidade foi modificada pela última vez.
    /// Sua atualização ocorre automaticamente por meio do método <see cref="UpdateEntity"/>.
    /// Caso a entidade nunca tenha sido modificada, o valor será nulo.
    /// </remarks>
    public DateTimeOffset? UpdatedAt { get; protected set; }

    /// <summary>
    /// Atualiza a entidade, ajustando a propriedade que indica a data e hora da última modificação.
    /// </summary>
    /// <remarks>
    /// Este método é utilizado para registrar o momento exato em que a entidade foi alterada.
    /// Após a chamada deste método, a propriedade <c>UpdatedAt</c> será atualizada com o horário atual
    /// (em UTC) para refletir a última modificação.
    /// </remarks>
    public void UpdateEntity()
        => UpdatedAt = DateTimeOffset.UtcNow;

    /// <summary>
    /// Retorna a coleção de eventos de domínio pendentes gerados pela entidade.
    /// </summary>
    /// <returns>
    /// Uma coleção somente leitura de <see cref="IDomainEvent"/> representando os eventos ainda não despachados.
    /// </returns>
    public IReadOnlyCollection<IDomainEvent> Events()
        => _events.AsReadOnly();

    /// <summary>
    /// Remove todos os eventos de domínio pendentes da entidade após o despacho.
    /// </summary>
    public void ClearEvents()
        => _events.Clear();

    /// <summary>
    /// Adiciona um evento de domínio à lista de eventos pendentes da entidade.
    /// </summary>
    /// <param name="event">O evento de domínio a ser registrado.</param>
    public void RaiseEvent(IDomainEvent @event)
        => _events.Add(@event);

    /// <summary>
    /// Retorna um código de hash que representa a entidade com base em sua propriedade <c>Id</c>.
    /// </summary>
    /// <remarks>
    /// Este método é utilizado para obter um identificador único de hash para a entidade,
    /// fundamentado no valor do identificador único (<c>Id</c>). É particularmente útil
    /// para inserir ou localizar instâncias em coleções que utilizam hashing, como <c>Dictionary</c>
    /// ou <c>HashSet</c>.
    /// </remarks>
    /// <returns>
    /// Um número inteiro que representa o código de hash da entidade gerado a partir do identificador único.
    /// </returns>
    public override int GetHashCode()
        => Id.GetHashCode();

    /// <summary>
    /// Determina se o objeto especificado é igual à entidade atual com base no tipo e no identificador único.
    /// </summary>
    /// <param name="obj">O objeto a ser comparado com a entidade atual.</param>
    /// <returns><c>true</c> se o objeto for uma entidade do mesmo tipo e com o mesmo <c>Id</c>; caso contrário, <c>false</c>.</returns>
    public override bool Equals(object? obj)
        => obj is Entity other && Equals(other);

    /// <summary>
    /// Determina se a entidade especificada é igual à entidade atual com base no tipo e no identificador único.
    /// </summary>
    /// <param name="other">A entidade a ser comparada com a instância atual.</param>
    /// <returns><c>true</c> se ambas as entidades forem do mesmo tipo e possuírem o mesmo <c>Id</c>; caso contrário, <c>false</c>.</returns>
    public bool Equals(Entity? other)
        => other is not null && GetType() == other.GetType() && Id == other.Id;
}