using Dapper;
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
    public class VentasController : ControllerBase
    {
        protected TicketsContext _tdbContext;
        protected BD2Context _bd2Context;

        public VentasController(TicketsContext tdbContext, BD2Context bd2Context) 
        {
            _tdbContext = tdbContext;
            _bd2Context = bd2Context;
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


        [HttpPost("getVentasRW")]
        public async Task<ActionResult<IEnumerable<VentaFranquiciaDTO>>> GetVentasFranquicias([FromBody] ConsultaVentasModel model)
        {
            try 
            {
                var inicio = model.fechaInicio.Date;
                var fin = model.fechaFin.Date;

                var resultado = await (
                    from vf in _tdbContext.VentaFranquicias
                    where vf.Fecha >= inicio && vf.Fecha <= fin
                    join vfd in _tdbContext.VentaFranquiciasDeliveries
                        on new { vf.Sucursal, Fecha = vf.Fecha.Value.Date }
                        equals new { vfd.Sucursal, Fecha = vfd.Fecha.Date } into deliveryGroup
                    from vfd in deliveryGroup.DefaultIfEmpty()
                    select new VentaFranquiciaDTO
                    {
                        Grupo = "REBELDES",
                        Sucursal = vf.Sucursal,
                        Fecha = vf.Fecha.Value,
                        VentaSalon = (decimal)vf.VentaSalon,
                        VentaDelivery = (decimal)vf.VentaDelivery,
                        VentaTotal = (decimal)vf.VentaTotal,
                        Cobros = vf.Cobros,
                        Cervezas = vf.Cervezas,
                        Destilados = vf.Destilados,
                        Bsa = vf.Bsa,
                        ItemsMenu = vf.ItemsMenu,

                        Uber = vfd != null ? (decimal?)vfd.Uber : null,
                        Rappi = vfd != null ? (decimal?)vfd.Rappi : null,
                        Didi = vfd != null ? (decimal?)vfd.Didi : null
                    }
                ).ToListAsync();

                using IDbConnection db = new SqlConnection(_tdbContext.Database.GetConnectionString());

                var parameters = new DynamicParameters();
                parameters.Add("@FECHAINI", inicio, DbType.Date);
                parameters.Add("@FECHAFIN", fin, DbType.Date);

                var result = await db.QueryAsync<DatosVentaFechaDto>(
                    "dbo.GET_DATOS_VENTA_FECHA",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                foreach (var item in result.ToList())
                {
                    resultado.Add(new VentaFranquiciaDTO()
                    {
                        Grupo = item.Region,
                        Sucursal = item.NombreAlmacen,
                        Fecha = item.Fecha,
                        VentaSalon = (decimal)item.Venta,
                        VentaDelivery = (decimal)(item.Venta_Uber + item.Venta_Didi + item.Venta_Rappi),
                        VentaTotal = (decimal)item.Venta_Total,
                        Cobros = item.Ayc,
                        Cervezas = item.Cerveza,
                        Destilados = item.Destilados,
                        Bsa = item.Bsa,
                        ItemsMenu = item.ItemsMenu,

                        Uber = item != null ? (decimal?)item.Venta_Uber : null,
                        Rappi = item != null ? (decimal?)item.Venta_Rappi : null,
                        Didi = item != null ? (decimal?)item.Venta_Didi : null
                    });
                }

                return Ok(resultado.OrderBy(x=>x.Fecha.Date).ThenByDescending(v => v.VentaTotal).ToList());

            } catch(Exception ex) 
            {
                return StatusCode(500, ex.Message);
            }
        }

    }

    public class ConsultaVentasModel 
    {
        public DateTime fechaInicio {  get; set; }
        public DateTime fechaFin {  get; set; }
    }

    public class DatosVentaFechaDto
    {
        public DateTime Fecha { get; set; }
        public string Region { get; set; }
        public long Venta { get; set; }
        public string CodAlmacen { get; set; }
        public string NombreAlmacen { get; set; }
        public int? Ayc { get; set; }
        public int? Bebidas { get; set; }
        public int? Cerveza { get; set; }
        public int? Destilados { get; set; }
        public int? Bsa { get; set; }
        public int? ItemsMenu { get; set; }
        public decimal? Venta_Uber { get; set; }
        public decimal? Venta_Didi { get; set; }
        public decimal? Venta_Rappi { get; set; }
        public long Venta_Total { get; set; }
    }

    public class VentaFranquiciaDTO
    { 
        public string Grupo { get; set; }
        public string Sucursal { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public decimal VentaSalon { get; set; }
        public decimal VentaDelivery { get; set; }
        public decimal VentaTotal { get; set; }
        public decimal? Cobros { get; set; }
        public int? Cervezas { get; set; }
        public int? Destilados { get; set; }
        public int? Bsa { get; set; }
        public int? ItemsMenu { get; set; }

        public decimal? Uber { get; set; }
        public decimal? Rappi { get; set; }
        public decimal? Didi { get; set; }
    }
}
