# Osigu Medical Orders — Prueba Técnica Senior .NET

Solución de referencia para la prueba técnica de procesamiento de órdenes médicas.

## 1. Objetivo

La solución implementa una API REST en .NET 8 para registrar y consultar órdenes médicas y un Worker Service independiente que procesa las órdenes de forma asíncrona.

Flujo principal:

```text
POST /api/orders
       |
       v
   Pending
       |
       | Worker Service
       v
   Processed
```

Una orden **nunca se crea directamente como procesada**. El estado inicial siempre es `Pendiente`.

---

## 2. Stack tecnológico

- .NET 8
- ASP.NET Core Web API
- Clean Architecture
- CQRS ligero con Commands y Queries
- Entity Framework Core 8
- SQLite
- Worker Service / `BackgroundService`
- Swagger / OpenAPI
- Serilog
- xUnit
- Git

No se utiliza MediatR: el CQRS está implementado con handlers explícitos para mantener la solución pequeña, transparente y con pocas dependencias.

---

## 3. Estructura

```text
Osigu.MedicalOrders
│
├── src
│   ├── Osigu.MedicalOrders.Api
│   ├── Osigu.MedicalOrders.Application
│   ├── Osigu.MedicalOrders.Domain
│   ├── Osigu.MedicalOrders.Infrastructure
│   └── Osigu.MedicalOrders.Worker
│
├── tests
│   └── Osigu.MedicalOrders.UnitTests
│
├── database
│   └── schema.sql
│
├── docs
│   ├── architecture.md
│   ├── openapi.yaml
│   └── adr
│       └── 0001-sqlite-and-ensurecreated.md
│
├── logs
│   └── evidence.log
│
├── Directory.Build.props
├── .editorconfig
├── .gitignore
├── global.json
└── Osigu.MedicalOrders.sln
```

### Capas

**Domain**

Contiene entidades, enumeraciones y reglas de negocio. No depende de infraestructura.

**Application**

Contiene los casos de uso, CQRS, contratos de repositorio, Unit of Work, DTOs y procesamiento de órdenes.

**Infrastructure**

Contiene EF Core, SQLite y las implementaciones concretas de Repository y Unit of Work.

**Api**

Contiene HTTP, Controllers, middleware global de excepciones, Swagger y configuración de logging.

**Worker**

Es un proceso independiente que consulta órdenes pendientes y ejecuta el caso de uso de procesamiento.

---

## 4. Requisitos

Instalar:

- .NET 8 SDK

No es necesario instalar:

- SQL Server
- SQLite Server
- RabbitMQ
- Redis
- Docker

SQLite es una base de datos embebida y el archivo se crea automáticamente.

---

## 5. Ejecución

Los siguientes comandos deben ejecutarse desde la raíz del repositorio.

### Restaurar dependencias

```powershell
dotnet restore
```

### Compilar

```powershell
dotnet build --configuration Release
```

### Ejecutar pruebas

```powershell
dotnet test --configuration Release
```

### Iniciar la API

```powershell
dotnet run --project src/Osigu.MedicalOrders.Api
```

La API queda disponible en:

```text
http://localhost:5080
```

Swagger:

```text
http://localhost:5080/swagger
```

Health check:

```text
http://localhost:5080/health
```

### Iniciar el Worker

En una segunda terminal, desde la raíz:

```powershell
dotnet run --project src/Osigu.MedicalOrders.Worker
```

El Worker consulta órdenes `Pendiente` cada 5 segundos por defecto.

---

## 6. Base de datos

La aplicación utiliza:

```text
SQLite
```

Archivo por defecto:

```text
data/medical-orders.db
```

El directorio `data` se crea automáticamente.

La cadena de conexión por defecto es:

```text
Data Source=data/medical-orders.db
```

También puede sobrescribirse mediante una variable de entorno.

### PowerShell

```powershell
$env:ConnectionStrings__DefaultConnection="Data Source=data/medical-orders.db"
```

### CMD

```cmd
set ConnectionStrings__DefaultConnection=Data Source=data/medical-orders.db
```

### DDL

El script completo está en:

```text
database/schema.sql
```

Puede ejecutarse directamente con cualquier cliente SQLite.

### Migraciones

La solución no utiliza EF Core migrations deliberadamente.

Para esta prueba se utiliza `EnsureCreated()` al iniciar la API y el Worker. Esto permite levantar la solución sin pasos adicionales de infraestructura y mantiene el DDL explícito como entregable.

Para un entorno productivo se recomienda reemplazar `EnsureCreated()` por EF Core migrations versionadas y un proceso controlado de despliegue.

---

## 7. API REST

La especificación original define los endpoints bajo `/api/orders`.

### Crear orden

```http
POST /api/orders
Content-Type: application/json
```

Request:

```json
{
  "patientId": "12345",
  "patientName": "Juan Perez",
  "serviceCode": "LAB001",
  "serviceDescription": "Hemograma",
  "priority": "Normal"
}
```

Prioridades permitidas:

```text
Normal
Urgente
```

`patientId` y `serviceCode` son obligatorios.

Respuesta:

```http
201 Created
```

