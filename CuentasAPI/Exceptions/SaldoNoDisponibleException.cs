namespace CuentasAPI.Exceptions;

public sealed class SaldoNoDisponibleException : Exception
{
    public SaldoNoDisponibleException()
        : base("Saldo no disponible")
    {
    }
}