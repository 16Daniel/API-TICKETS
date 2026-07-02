using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TICKETSAPI.ModelsTickets;

namespace TICKETSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VentasController : ControllerBase
    {
        protected TicketsContext _tdbContext;

        public VentasController(TicketsContext tdbContext) 
        {
            _tdbContext = tdbContext;
        }

        [HttpPost]
        [Route("agregarVenta")]
        public async Task<IActionResult> agregarVenta([FromForm] string sucursal, [FromForm] DateTime fecha, [FromForm] double ventaSalon, [FromForm] double ventaDelivery, [FromForm] double ventaTotal)
        {
            try
            {
                var reg = _tdbContext.VentaFranquicias.Where(x => x.Sucursal.Trim() == sucursal.Trim() && x.Fecha.Value.Date == fecha.Date).FirstOrDefault();
                if (reg == null)
                {
                    _tdbContext.VentaFranquicias.Add(new VentaFranquicia()
                    {
                        Sucursal = sucursal.Trim(),
                        Fecha = fecha,
                        VentaSalon = ventaSalon,
                        VentaDelivery = ventaDelivery,
                        VentaTotal = ventaTotal,
                    });
                }
                else 
                {
                    reg.Sucursal = sucursal.Trim();
                    reg.Fecha = fecha;  
                    reg.VentaSalon = ventaSalon;
                    reg.VentaDelivery = ventaDelivery;
                    reg.VentaTotal = ventaTotal;
                }
                await _tdbContext.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("agregarVentaDelivery")]
        public async Task<IActionResult> agregarVentaDelivery([FromForm] string sucursal, [FromForm] DateTime fecha, [FromForm] double ventaUber, [FromForm] double ventaRapi, [FromForm] double ventaDidi)
        {
            try
            {
                var reg = _tdbContext.VentaFranquiciasDeliveries.Where(x => x.Sucursal.Trim() == sucursal.Trim() && x.Fecha.Date == fecha.Date).FirstOrDefault();
                if (reg == null)
                {
                    _tdbContext.VentaFranquiciasDeliveries.Add(new VentaFranquiciasDelivery()
                    {
                        Sucursal = sucursal.Trim(),
                        Fecha = fecha,
                        Uber = ventaUber,
                        Rappi = ventaRapi,
                        Didi = ventaDidi    
                    });
                }
                else
                {
                    reg.Sucursal = sucursal.Trim();
                    reg.Fecha = fecha;
                    reg.Uber = ventaUber;
                    reg.Rappi = ventaRapi;
                    reg.Didi = ventaDidi; 
                }
                await _tdbContext.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("agregarTotalAyc")]
        public async Task<IActionResult> agregarTotalAyc([FromForm] string sucursal, [FromForm] DateTime fecha, [FromForm] int totalCobros)
        {
            try
            {
                var reg = _tdbContext.VentaFranquicias.Where(x => x.Sucursal.Trim() == sucursal.Trim() && x.Fecha.Value.Date == fecha.Date).FirstOrDefault();
                if (reg != null)
                {
                    reg.Cobros = totalCobros;
                    _tdbContext.VentaFranquicias.Update(reg); 
                }
                await _tdbContext.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost]
        [Route("agregarHitsDeVentas")]
        public async Task<IActionResult> agregarDetallesVentas([FromForm] string sucursal, [FromForm] DateTime fecha, [FromForm] int cervezas, [FromForm]int destilados, [FromForm] int bsa, [FromForm] int itemsmenu)
        {
            try
            {
                var reg = _tdbContext.VentaFranquicias.Where(x => x.Sucursal.Trim() == sucursal.Trim() && x.Fecha.Value.Date == fecha.Date).FirstOrDefault();
                if (reg != null)
                {
                    reg.Cervezas = cervezas;
                    reg.Destilados = destilados;
                    reg.Bsa = bsa;
                    reg.ItemsMenu = itemsmenu; 
                    _tdbContext.VentaFranquicias.Update(reg);
                }
                await _tdbContext.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}
