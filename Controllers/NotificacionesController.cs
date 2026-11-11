using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Mvc;
using TICKETSAPI.Funciones;

namespace TICKETSAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificacionesController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificacionesController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost("enviar-push")]
        public async Task<IActionResult> EnviarNotificacionPush([FromBody] PushNotificationRequest request, CancellationToken cancellationToken)
        {
            var resultado = await _notificationService.EnviarPushAsync(request, cancellationToken);

            if (!resultado.Exito)
            {
                return BadRequest(new { exito = false, error = resultado.Error });
            }

            return Ok(new
            {
                exito = true,
                enviados = resultado.Enviados,
                fallidos = resultado.Fallidos,
                tokensInvalidos = resultado.TokensInvalidos
            });
        }
    }
        public class PushNotificationRequest
    {
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public List<string> tokens { get; set; }
        public List<string> usuarios { get; set; }
    }

}