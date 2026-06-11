using DashboardApi.ModelsBD2;
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
    public class PedidosDeliveryController : ControllerBase
    {
        private readonly ILogger<CalendarioNominaController> _logger;
        protected TicketsContext _tdbContext;
        protected BD2Context _bd2Context;

        public PedidosDeliveryController(ILogger<CalendarioNominaController> logger, TicketsContext tkc, BD2Context bd2c)
        {
            _logger = logger;
            _tdbContext = tkc;
            _bd2Context = bd2c;
        }

        [HttpPost]
        [Route("agregarPedidoDelivery")]
        public async Task<IActionResult> agregarPedidoDelivery([FromBody] PedidoModel model)
        {
            try
            {
                var regbd = _tdbContext.PedidosDeliveries.Where(x => x.Idpedido == model.idpedido && x.App == model.app).FirstOrDefault();
                if (regbd == null)
                {
                    _tdbContext.PedidosDeliveries.Add(new PedidosDelivery()
                    {
                        Idpedido = model.idpedido,
                        App = model.app,
                        Idsuc = model.idsuc,
                        Fecha = DateTimeOffset.FromUnixTimeSeconds(model.fecha).LocalDateTime,
                        Jdata = model.jdata,
                        Procesado = false
                    });
                    await _tdbContext.SaveChangesAsync();
                }
                else 
                {
                    regbd.Jdata = model.jdata;
                    _tdbContext.PedidosDeliveries.Update(regbd);
                    await _tdbContext.SaveChangesAsync();
                }
                return Ok();
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("getPedidoDelivery")]
        public async Task<IActionResult> getPedidoDelivery([FromForm]int ids,[FromForm] DateTime fi,[FromForm] DateTime ff)
        {
            try
            {
                var pedidos = _tdbContext.PedidosDeliveries.Where(x=> x.Idsuc == ids && x.Fecha.Date >= fi.Date && x.Fecha.Date <= ff.Date).ToList();
                return Ok(pedidos.OrderByDescending(x=>x.Fecha));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Route("getDiccionario")]
        public async Task<IActionResult> getdiccionario()
        {
            try
            {
                var data = _tdbContext.DiccionarioDeliveries.ToList();    
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("getClientes")]
        public async Task<IActionResult> getClientes()
        {
            try
            {
                var data = _tdbContext.ClientesDeliveries.ToList();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/diccionario (estructura jerárquica para el frontend)
        [HttpGet("getListadiccionario")]
        public async Task<ActionResult<List<Object>>> GetEstructura()
        {
            List<Object> data = new List<Object>();
            var articulosbd = _tdbContext.DiccionarioDeliveries.Where(x => x.Esmodificador == false).ToList(); 

            foreach(var item in articulosbd) 
            {
                var modificadoresart = _tdbContext.DiccionarioDeliveries.Where(x=> x.Esmodificador == true && x.Idmenu == item.Codicg).ToList(); 
                var artbd = _bd2Context.Articulos1.Where(x=> x.Codarticulo == item.Codicg).FirstOrDefault();

                List<ModificadorDto> datamodificadores = new List<ModificadorDto>();

                foreach (var modificador in modificadoresart) 
                {
                    var artbdmod = _bd2Context.Articulos1.Where(x => x.Codarticulo == modificador.Codicg).FirstOrDefault();
                    string nombremod = "";
                    var modbd = _tdbContext.VwModificadores.Where(x => x.Codmodificador == modificador.Codmodificador).FirstOrDefault();
                    if (modbd != null)
                    {
                        nombremod = modbd.Descripcion;
                    }
                    else 
                    {
                        var modmenu = _tdbContext.VwModificadoresMenus.Where(x => x.Codmodificador == modificador.Codmodificador).FirstOrDefault();
                        if(modmenu != null) { nombremod = modmenu.Descripcion; }
                    }

                    var itemM = new ModificadorDto()
                    {
                        Id = modificador.Id,
                        Tienda = modificador.Tienda,
                        Nombre = modificador.Nombre,
                        CodIcg = modificador.Codicg,
                        CodModificador = modificador.Codmodificador,
                        idMenu = modificador.Idmenu,
                        Nombreicg = artbdmod.Descripcion,
                        Nombremodificador = nombremod
                    };
                    datamodificadores.Add(itemM); 
                }

                var itema = new ArticuloDto()
                {
                    Id = item.Id,
                    Tienda = item.Tienda,
                    Nombre = item.Nombre,
                    CodIcg = item.Codicg,
                    CodModificador = item.Codmodificador,
                    idMenu = item.Idmenu,
                    Nombreicg = artbd.Descripcion,
                    modificadores = datamodificadores
                };

                data.Add(itema);
            }

            return Ok(data);
        }

        // GET: api/diccionario/{id}
        [HttpGet("diccionario/{id}")]
        public async Task<ActionResult<DiccionarioDelivery>> GetItem(int id)
        {
            var item = await _tdbContext.DiccionarioDeliveries.FindAsync(id);
            if (item == null) return NotFound();
            return item;
        }

        // POST: api/diccionario
        [HttpPost("diccionario")]
        public async Task<ActionResult> Create(List<DiccionarioDelivery> data)
        {
            try 
            {
                _tdbContext.DiccionarioDeliveries.AddRange(data);
                await _tdbContext.SaveChangesAsync();
                return Ok();
            }catch(Exception ex) 
            {
                return StatusCode(500,ex.Message);
            }
        }

        // PUT: api/diccionario/{id}
        [HttpPut("diccionario/{id}")]
        public async Task<IActionResult> Update(int id, DiccionarioDelivery item)
        {
            if (id != item.Id) return BadRequest();
            _tdbContext.Entry(item).State = EntityState.Modified;

            try
            {
                await _tdbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_tdbContext.DiccionarioDeliveries.Any(e => e.Id == id))
                    return NotFound();
                throw;
            }
            return NoContent();
        }

        // DELETE: api/diccionario/{id}
        [HttpDelete("diccionario/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _tdbContext.DiccionarioDeliveries.FindAsync(id);
            if (item == null) return NotFound();

            if (!item.Esmodificador && item.Idmenu != null)
            {
                var modificadores = _tdbContext.DiccionarioDeliveries
                    .Where(x => x.Idmenu == item.Codicg && x.Esmodificador == true);
                _tdbContext.DiccionarioDeliveries.RemoveRange(modificadores);
            }
            _tdbContext.DiccionarioDeliveries.Remove(item);
            await _tdbContext.SaveChangesAsync();
            return NoContent();
        }


        [HttpGet("getArticulosIcg")]
        public async Task<ActionResult> GearticulosICg()
        {
            try
            {
                var articulos = _bd2Context.Articulos1
                .Where(x => x.Descatalogado == "F" && (x.Seccion == 1018 || x.Seccion == 2019 || x.Seccion == 1025))
                .Select(s => new { s.Codarticulo, s.Descripcion })
                .ToList();
                return Ok(articulos);
            }
            catch (Exception ex) 
            {
                return StatusCode(500, ex.Message);
            }
           
        }

        [HttpGet("getModificadoresArt/{id}")]
        public async Task<ActionResult> GetmodificadoresArt(int id)
        {
            try
            {
               List<ModificadorArt> modificadores = new List<ModificadorArt>();
                var mods = _tdbContext.VwModificadores.Where(x=> x.Codarticulo == id).ToList();
                var modmenu = _tdbContext.VwModificadoresMenus.Where(x => x.Codarticulo == id).ToList();

                foreach (var mod in mods) 
                {
                    var artsmod = _tdbContext.VwModificadoresDets.Where(x=> x.Codmodificador == mod.Codmodificador).ToList();

                    foreach (var art in artsmod)
                    {
                        string nombreapp = ""; 
                        var reg = _tdbContext.DiccionarioDeliveries.Where(x => x.Esmodificador == true && x.Codicg == art.Codigo).FirstOrDefault();
                        if(reg != null) { nombreapp = reg.Nombre; }
                        var item = new ModificadorArt()
                        {
                            codmodificador = mod.Codmodificador,
                            codarticulo = art.Codigo,
                            descripcion = art.Descripcion,
                            nombremodificador = mod.Descripcion,
                            nombreapp = nombreapp
                        };

                        modificadores.Add(item);
                    }

                }

                foreach (var mod in modmenu)
                {
                    var artsmod = _tdbContext.VwModificadoresDets.Where(x => x.Codmodificador == mod.Codmodificador).ToList();
                    foreach (var art in artsmod)
                    {
                        string nombreapp = "";
                        var reg = _tdbContext.DiccionarioDeliveries.Where(x => x.Esmodificador == true && x.Codicg == art.Codigo).FirstOrDefault();
                        if (reg != null) { nombreapp = reg.Nombre; }

                        var item = new ModificadorArt()
                        {
                            codmodificador = mod.Codmodificador,
                            codarticulo = art.Codigo,
                            descripcion = art.Descripcion,
                            nombremodificador = mod.Descripcion,
                            nombreapp = nombreapp
                        };

                        modificadores.Add(item);
                    }

                }

                return Ok(modificadores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }


        [HttpGet("getCatMarcas")]
        public async Task<ActionResult> GetCatMarcas ()
        {
            try
            {
                var marcas = _tdbContext.CatMarcasDeliveries.ToList(); 
                return Ok(marcas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

    }


    public class PedidoModel   
    {
        public string idpedido {  get; set; }
        public string app { get; set; }
        public int idsuc { get; set; }
        public long fecha { get; set; }
        public string jdata { get; set; }
    }


    public class ModificadorDto
    {
        public int Id { get; set; }
        public string Tienda { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public int CodIcg { get; set; }
        public int? CodModificador { get; set; }
        public int? idMenu { get; set; }
        public string Nombreicg { get; set; }
        public string Nombremodificador { get; set; }
    }

    public class ArticuloDto
    {
        public int Id { get; set; }
        public string Tienda { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public int CodIcg { get; set; }
        public int? CodModificador { get; set; }
        public int? idMenu { get; set; }
        public string Nombreicg {  get; set; }
        public List<ModificadorDto> modificadores { get; set; }
    }

    public class ModificadorArt 
    {
        public int codmodificador { get; set; }
        public int codarticulo { get; set; }
        public string descripcion { get; set; }
        public string nombremodificador { get; set; }
        public string nombreapp {  get; set; }
    }


}
