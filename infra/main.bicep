// Infraestructura de DentalCare en Azure:
//   - Azure Database for MySQL (Flexible Server) + base "dentalcare"
//   - Key Vault con los secretos (JWT, cadena de conexión, contraseña del odontólogo inicial)
//   - App Service Plan (Linux) + Web App .NET 8 con identidad administrada
// Las variables de ambiente de la Web App son referencias a Key Vault: el secret del JWT
// nunca queda en el código ni en la configuración en texto plano.

@description('Prefijo de los recursos (minúsculas, sin espacios).')
@minLength(3)
@maxLength(12)
param appName string = 'dentalcare'

param location string = resourceGroup().location

@description('SKU del App Service Plan. B1 es el mínimo con "Always On".')
param appServiceSku string = 'B1'

param mysqlAdminLogin string = 'dentaladmin'

@secure()
param mysqlAdminPassword string

@secure()
@minLength(32)
param jwtKey string

@secure()
@description('Contraseña del odontólogo inicial (admin) que se crea al primer arranque.')
param seedOdontologoPassword string

param seedOdontologoEmail string = 'admin@dentalcare.com'

var sufijo = uniqueString(resourceGroup().id)
var webAppName = '${appName}-api-${sufijo}'
var mysqlServerName = '${appName}-mysql-${sufijo}'
var keyVaultName = take('${appName}kv${sufijo}', 24)
var databaseName = 'dentalcare'

// ---------- MySQL ----------
resource mysql 'Microsoft.DBforMySQL/flexibleServers@2023-12-30' = {
  name: mysqlServerName
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '8.0.21'
    administratorLogin: mysqlAdminLogin
    administratorLoginPassword: mysqlAdminPassword
    storage: {
      storageSizeGB: 20
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
  }
}

resource mysqlDatabase 'Microsoft.DBforMySQL/flexibleServers/databases@2023-12-30' = {
  parent: mysql
  name: databaseName
  properties: {
    charset: 'utf8mb4'
    collation: 'utf8mb4_unicode_ci'
  }
}

// Permite conexiones desde servicios de Azure (la Web App).
resource mysqlFirewallAzure 'Microsoft.DBforMySQL/flexibleServers/firewallRules@2023-12-30' = {
  parent: mysql
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// ---------- App Service ----------
resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: '${appName}-plan-${sufijo}'
  location: location
  kind: 'linux'
  sku: {
    name: appServiceSku
  }
  properties: {
    reserved: true
  }
}

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: webAppName
  location: location
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      alwaysOn: appServiceSku != 'F1'
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      healthCheckPath: '/health'
    }
  }
}

// ---------- Key Vault ----------
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    enableRbacAuthorization: false
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    accessPolicies: [
      {
        // La Web App solo puede leer secretos.
        tenantId: subscription().tenantId
        objectId: webApp.identity.principalId
        permissions: {
          secrets: [ 'get', 'list' ]
        }
      }
    ]
  }
}

resource secretJwt 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'jwt-key'
  properties: {
    value: jwtKey
  }
}

resource secretDb 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'db-connection'
  properties: {
    value: 'Server=${mysql.properties.fullyQualifiedDomainName};Port=3306;Database=${databaseName};User=${mysqlAdminLogin};Password=${mysqlAdminPassword};SslMode=Required;'
  }
}

resource secretSeed 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'seed-odontologo-password'
  properties: {
    value: seedOdontologoPassword
  }
}

// Variables de ambiente de la API. "__" equivale a ":" en la configuración de .NET (Jwt__Key -> Jwt:Key).
resource appSettings 'Microsoft.Web/sites/config@2023-12-01' = {
  parent: webApp
  name: 'appsettings'
  properties: {
    ASPNETCORE_ENVIRONMENT: 'Production'
    Jwt__Key: '@Microsoft.KeyVault(SecretUri=${secretJwt.properties.secretUri})'
    ConnectionStrings__DefaultConnection: '@Microsoft.KeyVault(SecretUri=${secretDb.properties.secretUri})'
    Seed__Odontologo__Email: seedOdontologoEmail
    Seed__Odontologo__Password: '@Microsoft.KeyVault(SecretUri=${secretSeed.properties.secretUri})'
    Database__MigrateOnStartup: 'true'
    Swagger__Enabled: 'true'
  }
}

output webAppName string = webApp.name
output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
output keyVaultName string = keyVault.name
output mysqlServer string = mysql.properties.fullyQualifiedDomainName