Ejemplo:

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "patientId": "12345",
  "patientName": "Juan Perez",
  "serviceCode": "LAB001",
  "serviceDescription": "Hemograma",
  "priority": "Normal",
  "status": "Pendiente",
  "createdAt": "2026-10-07T15:00:00Z"
}
```

La orden se almacena con estado `Pendiente`.

---

## 8. Consultar órdenes

```http
GET /api/orders
```

Filtros:

```http
GET /api/orders?patientId=12345
GET /api/orders?status=Pendiente
GET /api/orders?status=Procesada
```

Paginación:

```http
GET /api/orders?pageNumber=1&pageSize=20
```

Combinando filtros:

```http
GET /api/orders?patientId=12345&status=Procesada&pageNumber=1&pageSize=20
```

`pageSize` se limita a un máximo de 100.

---

## 9. Consultar detalle

```http
GET /api/orders/{id}
```

Ejemplo:

```http
GET /api/orders/8d5f9a9e-6c4e-4b0e-a6d8-2e0c1e0b7c01
```

Si no existe:

```http
404 Not Found
```

---

## 10. Procesamiento asíncrono

El Worker Service es independiente de la API.

Proceso:

1. Consulta órdenes en estado `Pendiente`.
2. Toma las órdenes pendientes.
3. Simula procesamiento mediante una espera controlada.
4. Ejecuta `Order.MarkAsProcessed()`.
5. Cambia el estado a `Procesada`.
6. Registra `ProcessedAt`.
7. Persiste el cambio.
8. Escribe trazabilidad en los logs.

El intervalo del Worker se configura en:

```json
{
  "Worker": {
    "IntervalSeconds": 5
  }
}
```

---

## 11. Logging

Los logs se escriben en:

```text
logs/application-YYYYMMDD.log
```

Se registran:

- Creación de órdenes.
- Procesamiento.
- Cambio de estado.
- Errores.
- Excepciones no controladas.
- Resultado de cada ciclo del Worker cuando procesa órdenes.

El archivo:

```text
logs/evidence.log
```

contiene evidencia de referencia del formato de trazabilidad requerido para la entrega.

---

## 12. Manejo de errores

La API utiliza un middleware global:

```text
ExceptionHandlingMiddleware
```

Convierte excepciones de aplicación y dominio a `ProblemDetails`.

Ejemplo:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110",
  "title": "Validation error",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/orders",
  "errors": [
    "PatientId is required.",
    "ServiceCode is required."
  ]
}
```

No se exponen stack traces al consumidor de la API.

---

## 13. Swagger / OpenAPI

Swagger UI:

```text
http://localhost:5080/swagger
```

La especificación también está incluida físicamente en:

```text
docs/openapi.yaml
```

---

## 14. Pruebas

Ejecutar:

```powershell
dotnet test --configuration Release
```

La suite incluye pruebas para:

- Crear una orden válida.
- Crear una orden con estado inicial `Pendiente`.
- Rechazar `PatientId` vacío.
- Rechazar `ServiceCode` vacío.
- Marcar una orden como `Procesada`.

---

## 15. SOLID y Clean Code

La solución evita:

- Controllers con lógica de negocio.
- Acceso directo a EF Core desde API.
- Servicios monolíticos.
- Dependencias innecesarias.
- Duplicación de reglas.
- Exposición de setters públicos en la entidad.

La entidad `Order` controla sus propias transiciones de estado:

```csharp
order.MarkAsProcessed();
```

en lugar de exponer:

```csharp
order.Status = ...
```

La aplicación depende de abstracciones:

```text
IOrderRepository
IUnitOfWork
IOrderProcessingService
```

La infraestructura implementa dichas abstracciones.

---

## 16. Decisiones arquitectónicas

Las decisiones relevantes están documentadas en:

```text
docs/architecture.md
docs/adr/0001-sqlite-and-ensurecreated.md
```

La solución mantiene el alcance apropiado para una prueba técnica. No introduce microservicios, brokers o infraestructura adicional que no sea requerida por el problema.

---

## 17. Checklist de entrega

- [x] Código fuente completo.
- [x] Clean Architecture.
- [x] Domain.
- [x] Application.
- [x] Infrastructure.
- [x] API.
- [x] Worker Service independiente.
- [x] Repository.
- [x] Unit of Work.
- [x] CQRS.
- [x] CreateOrderCommand.
- [x] GetOrderQuery.
- [x] GetOrdersQuery.
- [x] SQLite.
- [x] Entity Framework Core.
- [x] Middleware global de excepciones.
- [x] Logging en archivo.
- [x] Unit Tests.
- [x] Swagger/OpenAPI.
- [x] DDL completo.
- [x] README.
- [x] Evidencia de logs.
- [x] Historial Git local con commits descriptivos.

---

## 18. Flujo de demostración recomendado

1. Levantar la API.
2. Abrir Swagger.
3. Ejecutar `POST /api/orders`.
4. Ejecutar `GET /api/orders`.
5. Verificar que la orden aparece como `Pendiente`.
6. Levantar el Worker.
7. Esperar el ciclo de procesamiento.
8. Ejecutar nuevamente `GET /api/orders`.
9. Verificar que la orden aparece como `Procesada`.
10. Ejecutar `GET /api/orders/{id}`.
11. Revisar `logs/application-YYYYMMDD.log`.

---

## 19. Nota sobre Git

La guía de entrega solicita un repositorio Git público con historial de commits descriptivos.

Este paquete incluye un repositorio Git local con commits progresivos. Para la entrega final:

```powershell
git remote add origin <URL_DEL_REPOSITORIO>
git branch -M main
git push -u origin main
```

El repositorio remoto debe configurarse como público según las instrucciones de entrega.
