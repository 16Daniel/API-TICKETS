using DashboardApi.Controllers;
using DashboardApi.ModelsBD2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TICKETSAPI.ModelsTickets;

namespace TICKETSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly ILogger<CatalogosController> _logger;
        protected BD2Context _contextdb2;
        protected TicketsContext _tdbContext;

        public TicketsController(ILogger<CatalogosController> logger, BD2Context db2c, TicketsContext tdbc)
        {
            _logger = logger;
            _contextdb2 = db2c;
            _tdbContext = tdbc;
        }

        [HttpPost]
        [Route("getTicktesH")]
        public async Task<ActionResult> GetTickets([FromForm]DateTime fechaini, [FromForm] DateTime fechafin, [FromForm] string idu, [FromForm] string rol)
        {
            try
            {  
                List<Ticket> tickets = new List<Ticket>();
                if (rol == "2") 
                {
                     tickets = _tdbContext.Tickets.Where(x => x.Iduser == idu && x.Fechafin.Value.Date >= fechaini.Date && x.Fechafin.Value.Date <= fechafin.Date).ToList();
                }

                if (rol == "4") 
                {
                     tickets = _tdbContext.Tickets.Where(x => x.Responsable == idu && x.Fechafin.Value.Date >= fechaini.Date && x.Fechafin.Value.Date <= fechafin.Date).ToList();
                }
                  
                return StatusCode(200, tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.ToString(),
                });
            }
        }

        [HttpPost]
        [Route("addTicket")]
        public async Task<ActionResult> createtk(Ticket tk)
        {
            try
            {
                // Calcular la diferencia
                TimeSpan diferencia = (TimeSpan)(tk.Fechafin - tk.Fecha);

                string duracion = ""; 
                // Construir el string
                duracion = $"Días: {diferencia.Days}, Horas: {diferencia.Hours}, Minutos: {diferencia.Minutes}";

                tk.Duracion = duracion;
                _tdbContext.Tickets.Add(tk);    
                await _tdbContext.SaveChangesAsync();

                return StatusCode(200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.ToString(),
                });
            }
        }

    }
}
