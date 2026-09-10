namespace CuentasAPI.Messaging.Contracts;

public static class ClienteEventTypes
{
    public const string Creado = "ClienteCreado";
    public const string Actualizado = "ClienteActualizado";
    public const string Eliminado = "ClienteEliminado";
}

public static class ClienteRoutingKeys
{
    public const string Creado = "cliente.creado";
    public const string Actualizado = "cliente.actualizado";
    public const string Eliminado = "cliente.eliminado";
}

public sealed record ClienteIntegrationEvent(
    Guid EventId,
    string EventType,
    int Version,
    DateTime OccurredAt,
    ClienteEventData Data);

public sealed record ClienteEventData(
    int ClienteId,
    string Nombre,
    string Identificacion,
    bool Estado);