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

        [HttpPost]
        [Route("traducirPedido")]
        public async Task<IActionResult> traducirpedido([FromBody] traduccionmodel pedido)
        {
            try
            {
               foreach(var item in pedido.items) 
                {
                    var itemdiccionario = _tdbContext.DiccionarioDeliveries.Where(x=>x.Nombre.ToLower() == item.nombre.ToLower() && x.Tienda == pedido.tienda).FirstOrDefault();
                    if (itemdiccionario != null) 
                    {
                        item.codarticulo = itemdiccionario.Codicg; 
                    }
                    foreach (var itemModel in item.subitems) 
                    {
                        var itemdiccionario2 = _tdbContext.DiccionarioDeliveries.Where(x => x.Nombre.ToLower() == itemModel.nombre.ToLower() && x.Tienda == pedido.tienda).FirstOrDefault();
                        if (itemdiccionario2 != null) 
                        {
                            itemModel.codarticulo = itemdiccionario2.Codicg;
                            itemModel.codmodificador = itemdiccionario2.Codmodificador; 
                        }
                    }
                }
                return Ok(pedido);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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

    public class traduccionmodel 
    {
        public string tienda { get; set; }
        public List<itemModel> items { get; set; }
    }
    public class subitemModel
    {
        public string nombre { get; set; }
        public double precio { get; set; }
        public int cantidad { get; set; }

        public int? codarticulo { get; set; }
        public int? codmodificador { get; set; }

    }

    public class itemModel
    {
        public string nombre { get; set; }
        public double precio { get; set; }
        public int cantidad { get; set; }
        public int? codarticulo { get; set; }
        public List<subitemModel> subitems { get; set; }
    }

}
