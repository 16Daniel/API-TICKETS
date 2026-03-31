using DashboardApi.Mail;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TICKETSAPI.ModelsTickets;

namespace TICKETSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailController : ControllerBase
    {
        protected TicketsContext _tdbContext;
        protected MailC _fnMail;  

        public MailController(TicketsContext tdbContext, MailC fnmail)
        {
            _tdbContext = tdbContext;
            _fnMail = fnmail;
        }

        [HttpPost]
        [Route(("enviarCorreo"))]
        public async Task<IActionResult> enviarcorreo([FromBody] NuevoCorreoModel model)
        {
            try
            {
                _fnMail.EnviarCorreoTareas(model.destinatario,model.body,model.titulo); 
                return Ok();               
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }



    }

    public class NuevoCorreoModel 
    {
        public string titulo { get; set; }
        public string body { get; set; }
        public string destinatario { get; set; }
    }
}
