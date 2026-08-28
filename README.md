# 🏦 Artemis Banking Pro (ABP)

Artemis Banking Pro (ABP) es una plataforma de **banca digital** desarrollada en **.NET 9**, diseñada para gestionar de forma integral las principales operaciones de una entidad financiera.

El sistema cuenta con una **aplicación web MVC** para los usuarios del sistema y una **Web API RESTful versionada** para permitir el consumo seguro de funcionalidades bancarias por aplicaciones externas.

---

## 📸 Capturas de Pantalla

### 🔐 Inicio de Sesión

<img width="1920" height="1080" alt="Captura de pantalla (493)" src="https://github.com/user-attachments/assets/c027d69f-3df5-42c8-bdae-995faad3220b" />

### 📊 Dashboard Administrativo

<img width="1920" height="1080" alt="Captura de pantalla (494)" src="https://github.com/user-attachments/assets/f1afc312-3763-40a6-8ced-dd0e5271e81d" />

### 👥 Gestión de Usuarios

<img width="1920" height="1080" alt="Captura de pantalla (495)" src="https://github.com/user-attachments/assets/af529838-f81d-4f8f-8929-90267c6fa3a4" />


### 💸 Monitor de Transacciones

<img width="1920" height="1080" alt="Captura de pantalla (496)" src="https://github.com/user-attachments/assets/abfb5c0c-3cf6-4c19-8d37-e1940356957b" />


### 💳 Gestión de Tarjetas de Crédito

<img width="1920" height="1080" alt="Captura de pantalla (497)" src="https://github.com/user-attachments/assets/5a7400fa-4761-4d52-b26d-4c5e3f49d9de" />

### 📅 Plan de Pagos de Préstamos

<img width="1920" height="1080" alt="Captura de pantalla (498)" src="https://github.com/user-attachments/assets/91f46636-00cd-4cf5-a9c3-fde9137e8b3e" />

### 🧾 Panel del Cajero

<img width="1920" height="1080" alt="Captura de pantalla (499)" src="https://github.com/user-attachments/assets/a7689389-398d-4bd2-893e-27989f54fb40" />


### 👤 Panel del Cliente

<img width="1920" height="1080" alt="Captura de pantalla (501)" src="https://github.com/user-attachments/assets/6dbd6ad9-29f2-423d-bb53-c90b90c60656" />

### 👥 Gestión de Beneficiarios
<img width="1920" height="1080" alt="Captura de pantalla (502)" src="https://github.com/user-attachments/assets/2f3fe426-a618-4312-bc23-ce390ce9c80d" />

---

# 🛠️ Tecnologías y Herramientas

- **Lenguaje:** C#
- **Framework:** .NET 9
- **Web:** ASP.NET Core MVC
- **API:** ASP.NET Core Web API
- **Arquitectura:** Onion Architecture
- **Patrón:** CQRS
- **Mediador:** MediatR
- **ORM:** Entity Framework Core
- **Base de Datos:** SQL Server
- **Autenticación:** ASP.NET Identity
- **Seguridad API:** JWT Bearer Authentication
- **Mapeo:** AutoMapper
- **Documentación:** Swagger / OpenAPI
- **Logging:** Serilog
- **Serverless:** Azure Functions
- **Pruebas:** Unit Tests e Integration Tests

---

# 🔐 Seguridad y Roles

## 👨‍💼 Administrador

- Gestión de usuarios.
- Gestión de cuentas.
- Gestión de préstamos.
- Gestión de tarjetas de crédito.
- Consulta de transacciones.
- Administración de productos financieros.
- Supervisión de operaciones.
- Acceso a recursos administrativos de la API.

## 👤 Cliente

- Consulta de cuentas de ahorro.
- Consulta de balances.
- Consulta de movimientos.
- Transferencias.
- Gestión de beneficiarios.
- Solicitud y consulta de préstamos.
- Consulta del plan de pagos.
- Pago de préstamos.
- Consulta de tarjetas de crédito.
- Pago de tarjetas.
- Avances de efectivo.
- Consulta del historial de operaciones.

## 🏪 Comercio

- Operaciones relacionadas con pagos.
- Integración con servicios de pago.
- Acceso a los recursos autorizados para comercios.

## 💵 Cajero

- Depósitos.
- Retiros.
- Pagos de tarjetas de crédito.
- Pagos de préstamos.
- Transferencias.
- Consulta de historial.
- Consulta de operaciones del día.
- Visualización del monto total operado.

---

# ⏰ Control Automático de Cuotas Vencidas

El proyecto incorpora una **Azure Function** encargada de realizar el control automático de las cuotas de los préstamos.

La función revisa periódicamente las cuotas pendientes y:

- Identifica cuotas cuya fecha de vencimiento ya pasó.
- Verifica si la cuota ha sido pagada completamente.
- Marca como vencidas las cuotas correspondientes.
- Actualiza el estado de las cuotas cuando son pagadas.
---

# 💳 Hermes Pay

Artemis Banking Pro incorpora integración con **Hermes Pay** para el procesamiento de determinadas operaciones de pago. La integración permite conectar las operaciones de pago con los productos y transacciones financieras de la plataforma.

