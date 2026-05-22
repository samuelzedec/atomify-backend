namespace BuildingBlocks.SharedKernel.Abstractions;

/// <summary>
/// Classe abstrata que serve como base para a implementação de objetos de valor.
/// Objetos de valor são caracterizados por sua identidade baseada em seus
/// atributos ou propriedades, sendo imutáveis e garantindo igualdade estrutural.
/// </summary>
public abstract record ValueObject;