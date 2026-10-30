# CI/CD y despliegue a Azure

El pipeline está en [`.github/workflows/ci-cd.yml`](../.github/workflows/ci-cd.yml) y separa la
integración continua del despliegue:

- **CI (`build-test`)**: corre en **cada push y pull request a `main`**. Hace `Restore → Build (Release)
  → Tests → Publish` y sube el publicado como artifact. Fija .NET 8 con `global.json`.
- **Deploy (`deploy`)**: **no es automático**. Corre **solo con `workflow_dispatch`** (ejecución manual),
  porque todavía no tenemos cuenta de Azure. Aplica las migraciones con EF Core y publica la API con un
  *publish profile*.

> La infraestructura se crea **a mano** una sola vez (ya no usamos Bicep). Los pasos de abajo son esa
> preparación inicial. Para costos y tiers disponibles, consultar siempre la
> [documentación oficial de Azure](https://learn.microsoft.com/azure/) — este documento no afirma que
> ningún recurso sea gratuito ni cita precios.

## 1. Crear los recursos en Azure

Se puede hacer desde el portal o con Azure CLI. Los comandos de abajo son de referencia; revisá en la
documentación oficial los nombres de SKU y opciones vigentes.

### 1.1. Resource Group

Contenedor lógico de todos los recursos del proyecto.

```bash
az group create --name rg-dentalcare --location brazilsouth
```

### 1.2. App Service (plan F1)

Web App Linux con runtime .NET 8, sobre un App Service Plan en el SKU **F1**.

```bash
az appservice plan create --name dentalcare-plan --resource-group rg-dentalcare --is-linux --sku F1
az webapp create --name dentalcare-api --resource-group rg-dentalcare --plan dentalcare-plan --runtime "DOTNETCORE:8.0"
```

- **Habilitar SCM Basic Auth** en el Web App (Configuración → General settings, o "SCM Basic Auth
  Publishing"): es necesario para poder desplegar con el *publish profile* desde GitHub Actions.
- En F1 **no hay Always On**: el primer request después de un rato de inactividad puede tardar más.

### 1.3. Base de datos MySQL (Azure Database for MySQL – Flexible Server)

Servidor MySQL 8 administrado + la base `dentalcare`.

```bash
az mysql flexible-server create --name dentalcare-mysql --resource-group rg-dentalcare --version 8.0.21
az mysql flexible-server db create --server-name dentalcare-mysql --resource-group rg-dentalcare --database-name dentalcare
```

**Firewall / acceso:**

- Habilitar **"Permitir el acceso público desde servicios de Azure"** en el servidor MySQL. Esto deja
  que la Web App (un servicio de Azure) se conecte a la base.
- El job de despliegue aplica las migraciones **desde el runner de GitHub**, que es externo a Azure y
  tiene **IP dinámica**. Para que ese paso pueda conectarse hay que agregar una regla de firewall que
  permita su IP (o abrir temporalmente el acceso durante el despliegue y cerrarlo después). Como
  alternativa, se pueden aplicar las migraciones desde una máquina ya autorizada en vez del runner.

**SSL:** Azure Database for MySQL exige conexión cifrada. La cadena de conexión debe incluir
`SslMode=Required` (revisá este punto si el despliegue falla por SSL).

## 2. Configurar la Web App (application settings)

En el Web App → Configuración → *Application settings*, cargar las variables que la API lee de su
configuración (en .NET, `__` equivale a `:`, por ejemplo `Jwt__Key` → `Jwt:Key`). **No se escriben
valores reales en el repositorio**: se cargan solo acá.

| App setting | Valor |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `Jwt__Key` | Secret del JWT (32+ caracteres aleatorios) |
| `ConnectionStrings__DefaultConnection` | Cadena a la base de Azure, con `SslMode=Required` |
| `Seed__Odontologo__Password` | Contraseña del odontólogo inicial `admin@dentalcare.com` |
| `Swagger__Enabled` | `true` (para poder probar la API en la defensa) |

La API aplica las migraciones pendientes al arrancar (`Database:MigrateOnStartup`) y crea el odontólogo
inicial a partir de `Seed:Odontologo`.

## 3. Secretos en GitHub

En GitHub → Settings → Secrets and variables → Actions:

| Secret | Valor |
|---|---|
| `AZURE_WEBAPP_PUBLISH_PROFILE` | Contenido del *publish profile* descargado del Web App (Overview → "Get publish profile") |
| `DB_CONNECTION_STRING` | Cadena de conexión a la base de Azure (con `SslMode=Required`), usada por el paso de migraciones |

Opcional, como *variable* (no secret): `AZURE_WEBAPP_NAME` con el nombre del Web App (por defecto el
workflow usa `dentalcare-api`).

> `DB_CONNECTION_STRING` se inyecta en el paso de migraciones como la variable
> `ConnectionStrings__DefaultConnection`, que es la que lee `DentalCareDbContextFactory`.

## 4. Ejecutar el despliegue (manual)

1. En GitHub, ir a la pestaña **Actions** → workflow **CI/CD**.
2. Botón **Run workflow**, elegir la rama `main` y confirmar.
3. Corre primero `build-test` y después `deploy`, que:
   - aplica las migraciones con `dotnet ef database update` sobre la base de Azure, y
   - publica el artifact en el App Service con el *publish profile*.
4. Una vez desplegada, la API queda en `https://<nombre-webapp>.azurewebsites.net` (Swagger en `/swagger`,
   health check en `/health`).

## 5. Limpieza

Borrar el Resource Group elimina de una sola vez todos los recursos creados:

```bash
az group delete --name rg-dentalcare
```
