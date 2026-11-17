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
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFinal,
            CancellationToken cancellationToken)
        {
            if (!fechaInicio.HasValue || !fechaFinal.HasValue)
            {
                return BadRequest(new
                {
                    mensaje = "Los parámetros fechaInicio y fechaFinal son obligatorios. Formato esperado: yyyy-MM-dd."
                });
            }

            var inicio = fechaInicio.Value.Date;
            var final = fechaFinal.Value.Date;

            if (inicio > final)
            {
                return BadRequest(new
                {
                    mensaje = "La fechaInicio no puede ser posterior a la fechaFinal."
                });
            }

            try
            {
                for (var fecha = inicio; fecha <= final; fecha = fecha.AddDays(1))
                {
                    await _workforceRepository.ProcesarAsync(
                        fecha,
                        cancellationToken);
                }

                return Ok(new
                {
                    mensaje = "El proceso Workforce se ejecutó correctamente para el rango indicado.",
                    fechaInicio = inicio.ToString("yyyy-MM-dd"),
                    fechaFinal = final.ToString("yyyy-MM-dd")
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
