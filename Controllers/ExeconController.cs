using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

using TP_ITSM.Models;
using TP_ITSM.Models.Execon;
using TP_ITSM.Services.Execon;

namespace TP_ITSM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExeconController : ControllerBase
    {
        private readonly IExeconServices _services;
        private readonly ILogger<ExeconController> _logger;
        
        public ExeconController(IExeconServices services, ILogger<ExeconController> logger)
        {
            _services = services;
            _logger = logger;
        }

        #region Get Task
        [HttpGet]
        [Route("Task/{assignmentId}")]
        public async Task<IActionResult> GetTask(int assignmentId)
        {
            try
            {
                var (success, result) = await _services.GetTask(assignmentId);

                if (!success || string.IsNullOrEmpty(result))
                {
                    ErrorResponse error = new ErrorResponse();
                    error.Mensaje = $"No se encontró la tarea con assignmentId: {assignmentId}";
                    return NotFound(error);
                }
                
                JsonDocument.Parse(result);
                return Content(result, "application/json; charset=utf-8");
            }
            catch (JsonException ex)
            {
                return StatusCode(500, $"El formato de tarea es inválida {ex}");
            }
            
        }
        #endregion

        #region Get Parent Info
        [HttpGet]
        [Route("ParentTask")]
        public async Task<IActionResult> GetParentInfo(string recId, string objName)
        {
            var (success, result) = await _services.GetParentInfo(recId, objName);

            if (!success || string.IsNullOrEmpty(result))
            {
                ErrorResponse error = new ErrorResponse();
                error.Mensaje = $"No se encontró el {objName}";
                return NotFound(error);
            }

            try
            {
                JsonDocument.Parse(result);
                return Content(result, "application/json; charset=utf-8");
            }
            catch (JsonException)
            {
                // Si el JSON no es válido, retornar error
                return StatusCode(500, "El formato del JSON es inválido");
            }

        }
        #endregion

        #region Get Account
        [HttpGet]
        [Route("Account")]
        public async Task<IActionResult> GetAccount(string recId)
        {
            var (success, result) = await _services.GetAccount(recId);

            if (!success || string.IsNullOrEmpty(result))
            {
                ErrorResponse error = new ErrorResponse();
                error.Mensaje = $"No se encontró el Account";
                return NotFound(error);
            }

            try
            {
                JsonDocument.Parse(result);
                return Content(result, "application/json; charset=utf-8");
            }
            catch (JsonException)
            {
                // Si el JSON no es válido, retornar error
                return StatusCode(500, "El formato del JSON es inválido");
            }
        }
        #endregion

        #region Get Location
        [HttpGet]
        [Route("Location")]
        public async Task<IActionResult> GetLocation(string recId)
        {
            var (success, result) = await _services.GetLocation(recId);

            if (!success || string.IsNullOrEmpty(result))
            {
                ErrorResponse error = new ErrorResponse();
                error.Mensaje = $"No se encontró el Location";
                return NotFound(error);
            }

            try
            {
                JsonDocument.Parse(result);
                return Content(result, "application/json; charset=utf-8");
            }
            catch (JsonException)
            {
                // Si el JSON no es válido, retornar error
                return StatusCode(500, "El formato del JSON es inválido");
            }
        }
        #endregion

        #region Get Employee
        [HttpGet]
        [Route("Employee")]
        public async Task<IActionResult> GetEmployee(string owner)
        {
            var (success, result) = await _services.GetEmployee(owner);

            if (!success || string.IsNullOrEmpty(result))
            {
                ErrorResponse error = new ErrorResponse();
                error.Mensaje = $"No se encontró el Owner";
                return NotFound(error);
            }

            try
            {
                JsonDocument.Parse(result);
                return Content(result, "application/json; charset=utf-8");
            }
            catch (JsonException)
            {
                // Si el JSON no es válido, retornar error
                return StatusCode(500, "El formato del JSON es inválido");
            }
        }
        #endregion

        #region Get Task Catalog
        [HttpGet]
        [Route("TaskCatalog")]
        public async Task<IActionResult> GetTaskCatalog(string subject)
        {
            var (success, result) = await _services.GetTaskCatalog(subject);

            if (!success || string.IsNullOrEmpty(result))
            {
                ErrorResponse error = new ErrorResponse();
                error.Mensaje = $"No se encontró el tipo de tarea: {subject}";
                return NotFound(error);
            }

            try
            {
                JsonDocument.Parse(result);
                return Content(result, "application/json; charset=utf-8");
            }
            catch (JsonException)
            {
                // Si el JSON no es válido, retornar error
                return StatusCode(500, "El formato del JSON es inválido");
            }

        }
        #endregion

        #region Request Task
        [HttpGet]
        [Route("TaskRequest/{assignmentId}")]
        public async Task<IActionResult> GetTaskReq(int assignmentId)
        {
            var (success, result) = await _services.GetTaskReq(assignmentId);

            if (!success || string.IsNullOrEmpty(result))
            {
                ErrorResponse error = new ErrorResponse();
                error.Mensaje = $"No se encontró la tarea con assignmentId: {assignmentId}";
                return NotFound(error);
            }

            try
            {
                JsonDocument.Parse(result);
                return Content(result, "application/json; charset=utf-8");
            }
            catch (JsonException)
            {
                // Si el JSON no es válido, retornar error
                return StatusCode(500, "El formato del JSON es inválido");
            }

        }
        #endregion

        #region Scheduled Task ITSM
        [HttpPost]
        [Route("ScheduledTask/{assignmentId}")]
        public async Task<IActionResult> ScheduledTask(int assignmentId)
        {
            _logger.LogInformation($"=== INICIO === Proceso de solicitud tarea {assignmentId}");
            
            try
            {
                var (success, result) = await _services.ScheduledTask(assignmentId);

                if (!success || string.IsNullOrEmpty(result))
                {
                    ErrorResponse error = new ErrorResponse();
                    error.Mensaje = $"Algo ocurrio al realizar la solicitud de la tarea {assignmentId}";
                    _logger.LogWarning($"Algo ocurrio al realizar la solicitud de la tarea {assignmentId}");
                    return NotFound(error);
                }

                JsonDocument.Parse(result);
                _logger.LogInformation($"=== FIN === Proceso de solicitud tarea {assignmentId}, Firebase generado: {result}");
                return Content(result, "application/json; charset=utf-8");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"Error al realizar solicitud de Tarea {assignmentId}");
                // Si el JSON no es válido, retornar error
                return StatusCode(500, "El formato del JSON es inválido");
            }
        }
        #endregion

        #region Update Task ITSM
        [HttpPatch]
        [Route("Task")]
        public async Task<IActionResult> UpTaskITSM([FromBody] object body)
        {
            ResponseTaskTP newBody = JsonSerializer.Deserialize<ResponseTaskTP>(body.ToString());
            
            var (success, result) = await _services.UpTask(newBody);

            if (success)
            {
                return Content(result, "application/json");
            }
            else
            {
                ErrorResponse error = new ErrorResponse();
                error.Mensaje = result;
                return Ok(error);
            }
        }
        #endregion

    }
}
