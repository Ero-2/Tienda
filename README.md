# Tienda Departamental

App móvil e-commerce construida con **.NET MAUI** + arquitectura de **microservicios** en .NET 10.

---

## Stack

| Capa | Tecnología |
|---|---|
| App móvil | .NET MAUI (Android, iOS, Windows, macOS) |
| Patrón | MVVM con CommunityToolkit.Mvvm |
| Gateway | YARP (Yet Another Reverse Proxy) |
| Microservicios | ASP.NET Core Minimal APIs |
| BD relacional | SQL Server 2022 |
| BD promociones | PostgreSQL 16 |
| Mensajería | RabbitMQ |
| Pagos | OpenPay (sandbox) |

---

## Arquitectura

```
App MAUI
    │
    └──► API Gateway (:5000)   ← YARP + JWT + Rate Limiting + Cache + Audit
              │
    ┌─────────┼──────────────────────┐
    │         │                      │
ms-clientes  ms-productos       ms-ordenes
  (:5004)      (:5003)            (:5001)
    │                               │
    │                          ms-pagos (:5002)
    │                               │ RabbitMQ
    │                          ms-envios (:5005)
    │
ms-promociones (:5006)
```

---

## Microservicios

| Servicio | Puerto | BD | Descripción |
|---|---|---|---|
| `ms-clientes` | 5004 | SQL Server | Login, registro, direcciones |
| `ms-productos` | 5003 | SQL Server | Catálogo, categorías |
| `ms-ordenes` | 5001 | SQL Server + RabbitMQ | Crear y consultar órdenes |
| `ms-pagos` | 5002 | SQL Server + RabbitMQ | Procesamiento OpenPay |
| `ms-envios` | 5005 | SQL Server | Rastreo de envíos |
| `ms-promociones` | 5006 | PostgreSQL | Promociones y descuentos |
| `api-gateway` | 5000 | — | Proxy YARP, punto de entrada único |

---

## API Gateway — Funciones

| Función | Detalle |
|---|---|
| **Routing YARP** | Enruta `/api/*` al microservicio correcto |
| **Autenticación JWT** | Valida token en cada request protegido |
| **Rate Limiting** | Public: 30 req/min · Auth: 120 req/min · Strict: 10 req/min |
| **Response Cache** | GET `/api/productos` cacheado 30s en memoria (`X-Cache: HIT/MISS`) |
| **User Context** | Inyecta `X-User-Id` y `X-User-Email` en requests proxeados |
| **IP Blocklist** | Bloquea IPs en `Security:BlockedIps` del config |
| **Audit Logging** | Registra método, path, userId, IP, status y duración |
| **Correlation ID** | `X-Correlation-Id` propagado en toda la cadena |
| **Health Checks** | `GET /health` — estado de todos los microservicios |
| **Swagger UI** | `GET /swagger` — documentación del gateway |
| **Error Handling** | Respuestas JSON uniformes para 4xx/5xx |

### Rutas del gateway

```
GET  /api/productos/{**}          → ms-productos  (público)
POST /api/clientes/login          → ms-clientes   (strict rate limit)
POST /api/clientes/registro       → ms-clientes   (strict rate limit)
*    /api/clientes/{**}           → ms-clientes   (JWT requerido)
*    /api/ordenes/{**}            → ms-ordenes    (JWT requerido)
*    /api/pagos/{**}              → ms-pagos      (JWT requerido, strict)
*    /api/envios/{**}             → ms-envios     (JWT requerido)
GET  /api/promociones/{**}        → ms-promociones (público)
POST /api/promociones/calcular    → ms-promociones (JWT requerido)
POST /api/promociones/{**}        → ms-promociones (solo Admin)
```

---

## Levantar el proyecto

### Requisitos
- Docker Desktop
- .NET 10 SDK (para desarrollo de la app)

### Backend completo (1 comando)

```bash
cd <ruta-del-repo>
docker-compose up --build
```

Servicios disponibles tras levantar:

