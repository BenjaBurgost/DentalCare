# Persistencia (EF Core + MySQL)

Resumen de cómo se guarda la información: qué hace cada capa, cómo se mapean las relaciones del diagrama
y qué comandos se usan para las migrations. La base es **MySQL 8** con el proveedor **Pomelo** y el
enfoque es **code first** (las tablas se generan a partir de las entidades).

## Qué hace cada capa

| Capa | Responsabilidad en la persistencia |
|---|---|
| `DentalCare.Domain` | Las **entidades** (`Paciente`, `Odontologo`, `Consultorio`, `Turno`, `Tratamiento`) y las interfaces de repositorio. No conoce EF Core. |
| `DentalCare.Application` | Servicios de negocio y DTOs. Trabaja contra las **interfaces** de repositorio, no contra EF. |
| `DentalCare.Infrastructure` | `DentalCareDbContext`, el repositorio genérico `Repository<T>` + los específicos, las **migrations** y la `DentalCareDbContextFactory` (para las herramientas de EF en tiempo de diseño). |
| `DentalCare.Web` | Arranca la app; en `Program.cs` aplica las migrations pendientes y siembra el odontólogo inicial. |

Que el dominio y la aplicación dependan solo de **interfaces** es lo que permite testear con SQLite en
memoria sin tocar MySQL.

## Cómo se mapean las relaciones

EF Core infiere el esquema a partir de las entidades y sus propiedades de navegación. En este proyecto
hay dos tipos de relación:

### 1:N (uno a muchos)

`Paciente`, `Odontologo` y `Consultorio` tienen cada uno **muchos** `Turno`. En cada caso:

- El lado "uno" expone una colección: `ICollection<Turno> Turnos`.
- El lado "muchos" (`Turno`) tiene la **clave foránea** y la navegación inversa, por ejemplo
  `int PacienteId` + `Paciente Paciente`. Como la FK es un `int` no anulable, la relación es **obligatoria**.

Así, un turno siempre pertenece a un paciente, un odontólogo y un consultorio.

### N:N (muchos a muchos) — `Turno` ↔ `Tratamiento`

Un turno incluye 1..* tratamientos y un tratamiento puede estar en muchos turnos. Cada entidad tiene una
colección de la otra (`Turno.Tratamientos` y `Tratamiento.Turnos`). EF Core crea **automáticamente** la
tabla intermedia `turno_tratamiento` (solo las dos FK), **sin necesidad de una entidad intermedia** en el
dominio. Es exactamente la N:N del diagrama de clases.

> Las tablas que genera la migration inicial son: `paciente`, `odontologo`, `consultorio`, `tratamiento`,
> `turno`, la intermedia `turno_tratamiento` y `__EFMigrationsHistory` (control interno de EF).

## Comandos de migrations (MySQL)

Se ejecutan con el proyecto de infraestructura como destino y el de Web como *startup*. Requieren el tool:
`dotnet tool install --global dotnet-ef --version 8.0.11`.

```bash
# Crear una nueva migration (code first)
dotnet ef migrations add NombreDescriptivo \
  --project src/DentalCare.Infrastructure \
  --startup-project src/DentalCare.Web

# Aplicar las migrations pendientes a la base
dotnet ef database update \
  --project src/DentalCare.Infrastructure \
  --startup-project src/DentalCare.Web
```

La cadena de conexión se toma de la variable de ambiente `ConnectionStrings__DefaultConnection` (la que
lee `DentalCareDbContextFactory`); si no está, usa el valor por defecto de desarrollo. En la práctica, la
app también aplica las migrations al arrancar (`Database:MigrateOnStartup`), así que `database update`
manual se usa sobre todo en el despliegue.

> ⚠️ **`dotnet ef database drop` BORRA LA BASE DE DATOS COMPLETA** (todas las tablas y sus datos). Es
> irreversible. Usalo solo contra una base descartable y confirmando **a qué base** apunta la cadena de
> conexión antes de ejecutarlo. Nunca lo corras contra una base con datos que quieras conservar.
