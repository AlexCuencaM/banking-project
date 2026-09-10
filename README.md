# Banking Project

Solución backend bancaria construida con microservicios en ASP.NET Core. Permite administrar clientes, cuentas y movimientos, consultar estados de cuenta y sincronizar clientes entre servicios mediante RabbitMQ.

## Funcionalidades

- CRUD de clientes con validaciones y almacenamiento seguro de contraseñas mediante hashing.
- Consulta, creación y actualización de cuentas asociadas a clientes activos.
- Tipos de cuenta `Ahorros` y `Corriente` representados mediante un enum.
- Registro de depósitos y retiros con actualización automática del saldo disponible.
- Actualización parcial de movimientos mediante `PATCH` y recálculo de saldos posteriores.
- Reporte de estado de cuenta por cliente y rango de fechas.
- Comunicación asíncrona entre microservicios mediante RabbitMQ.
- Patrones Outbox e Inbox para mejorar la entrega e idempotencia de eventos.
- Cola de mensajes fallidos o Dead Letter Queue (DLQ).
- Pruebas unitarias y de integración para `ClientesAPI`.
- Ejecución completa mediante Docker Compose.

## Arquitectura

| Componente | Responsabilidad | Persistencia |
| --- | --- | --- |
| `ClientesAPI` | Administración de clientes y publicación de eventos | `ClientesDb` |
| `CuentasAPI` | Cuentas, movimientos, reportes y proyección local de clientes | `CuentasDb` |
| RabbitMQ | Transporte de eventos de clientes entre los microservicios | Volumen Docker |
| SQL Server | Motor de las bases de datos de ambos servicios | Volumen Docker |

Cuando se crea, actualiza o elimina un cliente, `ClientesAPI` guarda el cambio y un mensaje Outbox. Un proceso en segundo plano publica el evento en RabbitMQ y `CuentasAPI` actualiza su proyección local del cliente. De esta manera, `CuentasAPI` no necesita consultar sincrónicamente a `ClientesAPI` para crear una cuenta.

## Tecnologías

- .NET 10 y ASP.NET Core Web API.
- Entity Framework Core 10.
- SQL Server 2022.
- RabbitMQ 4 con Management UI.
- Docker y Docker Compose.
- xUnit, Moq, EF Core InMemory y `WebApplicationFactory`.

## Estructura del proyecto

```text
banking-project/
├── ClientesAPI/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Messaging/
│   ├── Migrations/
│   ├── Repositories/
│   └── Services/
├── ClientesAPITester/
│   ├── Integration/
│   └── Services/
├── CuentasAPI/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Messaging/
│   ├── Migrations/
│   ├── Repositories/
│   └── Services/
├── docker-compose.yml
└── banking-project.slnx
```

## Requisitos

- Docker Desktop configurado para utilizar contenedores Linux.
- Al menos 4 GB de memoria disponible para Docker.
- Puertos `1433`, `5672`, `15672`, `8081` y `8082` disponibles.

## Ejecución con Docker Compose

Desde la carpeta raíz de `banking-project`, en Windows PowerShell:

```powershell
Copy-Item .env.example .env
docker compose up --build -d
docker compose ps
```

Si previamente se inició RabbitMQ manualmente con un contenedor llamado `rabbitmq`, hay que detenerlo antes para liberar los puertos:

```powershell
docker stop rabbitmq
```

Docker Compose inicia SQL Server, RabbitMQ y las dos APIs dentro de una misma red. En el ambiente `Docker`, las APIs aplican automáticamente sus migraciones y crean `ClientesDb` y `CuentasDb`.

### Servicios y puertos

| Servicio | Dirección |
| --- | --- |
| ClientesAPI | `http://localhost:8081` |
| CuentasAPI | `http://localhost:8082` |
| RabbitMQ Management | `http://localhost:15672` |
| RabbitMQ AMQP | `localhost:5672` |
| SQL Server | `localhost,1433` |

Las credenciales locales se toman de `.env`. Este archivo está ignorado por Git y no debe subirse al repositorio. Los valores de `.env.example` son únicamente para desarrollo local y deben cambiarse en cualquier ambiente compartido.

## Endpoints

### ClientesAPI

| Método | Ruta | Descripción |
| --- | --- | --- |
| `GET` | `/clientes` | Lista todos los clientes. |
| `GET` | `/clientes/{id}` | Obtiene un cliente por identificador. |
| `POST` | `/clientes` | Crea un cliente. |
| `PUT` | `/clientes/{id}` | Actualiza completamente un cliente. |
| `DELETE` | `/clientes/{id}` | Elimina un cliente. |

### CuentasAPI

