using Microsoft.AspNetCore.Mvc;
using TICKETSAPI.Repositories;

namespace TICKETSAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkforceController : ControllerBase
    {
        private readonly IWorkforceRepository _workforceRepository;

        public WorkforceController(IWorkforceRepository workforceRepository)
        {
            _workforceRepository = workforceRepository;
        }

        [HttpPost("procesar")]
        public async Task<IActionResult> Procesar(
            [FromQuery] DateTime? fecha,
            CancellationToken cancellationToken)
        {
            if (!fecha.HasValue)
            {
                return BadRequest(new
                {
                    mensaje = "El parámetro fecha es obligatorio. Formato esperado: yyyy-MM-dd."
                });
            }

            try
            {
                await _workforceRepository.ProcesarAsync(
                    fecha.Value.Date,
                    cancellationToken);

                return Ok(new
                {
                    mensaje = "El proceso Workforce se ejecutó correctamente.",
                    fecha = fecha.Value.Date.ToString("yyyy-MM-dd")
                });
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, new
                {
                    mensaje = "El proceso Workforce fue cancelado."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Ocurrió un error al ejecutar el proceso Workforce.",
                    detalle = ex.Message
                });
            }
        }
    }
}
