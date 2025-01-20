using DashboardApi.Controllers;
using DashboardApi.ModelsBD2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TICKETSAPI.ModelsTickets;

namespace TICKETSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly ILogger<CatalogosController> _logger;
        protected BD2Context _contextdb2;
        protected TicketsContext _tdbContext;


        public RolesController(ILogger<CatalogosController> logger, BD2Context db2c, TicketsContext tdbc)
        {
            _logger = logger;
            _contextdb2 = db2c;
            _tdbContext = tdbc;
        }

        [HttpGet]
        [Route("getRoles")]
        public async Task<ActionResult> GetRoles()
        {
            try
            {
                List<CatRole> roles = new List<CatRole>();
                roles = _tdbContext.CatRoles.ToList();
                return StatusCode(200, roles);
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

        [HttpGet]
        [Route("getRutas")]
        public async Task<ActionResult> GetRutas()
        {
            try
            {
                List<CatRuta> rutas = new List<CatRuta>();
                rutas = _tdbContext.CatRutas.ToList();
                return StatusCode(200, rutas);
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

        [HttpGet]
        [Route("getRutasRol/{idr}")]
        public async Task<ActionResult> GetRutasRol(int idr)
        {
            try
            {
                List<CatRuta> rutas = new List<CatRuta>();
                var idrutas = _tdbContext.AccesosRutas.Where(x => x.IdRol == idr).ToList();

                foreach (var item in idrutas)
                {
                    var ruta = _tdbContext.CatRutas.Where(x => x.Id == item.IdRuta).FirstOrDefault();
                    if (ruta != null)
                    {
                        rutas.Add(ruta);
                    }

                }

                return StatusCode(200, rutas);
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
        [Route("createRol")]
        public async Task<ActionResult> createrol(CatRole model)
        {
            try
            {
                CatRole newrol = new CatRole()
                {
                    Descripcion = model.Descripcion,
                };
                _tdbContext.CatRoles.Add(newrol);
                await _tdbContext.SaveChangesAsync();

                return StatusCode(200, new { id = newrol.Id });
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
        [Route("updateRol")]
        public async Task<ActionResult> updaterol(CatRole model)
        {
            try
            {
                _tdbContext.CatRoles.Update(model);
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

        [HttpGet]
        [Route("deleteRol/{id}")]
        public async Task<ActionResult> deleterol(int id)
        {
            try
            {

                var accesos = _tdbContext.AccesosRutas.Where(x => x.IdRol == id).ToList();
                foreach (var acceso in accesos)
                {
                    _tdbContext.AccesosRutas.Remove(acceso);
                    await _tdbContext.SaveChangesAsync();
                }

                var rol = _tdbContext.CatRoles.Find(id);
                if (rol != null)
                {
                    _tdbContext.CatRoles.Remove(rol);
                    await _tdbContext.SaveChangesAsync();
                }
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

        [HttpPost]
        [Route("saveAccesos")]
        public async Task<ActionResult> saveAccesos([FromForm] string jdata, [FromForm] int idr)
        {
            try
            {

                var accesos = _tdbContext.AccesosRutas.Where(x => x.IdRol == idr).ToList();
                foreach (var acceso in accesos)
                {
                    _tdbContext.AccesosRutas.Remove(acceso);
                    await _tdbContext.SaveChangesAsync();
                }

                int[] intArray = JsonSerializer.Deserialize<int[]>(jdata);

                foreach (var item in intArray)
                {
                    _tdbContext.AccesosRutas.Add(new AccesosRuta()
                    {
                        IdRol = idr,
                        IdRuta = item
                    });
                }
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
