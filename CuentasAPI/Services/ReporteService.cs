using CuentasAPI.DTOs.Reportes;
using CuentasAPI.Repositories.Interfaces;
using CuentasAPI.Services.Interfaces;

namespace CuentasAPI.Services;

public sealed class ReporteService : IReporteService
{
    private readonly IReporteRepository _repository;
    private readonly IClienteProyeccionRepository
        _clientesRepository;

    public ReporteService(
        IReporteRepository repository,
        IClienteProyeccionRepository clientesRepository)
    {
        _repository = repository;
        _clientesRepository = clientesRepository;
    }

    public async Task<IReadOnlyList<ReporteEstadoCuentaResponse>>
        ObtenerEstadoCuentaAsync(
            ReporteEstadoCuentaRequest request,
            CancellationToken cancellationToken = default)
    {
        if (!request.FechaInicio.HasValue ||
            !request.FechaFin.HasValue)
        {
            throw new ArgumentException(
                "Debe especificar fechaInicio y fechaFin.");
        }

        if (request.FechaInicio > request.FechaFin)
        {
            throw new ArgumentException(
                "La fecha inicial no puede ser mayor que la fecha final.");
        }

        var cliente =
            await _clientesRepository.ObtenerPorIdAsync(
                request.ClienteId,
                cancellationToken);

        if (cliente is null)
        {
            throw new KeyNotFoundException(
                $"No se encontró el cliente {request.ClienteId}.");
        }

        var fechaInicio = ConvertirAUtc(
            request.FechaInicio.Value);

        var fechaFinExclusiva = ConvertirAUtc(
            request.FechaFin.Value).AddDays(1);

        var registros =
            await _repository.ObtenerEstadoCuentaAsync(
                request.ClienteId,
                fechaInicio,
                fechaFinExclusiva,
                cancellationToken);

        return registros.Select(registro =>
            new ReporteEstadoCuentaResponse
            {
                Fecha = registro.Fecha,
                Cliente = cliente.Nombre,
                NumeroCuenta = registro.NumeroCuenta,
                Tipo = registro.Tipo,
                SaldoInicial = registro.SaldoInicial,
                Estado = registro.Estado,
                Movimiento = registro.Movimiento,
                SaldoDisponible = registro.SaldoDisponible
            })
            .ToList();
    }

    private static DateTime ConvertirAUtc(DateOnly fecha)
    {
        var fechaHora =
            fecha.ToDateTime(TimeOnly.MinValue);

        return DateTime.SpecifyKind(
            fechaHora,
            DateTimeKind.Utc);
    }
}