| Método | Ruta | Descripción |
| --- | --- | --- |
| `GET` | `/cuentas` | Lista todas las cuentas. |
| `GET` | `/cuentas/{id}` | Obtiene una cuenta por identificador. |
| `POST` | `/cuentas` | Crea una cuenta para un cliente activo. |
| `PUT` | `/cuentas/{id}` | Actualiza número, tipo y estado de una cuenta. |
| `GET` | `/movimientos` | Lista movimientos; acepta el filtro opcional `cuentaId`. |
| `GET` | `/movimientos/{id}` | Obtiene un movimiento por identificador. |
| `POST` | `/movimientos` | Registra un depósito o retiro. |
| `PATCH` | `/movimientos/{id}` | Modifica el valor de un movimiento y recalcula los saldos. |
| `GET` | `/reportes` | Obtiene el estado de cuenta por cliente y rango de fechas. |

Los valores positivos de un movimiento representan depósitos y los negativos representan retiros. El valor cero no está permitido. Los tipos de cuenta aceptados son `Ahorros` y `Corriente`.

Ejemplo de reporte:

```http
GET http://localhost:8082/reportes?clienteId=1&fechaInicio=2026-09-01&fechaFin=2026-09-30
```

## Prueba funcional de extremo a extremo

Primero se crea un cliente en `ClientesAPI`:

```powershell
$clienteRequest = @{
    nombre = "Jose Lema"
    edad = 35
    identificacion = "0912345678"
    direccion = "Otavalo sn y principal"
    telefono = "098254785"
    contrasena = "1234"
    estado = $true
} | ConvertTo-Json

$cliente = Invoke-RestMethod `
    -Method Post `
    -Uri "http://localhost:8081/clientes" `
    -ContentType "application/json" `
    -Body $clienteRequest

$cliente
```

Después se espera la propagación del evento por RabbitMQ y se crea la cuenta con el `clienteId` retornado:

```powershell
Start-Sleep -Seconds 3

$cuentaRequest = @{
    clienteId = $cliente.clienteId
    numeroCuenta = "478758"
    tipoCuenta = "Ahorros"
    saldoInicial = 1000
    estado = $true
} | ConvertTo-Json

$cuenta = Invoke-RestMethod `
    -Method Post `
    -Uri "http://localhost:8082/cuentas" `
    -ContentType "application/json" `
    -Body $cuentaRequest

$cuenta
```

Finalmente se registra un movimiento:

```powershell
$movimientoRequest = @{
    cuentaId = $cuenta.cuentaId
    valor = -100
} | ConvertTo-Json

Invoke-RestMethod `
    -Method Post `
    -Uri "http://localhost:8082/movimientos" `
    -ContentType "application/json" `
    -Body $movimientoRequest
```

La creación exitosa de la cuenta confirma que el evento del cliente fue publicado por `ClientesAPI`, atravesó RabbitMQ y fue procesado por `CuentasAPI`.

## Mensajería RabbitMQ

- Exchange principal: `banking.events` de tipo `topic`.
- Cola consumidora: `cuentas.clientes`.
- Eventos: `ClienteCreado`, `ClienteActualizado` y `ClienteEliminado`.
- Routing keys: `cliente.creado`, `cliente.actualizado` y `cliente.eliminado`.
- Dead Letter Exchange: `banking.events.dlx`.
- Dead Letter Queue: `cuentas.clientes.dlq`.
- Límite de entregas configurado: 3.

## Pruebas automatizadas

Desde la raíz del repositorio:

```powershell
dotnet test .\banking-project.slnx
```

El proyecto `ClientesAPITester` contiene:

- Pruebas unitarias de creación de clientes, identificación duplicada y generación del evento Outbox.
- Una prueba de integración de `POST /clientes` que verifica la respuesta HTTP, la persistencia del cliente, el hash de la contraseña y el mensaje Outbox.

## Comandos útiles

Consultar el estado de los contenedores:

```powershell
docker compose ps
```

Consultar los logs:

```powershell
docker compose logs --tail=100 clientesapi cuentasapi
docker compose logs --tail=100 rabbitmq sqlserver
```

Reconstruir únicamente una API después de modificar su código:

```powershell
docker compose build --no-cache clientesapi
docker compose up -d --force-recreate clientesapi
```

Detener la solución conservando los datos:

```powershell
docker compose down
```

Eliminar contenedores y volúmenes persistentes:

```powershell
docker compose down -v
```

> `docker compose down -v` elimina las bases de datos y los mensajes persistidos. Debe utilizarse únicamente cuando se quiera reiniciar completamente el ambiente.

## OpenAPI

Los documentos OpenAPI se habilitan cuando cada API se ejecuta con el ambiente `Development`. Actualmente no están publicados por los contenedores, que utilizan el ambiente `Docker`.
