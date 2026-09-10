# F7 - Ejecución con Docker Compose

## Requisitos

- Docker Desktop ejecutándose con contenedores Linux.
- Al menos 4 GB de memoria disponible para Docker.

## Ejecución en Windows PowerShell

Desde la carpeta raíz de `banking-project`:

```powershell
docker stop rabbitmq #si tiene un contenedor corriendo
Copy-Item .env.example .env
docker compose up --build -d
docker compose ps
```

El primer comando detiene y elimina el RabbitMQ creado con `--rm`. Compose
iniciará su propio RabbitMQ dentro de la misma red que las APIs.

## Direcciones

- ClientesAPI: http://localhost:8081
- CuentasAPI: http://localhost:8082
- OpenAPI Clientes: http://localhost:8081/openapi/v1.json
- OpenAPI Cuentas: http://localhost:8082/openapi/v1.json
- RabbitMQ Management: http://localhost:15672
- SQL Server: localhost,1433

Las credenciales de desarrollo se encuentran en `.env`. Ese archivo está
ignorado por Git y no debe subirse.

## Comprobación

```powershell
docker compose logs -f clientesapi cuentasapi
```

Crear primero un cliente en `POST http://localhost:8081/clientes`. Luego
esperar unos segundos y crear su cuenta en `POST http://localhost:8082/cuentas`.
Esto comprueba que el evento atravesó RabbitMQ y que CuentasAPI creó su
proyección local del cliente.

## Detener

```powershell
docker compose down
```

Para eliminar también los datos persistidos de SQL Server y RabbitMQ:

```powershell
docker compose down -v
```
