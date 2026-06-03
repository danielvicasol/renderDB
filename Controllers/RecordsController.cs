using Microsoft.AspNetCore.Mvc;
using RenderDB.Services;

namespace RenderDB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecordsController : ControllerBase
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<RecordsController> _logger;

        public RecordsController(IDatabaseService databaseService, ILogger<RecordsController> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        /// <summary>
        /// Verificar conexión a PostgreSQL
        /// </summary>
        /// <returns>Estado de la conexión y hora del servidor</returns>
        [HttpPost("verify")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> VerifyConnection()
        {
            _logger.LogInformation("POST /api/records/verify - Verificando conexión");
            
            var (success, message, serverTime) = await _databaseService.VerifyConnectionAsync();

            if (!success)
            {
                _logger.LogWarning("Fallo en verificación de conexión: {Message}", message);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = message,
                    timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                success = true,
                message = message,
                serverTime = serverTime,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Obtener todos los registros
        /// </summary>
        /// <returns>Lista de registros</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRecords()
        {
            _logger.LogInformation("GET /api/records - Obteniendo todos los registros");
            
            var (success, message, records) = await _databaseService.GetAllRecordsAsync();

            if (!success)
            {
                _logger.LogWarning("Error al obtener registros: {Message}", message);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = message,
                    timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                success = true,
                message = message,
                count = records?.Count ?? 0,
                data = records ?? new List<DemoRecord>(),
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Insertar un nuevo registro
        /// </summary>
        /// <param name="nombre">Nombre del registro</param>
        /// <param name="email">Email del registro</param>
        /// <returns>ID del registro creado</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertRecord([FromBody] CreateRecordRequest request)
        {
            _logger.LogInformation("POST /api/records - Insertando nuevo registro: {Nombre}, {Email}", 
                                 request?.Nombre, request?.Email);

            if (request == null || string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.Email))
            {
                _logger.LogWarning("Validación fallida: nombre o email vacíos");
                return BadRequest(new
                {
                    success = false,
                    message = "❌ El nombre y email son obligatorios",
                    timestamp = DateTime.UtcNow
                });
            }

            var (success, message, recordId) = await _databaseService.InsertRecordAsync(request.Nombre, request.Email);

            if (!success)
            {
                _logger.LogWarning("Error al insertar registro: {Message}", message);
                return BadRequest(new
                {
                    success = false,
                    message = message,
                    timestamp = DateTime.UtcNow
                });
            }

            return CreatedAtAction(nameof(GetRecords), new
            {
                success = true,
                message = message,
                recordId = recordId,
                nombre = request.Nombre,
                email = request.Email,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Prueba rápida con datos de ejemplo
        /// </summary>
        [HttpPost("demo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DemoInsert()
        {
            _logger.LogInformation("POST /api/records/demo - Insertando registro de demostración");
            
            var (success, message, recordId) = await _databaseService.InsertRecordAsync(
                "Usuario Demo",
                "demo@example.com"
            );

            if (!success)
            {
                return BadRequest(new { success = false, message, timestamp = DateTime.UtcNow });
            }

            return Ok(new
            {
                success = true,
                message = message,
                recordId = recordId,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Modelo para crear un nuevo registro
    /// </summary>
    public class CreateRecordRequest
    {
        public string? Nombre { get; set; }
        public string? Email { get; set; }
    }
}