| URL | Descripción |
|---|---|
| `http://localhost:5000` | API Gateway |
| `http://localhost:5000/swagger` | Swagger UI del gateway |
| `http://localhost:5000/health` | Health check de todos los ms |
| `http://localhost:15672` | RabbitMQ Management (admin/Admin_12345) |

### App MAUI (desarrollo)

```bash
cd Tienda
dotnet build
# Android emulador
dotnet run -f net10.0-android
# Windows
dotnet run -f net10.0-windows10.0.19041.0
```

> **Nota:** En emulador Android, el gateway está en `http://10.0.2.2:5000`. En Windows/iOS usa `http://localhost:5000`. Esto se configura automáticamente en `MauiProgram.cs`.

---

## Vistas de la app

| Pantalla | Descripción |
|---|---|
| Login / Registro | Autenticación con ms-clientes |
| Productos | Catálogo con filtro por categoría y búsqueda |
| Detalle producto | Imagen, precio, stock, agregar al carrito |
| Carrito | Items, descuento en tiempo real vía ms-promociones |
| Promociones | Reglas de descuento + cálculo sobre el carrito actual |
| Pago | Tokenización de tarjeta con OpenPay |
| Mis órdenes | Historial de compras |
| Rastreo | Estado del envío en tiempo real vía ms-envios |
| Cuenta | Perfil, direcciones, notificaciones |

---

## Seguridad

- JWT firmado con HMAC-SHA256 (secret compartido entre gateway y ms-clientes)
- Rate limiting por IP/usuario para prevenir abuso
- IP Blocklist configurable en `api-gateway/appsettings.json`
- Credentials de BD y RabbitMQ solo en variables de entorno (docker-compose)

---

## Vistas implementadas (sesiones recientes)

| Feature | Descripción |
|---|---|
| **Crédito por cuenta** | Crédito aleatorio $200/$500/$1000 al registrar. Pago sin tarjeta. |
| **Meses sin interés (MSI)** | 3/6/9 MSI según monto. Cobra solo primera cuota, genera plan de pagos. |
| **Envío gratis ≥ $50** | ms-envios calcula costo; mensaje "faltan $X para envío gratis". |
| **Perfil dinámico** | Crédito + historial órdenes + planes MSI activos + direcciones. |
| **Detalle de orden** | Lista items, subtotal, descuento, total, modalidad, estado. Botón rastreo. |
| **Rastreo con NumRastreo real** | ms-envios consume `orden.creada` vía RabbitMQ → genera `TDA-{yyyyMM}-{xxxxxx}`. |
| **Descuentos** | Electrónicos 5% siempre. Otros 10% si subtotal ≥ $1,000. |
| **Healthcheck SQL Server** | TCP check en docker-compose; ms-* arrancan solo cuando SQL Server está listo. |

---

## Pendientes / Próximas implementaciones

| Feature | Detalle |
|---|---|
| **Historial de cuotas completadas** | Mostrar en perfil qué planes MSI ya terminó el usuario |
| **Recargar / aumentar crédito** | Formulario o lógica para subir el límite de crédito |
| **Notificaciones push de cuota** | Aviso cuando una cuota MSI está por vencer |
| **Formulario de dirección** | AccountPage tiene "Próximamente" — conectar con ms-clientes |
| **Carrier real en envíos** | NumRastreo actual es mock (TDA-xxxxxx). Integrar FedEx/DHL/UPS API |
| **Actualización de estado de envío** | El `PUT /api/envios/{id}/estado` existe pero nadie lo llama automáticamente |
| **Filtro de historial de órdenes** | Por fecha, estado, monto |
| **Detalle de orden desde notificaciones** | Deep link a OrderDetailPage |
| **Admin panel** | CRUD productos, gestión órdenes, actualizar estados de envío |
| **Refresh token** | JWT actual no tiene renovación automática |

---

## Credenciales por defecto (desarrollo)

| Servicio | Usuario | Contraseña |
|---|---|---|
| SQL Server | `sa` | `Admin_12345` |
| PostgreSQL | `admin` | `Admin_12345` |
| RabbitMQ | `admin` | `Admin_12345` |
