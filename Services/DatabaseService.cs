using Npgsql;

namespace RenderDB.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DatabaseService> _logger;
        private string _connectionString = string.Empty;

        public DatabaseService(IConfiguration configuration, ILogger<DatabaseService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            InitializeConnectionString();
        }

        /// <summary>
        /// Inicializa la cadena de conexión desde variables de entorno o configuración
        /// </summary>
        private void InitializeConnectionString()
        {
            // Primero intenta desde variable de entorno DATABASE_URL (usado por Render)
            string? databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            
            if (!string.IsNullOrEmpty(databaseUrl))
            {
                // Render proporciona una URL de conexión en formato postgresql://
                // Convertir a formato que Npgsql entienda
                _connectionString = ConvertPostgresUriToNpgsqlConnectionString(databaseUrl);
                _logger.LogInformation("Usando DATABASE_URL de variable de entorno");
            }
            else
            {
                // Fallback a configuración local
                string? host = _configuration["DatabaseConnection:Host"] ?? 
                               Environment.GetEnvironmentVariable("DB_HOST") ?? 
                               "dpg-d8fvcce47okc73errllg-a.frankfurt-postgres.render.com";
                
                int port = int.Parse(_configuration["DatabaseConnection:Port"] ?? 
                                   Environment.GetEnvironmentVariable("DB_PORT") ?? "5432");
                
                string? database = _configuration["DatabaseConnection:Database"] ?? 
                                  Environment.GetEnvironmentVariable("DB_NAME") ?? 
                                  "database_liha";
                
                string? username = _configuration["DatabaseConnection:Username"] ?? 
                                  Environment.GetEnvironmentVariable("DB_USER") ?? 
                                  "admin";
                
                string? password = _configuration["DatabaseConnection:Password"] ?? 
                                  Environment.GetEnvironmentVariable("DB_PASSWORD") ?? 
                                  "YOUR_PASSWORD";

                _connectionString = $"Host={host};Port={port};Username={username};Password={password};" +
                                  $"Database={database};SSL Mode=Require;Trust Server Certificate=true;";

                _logger.LogInformation("Usando configuración local (appsettings.json o variables de entorno)");
            }
        }

        /// <summary>
        /// Convierte una URI de PostgreSQL (postgresql://user:pass@host:port/db) al formato de conexión de Npgsql
        /// </summary>
        private string ConvertPostgresUriToNpgsqlConnectionString(string postgresUri)
        {
            try
            {
                var uri = new Uri(postgresUri);
                var builder = new NpgsqlConnectionStringBuilder
                {
                    Host = uri.Host,
                    Port = uri.Port > 0 ? uri.Port : 5432,
                    Username = uri.UserInfo?.Split(':')[0] ?? "admin",
                    Password = uri.UserInfo?.Contains(':') == true ? uri.UserInfo.Split(':')[1] : string.Empty,
                    Database = uri.AbsolutePath.TrimStart('/'),
                    SslMode = SslMode.Require,
                    TrustServerCertificate = true
                };

                _logger.LogInformation("Convertida URI PostgreSQL a connection string de Npgsql");
                return builder.ConnectionString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al convertir URI de PostgreSQL");
                throw;
            }
        }

        /// <summary>
        /// Inicializa la base de datos (crea tabla si no existe)
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                await CreateTableIfNotExistsAsync();
                _logger.LogInformation("Base de datos inicializada correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al inicializar base de datos");
            }
        }

        /// <summary>
        /// Verifica la conexión a la base de datos
        /// </summary>
        public async Task<(bool success, string message, DateTime? serverTime)> VerifyConnectionAsync()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var cmd = new NpgsqlCommand("SELECT NOW();", connection))
                    {
                        var result = await cmd.ExecuteScalarAsync();
                        DateTime serverTime = (DateTime)result!;

                        _logger.LogInformation("Conexión verificada. Hora del servidor: {ServerTime}", serverTime);
                        return (true, "✅ Conexión a PostgreSQL verificada correctamente", serverTime);
                    }
                }
            }
            catch (NpgsqlException pgEx)
            {
                _logger.LogError(pgEx, "Error de conexión PostgreSQL");
                return (false, $"❌ Error de conexión PostgreSQL: {pgEx.Message}", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al verificar conexión");
                return (false, $"❌ Error inesperado: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Inserta un nuevo registro
        /// </summary>
        public async Task<(bool success, string message, int? recordId)> InsertRecordAsync(string nombre, string email)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return (false, "❌ El nombre no puede estar vacío", null);

            if (string.IsNullOrWhiteSpace(email))
                return (false, "❌ El email no puede estar vacío", null);

            if (!email.Contains("@"))
                return (false, "❌ Email inválido (debe contener @)", null);

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var insertSql = @"
                        INSERT INTO demo_records (nombre, email) 
                        VALUES (@nombre, @email)
                        RETURNING id;
                    ";

                    using (var cmd = new NpgsqlCommand(insertSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@nombre", nombre);
                        cmd.Parameters.AddWithValue("@email", email);

                        var result = await cmd.ExecuteScalarAsync();
                        int recordId = (int)result!;

                        _logger.LogInformation("Registro insertado. ID: {RecordId}, Nombre: {Nombre}, Email: {Email}", 
                                            recordId, nombre, email);
                        
                        return (true, $"✅ Registro insertado correctamente (ID: {recordId})", recordId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al insertar registro");
                return (false, $"❌ Error al insertar registro: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Obtiene todos los registros
        /// </summary>
        public async Task<(bool success, string message, List<DemoRecord>? records)> GetAllRecordsAsync()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var selectSql = @"
                        SELECT id, nombre, email, created_at 
                        FROM demo_records 
                        ORDER BY id DESC;
                    ";

                    var records = new List<DemoRecord>();

                    using (var cmd = new NpgsqlCommand(selectSql, connection))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                records.Add(new DemoRecord
                                {
                                    Id = reader.GetInt32(0),
                                    Nombre = reader.GetString(1),
                                    Email = reader.GetString(2),
                                    CreatedAt = reader.GetDateTime(3)
                                });
                            }
                        }
                    }

                    _logger.LogInformation("Se leyeron {RecordCount} registros", records.Count);
                    return (true, $"✅ Se encontraron {records.Count} registros", records);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer registros");
                return (false, $"❌ Error al leer registros: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Crea la tabla si no existe
        /// </summary>
        private async Task CreateTableIfNotExistsAsync()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var createTableSql = @"
                        CREATE TABLE IF NOT EXISTS demo_records (
                            id SERIAL PRIMARY KEY,
                            nombre VARCHAR(100),
                            email VARCHAR(100),
                            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                        );
                    ";

                    using (var cmd = new NpgsqlCommand(createTableSql, connection))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }

                    _logger.LogInformation("Tabla 'demo_records' verificada/creada");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear tabla");
                throw;
            }
        }
    }
}
