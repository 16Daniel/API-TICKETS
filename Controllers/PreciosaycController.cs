using DashboardApi.ModelsBD2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TICKETSAPI.ModelsTickets;

namespace TICKETSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreciosaycController : ControllerBase
    {
        private readonly ILogger<CalendarioNominaController> _logger;
        protected TicketsContext _tdbContext;
        protected BD2Context _bd2Context;

        public PreciosaycController(ILogger<CalendarioNominaController> logger, TicketsContext tkc, BD2Context bd2c)
        {
            _logger = logger;
            _tdbContext = tkc;
            _bd2Context = bd2c;
        }

        [HttpGet]
        [Route("getPreciosAYC")]
        public async Task<IActionResult> GetPreciosAYC()
        {
            try 
            {
                var datadb = _tdbContext.PreciosAycs.ToList(); 
                List<Object> data = new List<Object>(); 
                foreach(var item in datadb) 
                {
                    string nombresucursal = "";
                    if (item.Grupo == "QRO" || item.Grupo == "CDMX" || item.Grupo == "SLP")
                    {
                        var sucursal = _bd2Context.RemFronts.Where(x => x.Idfront == item.Ids).FirstOrDefault();
                        if (sucursal != null) { nombresucursal = sucursal.Titulo; }
                    }
                    else 
                    {
                        var sucursal = _tdbContext.SucursalesFranquicias.Where(x=> x.Grupo == item.Grupo && x.Idf == item.Ids).FirstOrDefault();
                        if (sucursal != null) { nombresucursal = sucursal.Nombre; }
                    }

                    data.Add(new
                    {
                        Id = item.Id,
                        Ids = item.Ids,
                        CLunes = item.CLunes,
                        CMartes = item.CMartes,
                        CMiercoles = item.CMiercoles,
                        CJueves = item.CJueves,
                        CViernes = item.CViernes,
                        CSabado = item.CSabado,
                        CDomingo = item.CDomingo,
                        Grupo = item.Grupo,
                        nombreSuc = nombresucursal
                    });
                }

                return Ok(data); 
            }
            catch (Exception ex) 
            {
               return StatusCode(500, ex.Message);
            }

        }

        [HttpPost]
        [Route("agregarPreciosAYC")]
        public async Task<IActionResult> agregarPrecios([FromBody] PreciosAycModel model)
        {
            try
            {
                _tdbContext.PreciosAycs.Add(new PreciosAyc
                {
                    Ids = model.Ids,
                    CLunes = model.CLunes,
                    CMartes = model.CMartes,
                    CMiercoles = model.CMiercoles,
                    CJueves = model.CJueves,
                    CViernes = model.CViernes,
                    CSabado = model.CSabado,
                    CDomingo = model.CDomingo,
                    Grupo = model.Grupo
                }); 
                await _tdbContext.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("actualizarPreciosAYC")]
        public async Task<IActionResult> actualizarPrecios([FromBody] PreciosAycModel model)
        {
            try
            {  
                var itemreg = _tdbContext.PreciosAycs.Where(x=> x.Id == model.Id).FirstOrDefault();
                if (itemreg != null) 
                {
                    itemreg.CLunes = model.CLunes;
                    itemreg.CMartes = model.CMartes;
                    itemreg.CMiercoles = model.CMiercoles;
                    itemreg.CJueves = model.CJueves; 
                    itemreg.CViernes = model.CViernes;
                    itemreg.CSabado = model.CSabado;
                    itemreg.CDomingo = model.CDomingo;

                    _tdbContext.PreciosAycs.Update(itemreg); 
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
        [Route("eliminarPreciosAYC/{id}")]
        public async Task<IActionResult> eliminarColorAYC(int id)
        {
            try
            {
                var preciosayc = _tdbContext.PreciosAycs.Where(x => x.Id == id).FirstOrDefault();
                if ( preciosayc != null)
                {
                    _tdbContext.PreciosAycs.Remove(preciosayc); 
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

    public partial class PreciosAycModel
    {
        public int? Id { get; set; }
        public int Ids { get; set; }
        public int CLunes { get; set; } 
        public int CMartes { get; set; }
        public int CMiercoles { get; set; }
        public int CJueves { get; set; } 
        public int CViernes { get; set; }
        public int CSabado { get; set; } 
        public int CDomingo { get; set; } 
        public string Grupo { get; set; }
    }
}
