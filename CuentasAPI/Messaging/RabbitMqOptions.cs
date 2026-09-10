namespace CuentasAPI.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";

    public string Exchange { get; set; } = "banking.events";
    public string Queue { get; set; } = "cuentas.clientes";

    public string DeadLetterExchange { get; set; }
        = "banking.events.dlx";

    public string DeadLetterQueue { get; set; }
        = "cuentas.clientes.dlq";

    public string DeadLetterRoutingKey { get; set; }
        = "cuentas.clientes.failed";

    public ushort PrefetchCount { get; set; } = 1;
    public int DeliveryLimit { get; set; } = 3;
}