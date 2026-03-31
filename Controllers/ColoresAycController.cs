using DashboardApi.ModelsBD2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TICKETSAPI.ModelsTickets;

namespace TICKETSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ColoresAycController : ControllerBase
    {
        private readonly ILogger<CalendarioNominaController> _logger;
        protected TicketsContext _tdbContext;
        protected BD2Context _bd2Context;

        public ColoresAycController(ILogger<CalendarioNominaController> logger, TicketsContext tkc, BD2Context bd2c)
        {
            _logger = logger;
            _tdbContext = tkc;
            _bd2Context = bd2c;
        }

        [HttpGet]
        [Route("getColoresAYC")]
        public async Task<IActionResult> GetColoresAYC()
        {
            try
            {
                var data = _tdbContext.ColoresAycs.ToList();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        [HttpPost]
        [Route("agregarColorAYC")]
        public async Task<IActionResult> agregarColorAYC([FromForm] string color, [FromForm] double precio)
        {
            try
            {
                _tdbContext.ColoresAycs.Add(new ColoresAyc() { Color= color, Precio = precio });
                await _tdbContext.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        [Route("actualizarColorAYC")]
        public async Task<IActionResult> actualizarColorAYC([FromForm] int id, [FromForm] string color, [FromForm] double precio)
        {
            try
            {
                var colorayc = _tdbContext.ColoresAycs.Where(x=> x.Id == id).FirstOrDefault();
                if (colorayc != null) 
                {
                    colorayc.Color = color;
                    colorayc.Precio = precio;
                    _tdbContext.ColoresAycs.Update(colorayc); 
                    await _tdbContext.SaveChangesAsync();
                }
                
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete]
        [Route("eliminarColorAYC/{id}")]
        public async Task<IActionResult> eliminarColorAYC(int id)
        {
            try
            {
                var colorayc = _tdbContext.ColoresAycs.Where(x => x.Id == id).FirstOrDefault();
                if (colorayc != null)
                {
                    _tdbContext.ColoresAycs.Remove(colorayc);
                    await _tdbContext.SaveChangesAsync();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