---
# 📚 Swagger / OpenAPI

La Web API incluye documentación mediante **Swagger / OpenAPI**.

Swagger permite:

- Visualizar los endpoints.
- Consultar parámetros.
- Consultar respuestas.
- Probar solicitudes.
- Autenticar mediante JWT.
- Ver las diferentes versiones de la API.
  
---

# 🧪 Pruebas

El proyecto incorpora **pruebas unitarias y pruebas de integración**.

## Unit Tests

Estas pruebas permiten validar componentes individuales y diferentes funcionalidades de la capa de aplicación.

## Integration Tests

Estas pruebas permiten validar la interacción entre diferentes componentes de la aplicación, incluyendo la capa de persistencia.

---

# 🧩 CQRS + MediatR

La aplicación utiliza **CQRS (Command Query Responsibility Segregation)** para separar las operaciones de lectura y escritura.

La comunicación entre los controladores y los casos de uso se realiza mediante **MediatR**.

Este enfoque permite mantener los controladores ligeros y organizar la lógica de negocio de forma independiente.

---

# ⚙️ Configuración del Proyecto

## Requisitos previos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/es-es/sql-server) (local o express)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (recomendado)
- Opcional: [Azure Functions Core Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local)

## 1. Clonar el repositorio

```bash
git clone https://github.com/MarielyRoa/Artemis-Banking-Pro-ABP-.git
cd Artemis-Banking-Pro-ABP-
```

## 2. Configurar Secretos (User Secrets)

> ⚠️ **IMPORTANTE:** Este proyecto utiliza **.NET User Secrets** para manejar credenciales de forma segura. **NUNCA** subas contraseñas, API keys o connection strings a GitHub.

Ejecuta estos comandos desde la carpeta del proyecto API:

```bash
cd "Artemis Banking Pro WebApi"

# Inicializar User Secrets
dotnet user-secrets init

# Configurar Connection Strings
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=TU_SERVIDOR;Database=ArtemisBankingAppDb;Trusted_Connection=True;TrustServerCertificate=true"
dotnet user-secrets set "ConnectionStrings:IdentityConnection" "Server=TU_SERVIDOR;Database=ArtemisBankingAppDb;Trusted_Connection=True;TrustServerCertificate=true"

# Configurar Email
dotnet user-secrets set "EmailSettings:EmailFrom" "tu-email@gmail.com"
dotnet user-secrets set "EmailSettings:SmtpUser" "tu-email@gmail.com"
dotnet user-secrets set "EmailSettings:SmtpPass" "tu-app-password-de-gmail"

# Configurar JWT
dotnet user-secrets set "JWTSettings:SecretKey" "tu-clave-secreta-minimo-32-caracteres-aqui"
dotnet user-secrets set "JWTSettings:Issuer" "ArtemisBankingIdentity"
dotnet user-secrets set "JWTSettings:Audience" "ArtemisBankingApi"
```

## 3. Configurar la Base de Datos

1. Crear la base de datos `ArtemisBankingAppDb` en SQL Server
2. Aplicar migraciones:

```bash
dotnet ef database update --project "ABP.Infrastructure.Persistence" --startup-project "Artemis Banking Pro WebApi"
```

## 4. Ejecutar la Aplicación Web

```bash
dotnet run --project "Artemis Banking Pro"
```

## 5. Ejecutar la Web API

```bash
dotnet run --project "Artemis Banking Pro WebApi"
```

La API estará disponible en: `https://localhost:5001/swagger`

## 6. Ejecutar las Pruebas

```bash
dotnet test
```

---

# 📂 Proyectos de la Solución

| Proyecto | Responsabilidad |
|---|---|
| `ABP.Core.Domain` | Entidades, interfaces y reglas del dominio |
| `ABP.Core.Application` | Casos de uso, CQRS, MediatR, DTOs y servicios |
| `ABP.Infrastructure.Persistence` | Persistencia y Entity Framework Core |
| `ABP.Infrastructure.Identity` | Autenticación y gestión de usuarios |
| `ABP.Infrastructure.Shared` | Servicios e infraestructura compartida |
| `Artemis Banking Pro` | Aplicación Web MVC |
| `Artemis Banking Pro WebApi` | API RESTful versionada |
| `AutomaticOverduePaymentControlFunction` | Control automático de cuotas vencidas |
| `ABP.Unit.Tests` | Pruebas unitarias |
| `ABP.Integration.Test` | Pruebas de integración |

---

# 👥 Equipo de Desarrollo

| Nombre | GitHub | Rol |
|--------|--------|-----|
| **Mariely Gerardine Roa Baez** | [@MarielyRoa](https://github.com/MarielyRoa) | Desarrolladora |
| **Daferlin Álvarez** | [@Josbel01](https://github.com/Josbel01) | Desarrollador |
| **Carlos Eliezer Font De Jesus** | — | Desarrollador |
| **Victor Enriquez Nunez Carvajal** | — | Desarrollador |

---

> 💡 **Nota:** Este proyecto fue desarrollado con fines académicos para el curso de Desarrollo de Software en el **ITLA** (Instituto Tecnológico de las Américas).
