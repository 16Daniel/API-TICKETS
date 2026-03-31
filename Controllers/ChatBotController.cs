using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TICKETSAPI.ModelsTickets;

namespace TICKETSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatBotController : ControllerBase
    {
        protected TicketsContext _tdbContext;

        public ChatBotController(TicketsContext tdbContext)
        {
            _tdbContext = tdbContext;
        }

        [HttpPost]
        [Route(("validar-ticket"))]
        public async Task<IActionResult> ValidarTicket([FromForm] string idTicket)
        {
            try
            {
       
                string[] data = idTicket.Contains("T") ? idTicket.Split('T'): idTicket.Split('F');

                int ids = 0;
                int numt = 0;
                try
                {
                    ids = int.Parse(data[0]);
                    numt = int.Parse(data[1]);

                }
                catch (Exception ext) 
                {
                    return BadRequest(new {message = "el ticket no es válido"});
                }
                var _connectionString = _tdbContext.Database.GetDbConnection().ConnectionString;

                using SqlConnection conn = new SqlConnection(_connectionString);
                using SqlCommand cmd = new SqlCommand("SP_VALIDAR_TICKET", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IDS", ids);
                cmd.Parameters.AddWithValue("@NUMT", numt);

                await conn.OpenAsync();

                using SqlDataReader reader = await cmd.ExecuteReaderAsync();

                // PRIMER RESULTSET
                int total = 0;
                if (await reader.ReadAsync())
                {
                    total = reader.GetInt32(0);
                }

                // SEGUNDO RESULTSET
                List<ValidarTicketDetalle> detalle = new();

                await reader.NextResultAsync();
                while (await reader.ReadAsync())
                {
                    detalle.Add(new ValidarTicketDetalle
                    {
                        FO = reader.GetInt32(0),
                        TITULO = reader.GetString(1),
                        DESCRIPCION = reader.GetString(2),
                        UDSPAGADAS = reader.GetDouble(3),
                        SECCION = reader.GetString(4),
                        FECHA = reader.GetDateTime(5),
                        HORA = reader.GetDateTime(6)
                    });
                }

                Boolean ticketvalido = false;
                if (total > 0) { ticketvalido = true; }

                if (ticketvalido)
                {
                    int totalayc = 0;

                    foreach(var item in detalle) 
                    {
                        totalayc = (int)(totalayc + item.UDSPAGADAS);
                    }

                    if(detalle.Count > 0) 
                    {
                        DateTime hora = detalle[0].HORA;
                        DateTime fecha = detalle[0].FECHA;

                        DateTime fechayhora = new DateTime(
                            fecha.Year,
                            fecha.Month,
                            fecha.Day,
                            hora.Hour,
                            hora.Minute,
                            hora.Second,
                            hora.Millisecond
                        );

                        var datat = new { branchId = ids, branch = detalle[0].TITULO, total_redemption = totalayc, datetime = fechayhora };
                        return Ok(datat);
                    } 
                    else 
                    {
                        return StatusCode(204); 
                    }
                    
                }
                else 
                {
                    return NotFound();
                } 
            }
            catch (Exception ex) 
            {
                return StatusCode(500, ex.Message);
            }
        }
    }

    public class ValidarTicketTotal
    {
        public int TOT { get; set; }
    }

    public class ValidarTicketDetalle
    {
        public int FO { get; set; }
        public string TITULO { get; set; }
        public string DESCRIPCION { get; set; }
        public double UDSPAGADAS { get; set; }
        public string SECCION { get; set; }
        public DateTime FECHA { get; set; }
        public DateTime HORA { get; set; }
    }

}
