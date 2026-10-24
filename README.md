# Dental Care – API

Trabajo Integrador Final – Tecnicatura Universitaria en Programación (UTN FRRo).
Integrantes: Amaya Lara, Burgos Benjamín, Mendez Matías, Wilson Sebastián.

## Arquitectura (Clean Architecture)

| Proyecto | Responsabilidad |
|---|---|
| `DentalCare.Domain` | Entidades, enums e interfaces de repositorios. No depende de nadie. |
| `DentalCare.Application` | Interfaces y servicios de negocio. Depende de Domain. |
| `DentalCare.Infrastructure` | `DentalCareDbContext` (EF Core + MySQL/Pomelo), repositorios (genérico + específicos). |
| `DentalCare.Web` | Controladores, `Program.cs`, configuración JWT. |

Entidades (según el diagrama de clases corregido): `Paciente`, `Odontologo`, `Consultorio`, `Turno` y `Tratamiento`.
La relación Turno–Tratamiento es N:N; EF Core crea la tabla intermedia `turno_tratamiento` sin entidad propia.

## Requisitos

- .NET 8 SDK
- MySQL 8
- Herramienta de EF: `dotnet tool install --global dotnet-ef --version 8.0.11`

## Primera puesta en marcha

1. Ajustar la cadena de conexión en `src/DentalCare.Web/appsettings.json`
   (o con la variable de ambiente `ConnectionStrings__DefaultConnection`).
2. Generar la migration inicial (crea la carpeta `Migrations` con el snapshot):

   ```bash
   dotnet ef migrations add InitialCreate \
     --project src/DentalCare.Infrastructure \
     --startup-project src/DentalCare.Web \
     --output-dir Migrations
   ```

3. Aplicar la migration a la base:

   ```bash
   dotnet ef database update \
     --project src/DentalCare.Infrastructure \
     --startup-project src/DentalCare.Web
   ```

4. Ejecutar la API:

   ```bash
   dotnet run --project src/DentalCare.Web
   ```

## Secretos

- `Jwt:Key` **no** debe commitearse. En desarrollo se usa el valor de `appsettings.Development.json`
  (solo local). En Azure va en la variable de ambiente `Jwt__Key` (idealmente desde Key Vault).
- Cambiar `Password=CAMBIAR` de la cadena de conexión por la real, sin commitearla.
