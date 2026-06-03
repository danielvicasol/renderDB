# RenderDB - PostgreSQL API REST

**Aplicación Web ASP.NET Core 6.0** que proporciona una **API REST** para probar conexión directa a PostgreSQL alojada en **Render**.

## 📋 Descripción

RenderDB es una API que permite:
- ✅ Verificar la conexión a PostgreSQL mediante endpoint HTTP
- ✅ Insertar registros mediante POST
- ✅ Listar registros mediante GET
- ✅ Validar datos automáticamente
- ✅ Swagger UI integrada para documentación interactiva
- ✅ Desplegable en Render con Docker
- ✅ Variables de entorno configurables
- ✅ Logging y auditoría completa

## 🔧 Requisitos

### Desarrollo Local
- **.NET 6.0** SDK o superior
- **Visual Studio 2022** (Recomendado) o Visual Studio Code
- **Credenciales de PostgreSQL en Render**

### Producción (Render)
- Cuenta en [Render](https://render.com)
- Repository en GitHub
- Base de datos PostgreSQL en Render

## 📦 Dependencias

- **Npgsql 7.0.0** - Driver para PostgreSQL
- **Swashbuckle 6.4.0** - Swagger/OpenAPI
- **Microsoft.Extensions.Configuration** - Configuración
- **ASP.NET Core 6.0** - Framework web

## 📡 Endpoints API

### 1. Verificar Conexión
```http
POST /api/records/verify
```
Verifica que la API puede conectarse a PostgreSQL.

### 2. Listar Registros
```http
GET /api/records
```
Obtiene todos los registros ordenados por ID descendente.

### 3. Insertar Registro
```http
POST /api/records
Content-Type: application/json

{
  "nombre": "Juan García",
  "email": "juan@example.com"
}
```

### 4. Insertar Demo
```http
POST /api/records/demo
```
Inserta automáticamente un registro de prueba.

### 5. Health Check
```http
GET /health
```

### 6. Información
```http
GET /
```

## ⚙️ Configuración Local

### 1. Editar appsettings.Development.json

Abre el archivo y reemplaza con tus credenciales de Render:

```json
{
  "DatabaseConnection": {
    "Host": "tu-host.render.com",
    "Port": 5432,
    "Database": "tu-database",
    "Username": "tu-usuario",
    "Password": "tu-password"
  }
}
```

## 🚀 Desarrollo Local

### Visual Studio 2022 (Recomendado)

1. Abre Visual Studio 2022
2. File → Open → Folder → Selecciona `RenderDB`
3. Visual Studio detectará automáticamente que es .NET
4. Presiona **F5** para ejecutar
5. La API se abrirá en `https://localhost:7001`
6. Accede a Swagger: `https://localhost:7001/swagger`

### Línea de Comandos

```bash
cd RenderDB
dotnet restore
dotnet run
```

La API estará disponible en `https://localhost:5001`

## 🐳 Desplegar en Render

### Paso 1: Subir a GitHub

```bash
cd RenderDB
git init
git add .
git commit -m "RenderDB ASP.NET Core API v2"
git remote add origin https://github.com/TU_USUARIO/RenderDB.git
git branch -M main
git push -u origin main
```

### Paso 2: Crear Web Service en Render

1. [Render Dashboard](https://dashboard.render.com)
2. **New** → **Web Service**
3. Conecta tu repositorio GitHub `RenderDB`
4. **Configuración:**
   - **Name:** `renderdb-api`
   - **Environment:** `Docker`
   - **Plan:** `Free` (o el que prefieras)
   - **Region:** `Frankfurt` (más cercano a Render DB)

### Paso 3: Configurar Variables de Entorno

En Render Dashboard del servicio, ve a **Environment**:

**Recomendado - Una sola variable:**
```
DATABASE_URL = postgres://admin:YOUR_PASSWORD@dpg-d8fvcce47okc73errllg-a.frankfurt-postgres.render.com:5432/database_liha?sslmode=require
```

**O variables individuales:**
```
DB_HOST = dpg-d8fvcce47okc73errllg-a.frankfurt-postgres.render.com
DB_PORT = 5432
DB_NAME = database_liha
DB_USER = admin
DB_PASSWORD = YOUR_PASSWORD
```

### Paso 4: Deploy

1. Render detectará el `Dockerfile` automáticamente
2. Haz clic en **Deploy**
3. Espera 3-5 minutos
4. Tu API estará en `https://renderdb-api.onrender.com`

## 🧪 Pruebas

## 🧪 Pruebas

### Con cURL

```bash
# Verificar conexión
curl -X POST https://renderdb-api.onrender.com/api/records/verify

# Listar registros
curl https://renderdb-api.onrender.com/api/records

# Insertar registro
curl -X POST https://renderdb-api.onrender.com/api/records \
  -H "Content-Type: application/json" \
  -d '{"nombre":"Test User","email":"test@example.com"}'

# Demo
curl -X POST https://renderdb-api.onrender.com/api/records/demo

# Health check
curl https://renderdb-api.onrender.com/health
```

### Con PowerShell

```powershell
# Verificar conexión
Invoke-WebRequest -Uri "https://renderdb-api.onrender.com/api/records/verify" -Method Post

# Listar registros
Invoke-WebRequest -Uri "https://renderdb-api.onrender.com/api/records"

# Insertar
$body = @{nombre="Juan García"; email="juan@example.com"} | ConvertTo-Json
Invoke-WebRequest -Uri "https://renderdb-api.onrender.com/api/records" `
  -Method Post `
  -ContentType "application/json" `
  -Body $body
```

## 📂 Estructura del Proyecto

```
RenderDB/
├── Controllers/
│   └── RecordsController.cs    # Endpoints API
├── Services/
│   ├── IDatabaseService.cs     # Interfaz
│   └── DatabaseService.cs      # Lógica de BD
├── Program.cs                  # Configuración ASP.NET Core
├── RenderDB.csproj             # Dependencias
├── Dockerfile                  # Para Render
├── .dockerignore
├── appsettings.json            # Producción
├── appsettings.Development.json # Desarrollo
├── .gitignore
└── README.md
```

## 🔐 Seguridad

- ✅ **SSL/TLS Obligatorio:** `SSL Mode=Require`
- ✅ **Consultas Parametrizadas:** Protección contra SQL injection
- ✅ **Variables de Entorno:** Credenciales no hardcodeadas
- ✅ **CORS:** Configurado para permitir múltiples orígenes
- ✅ **Logging:** Auditoría completa de operaciones
- ✅ **Manejo de errores:** Captura de excepciones específicas

## 📊 Estructura de Datos

```sql
CREATE TABLE demo_records (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(100),
    email VARCHAR(100),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

## 🐛 Troubleshooting

| Error | Solución |
|-------|----------|
| Connection Refused | Verifica credenciales y conectividad |
| 502 Bad Gateway (Render) | Revisa logs en Render Dashboard |
| Database Not Found | Verifica nombre exacto de la BD |
| Timeout | Espera 1-2 minutos en primer deploy |
| CORS Error | Ya está configurado, pero verifica origen |

## 📈 Monitoreo en Render

Para ver logs en producción:
1. Render Dashboard → Tu servicio `renderdb-api`
2. Pestaña **Logs**
3. Busca errores de conexión o aplicación

## 🔄 Deploy Continuo

Este proyecto está configurado para **auto-deploy desde GitHub**:
- Cada `git push` a `main` dispara un nuevo deploy
- Render compila la imagen Docker automáticamente
- Sin downtime entre actualizaciones

## 📚 Documentación Adicional

- [ASP.NET Core Docs](https://learn.microsoft.com/en-us/dotnet/core/)
- [Npgsql Documentation](https://www.npgsql.org/)
- [Render PostgreSQL](https://render.com/docs/postgresql)
- [PostgreSQL Official](https://www.postgresql.org/docs/)

## 💡 Notas de Desarrollo

- **Inyección de dependencias:** Configurada en Program.cs
- **Logging integrado:** Todos los eventos se registran
- **Async/Await:** Operaciones asincrónicas optimizadas
- **Swagger:** Documentación interactiva automática
- **Docker:** Multi-stage build para optimizar imagen

## 🛠️ Extensiones Posibles

- Agregar autenticación JWT
- Implementar CRUD completo (Update, Delete)
- Integrar Entity Framework Core
- Agregar validación más robusta
- Implementar caching
- Agregar más tablas y relaciones

---

**Versión:** 2.0 (Web API con Docker)  
**Estado:** ✅ Listo para producción en Render  
**Última actualización:** 2026-06-03  
**Autor:** Senior .NET Developer
