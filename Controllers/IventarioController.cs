using DashboardApi.ModelsBD2;
using DashboardApi.ModelsDBRebel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TICKETSAPI.Funciones;
using TICKETSAPI.ModelsBD2Prueba;

namespace TICKETSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IventarioController : ControllerBase
    {
        protected BD2Context _context;
        public FuncionesInventario _fnInv;
        protected DBRebelContext _dbRebelContext;
        protected BD2ContextPrueba _contextPrueba;
        public IventarioController(BD2Context contextdb2, FuncionesInventario fnInv, DBRebelContext dbRebelContext, BD2ContextPrueba contextprueba)
        {
            _context = contextdb2;
            _fnInv = fnInv;
            _dbRebelContext = dbRebelContext;
            _contextPrueba = contextprueba;
        }

        [HttpGet]
        [Route("getArticulos/{idSucursal}")]
        public async Task<ActionResult> getArticulosInventario(int idSucursal)
        {
            List<StockDto> _stock = new List<StockDto>();
            List<StockDto> _stock2 = new List<StockDto>();
            var timeNow = DateTime.Now;
            var serie = _context.RemCajasfronts.FirstOrDefault(x => x.Idfront == idSucursal).Codalmventas;
            if (serie != null)
            {
                _stock = _context.Stocks
                    .Join(_context.Articuloscamposlibres,
                    art => art.Codarticulo,
                    stk => stk.Codarticulo,
                    (art, stk) => new StockDto()
                    {
                        Codalmacen = art.Codalmacen,
                        Codarticulo = stk.Codarticulo,
                        Regulariza = stk.Regulariza,
                        Unidadessat = stk.Unidadessat,
                        Unidadmedida = stk.UnidadmedidaReg,
                        RegularizaSemanal = stk.RegularizaSemanal,
                        Orden = stk.OrdenInventarioApp,

                    })
                    .Join(_context.Articulos1,
                    art => art.Codarticulo,
                    stk => stk.Codarticulo,
                    (art, stk) => new StockDto()
                    {
                        Codalmacen = art.Codalmacen,
                        Descripcion = stk.Descripcion,
                        Codarticulo = art.Codarticulo,
                        Regulariza = art.Regulariza,
                        Unidadessat = art.Unidadessat,
                        Unidadmedida = stk.Unidadmedida,
                        RegularizaSemanal = art.RegularizaSemanal,
                        Orden = art.Orden,
                    })
                    .Where(s => s.Codalmacen == serie && s.RegularizaSemanal == "T").ToList();

            }

            if (timeNow.Hour < 3)
            {

                _stock = _stock.Where(s => !_context.Moviments.Where(es => es.Fecha == DateTime.Now.Date.AddDays(-1) && es.Codarticulo == s.Codarticulo && es.Codalmacenorigen == s.Codalmacen && es.Codalmacendestino == "" && es.Hora.Value.Hour > 3 && es.Tipo == "REG").Any()).ToList();
                _stock2 = _stock.Where(s => !_context.Moviments.Where(es => es.Fecha == DateTime.Now.Date && es.Codarticulo == s.Codarticulo && es.Codalmacenorigen == s.Codalmacen && es.Codalmacendestino == "" && es.Tipo == "REG").Any()).ToList();
                if (_stock.LongCount() > 0)
                {

                    if (_stock2.LongCount() > 0)
                    {

                        return StatusCode(200, _stock);

                    }
                    else
                    {
                        return StatusCode(200, _stock2);

                    }

                }
                else
                {

                    return StatusCode(200, _stock);

                }
            }
            else
            {

                _stock = _stock.Where(s => !_context.Moviments.Where(es => es.Fecha == DateTime.Now.Date && es.Codarticulo == s.Codarticulo && es.Codalmacenorigen == s.Codalmacen && es.Codalmacendestino == "" && es.Hora.Value.Hour >= 7 && es.Hora.Value.Hour < 17 && es.Tipo == "REG").Any()).ToList();
                return StatusCode(200, _stock);
            }

        }

        [HttpGet]
        [Route("getArticulosV/{idSucursal}")]
        public async Task<ActionResult> getArticulosInventarioV(int idSucursal)
        {
            List<StockDto> _stock = new List<StockDto>();
            var Hrs = DateTime.Now.Hour;
            var ampm = Hrs >= 12 ? "PM" : "AM";

            var serie = _context.RemCajasfronts.FirstOrDefault(x => x.Idfront == idSucursal).Codalmventas;
            if (serie != null)
            {
                _stock = _context.Stocks
                    .Join(_context.Articuloscamposlibres,
                    art => art.Codarticulo,
                    stk => stk.Codarticulo,
                    (art, stk) => new StockDto()
                    {
                        Codalmacen = art.Codalmacen,
                        Codarticulo = stk.Codarticulo,
                        Regulariza = stk.Regulariza,
                        Unidadessat = stk.Unidadessat,
                        Unidadmedida = stk.UnidadmedidaReg,
                        RegularizaSemanal = stk.RegularizaSemanal,
                        Orden = stk.OrdenInventarioApp,
                    })
                    .Join(_context.Articulos1,
                    art => art.Codarticulo,
                    stk => stk.Codarticulo,
                    (art, stk) => new StockDto()
                    {
                        Codalmacen = art.Codalmacen,
                        Descripcion = stk.Descripcion,
                        Codarticulo = art.Codarticulo,
                        Regulariza = art.Regulariza,
                        Unidadessat = art.Unidadessat,
                        Unidadmedida = stk.Unidadmedida,
                        RegularizaSemanal = art.RegularizaSemanal,
                        Orden = art.Orden,
                    })
                    .Where(s => s.Codalmacen == serie && s.RegularizaSemanal == "T").ToList();

            }
            if (ampm.ToString().Equals("AM"))
            {

                _stock = _stock.Where(s => !_context.Moviments.Where(es => es.Fecha == DateTime.Now.Date && es.Codarticulo == s.Codarticulo && es.Codalmacenorigen == s.Codalmacen && es.Codalmacendestino == "" && es.Hora.Value.Hour > 1 && es.Hora.Value.Hour < 7 && es.Tipo == "REG").Any()).ToList();


                return StatusCode(200, _stock);
            }
            else
            {

                _stock = _stock.Where(s => !_context.Moviments.Where(es => es.Fecha == DateTime.Now.Date.AddDays(1) && es.Codarticulo == s.Codarticulo && es.Codalmacenorigen == s.Codalmacen && es.Codalmacendestino == "" && es.Hora.Value.Hour > 1 && es.Hora.Value.Hour < 7 && es.Tipo == "REG").Any()).ToList();
                return StatusCode(200, _stock);
            }


        }

        [HttpGet]
        [Route("ValidateStock/{id_sucursal}/{codarticulo}/{cantidad}")]
        public async Task<ActionResult> ValidateStock(int id_sucursal, decimal cantidad, int codarticulo)
        {
             Boolean Success = false;
             string Message = string.Empty;
            decimal _cantidad = 0;
            try
            {
                if (_fnInv.StockValidate(id_sucursal, codarticulo) < 0)
                {
                    _cantidad = _fnInv.StockValidate(id_sucursal, codarticulo) + cantidad;

                }
                else
                {
                    _cantidad = _fnInv.StockValidate(id_sucursal, codarticulo) - cantidad;
                }

                _cantidad = _cantidad < 0 ? _cantidad * -1 : _cantidad;
                if (_cantidad >= 10)
                {
                    Success = true;
                    Message = "" + _cantidad;
                }
                else
                {
                    Success = false;
                    Message = "" + _cantidad;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }

            return Ok(new { Success = Success, Message = Message });
        }


        [HttpGet]
        [Route("ValidateStockV/{id_sucursal}/{codarticulo}/{cantidad}")]
        public async Task<ActionResult> ValidateStockV(int id_sucursal, decimal cantidad, int codarticulo)
        {
            Boolean Success = false;
            string Message = string.Empty;
            decimal _cantidad = 0;
            try
            {
                if (_fnInv.StockValidateV(id_sucursal, codarticulo) < 0)
                {
                    _cantidad = _fnInv.StockValidateV(id_sucursal, codarticulo) + cantidad;

                }
                else
                {
                    _cantidad = _fnInv.StockValidateV(id_sucursal, codarticulo) - cantidad;
                }

                _cantidad = _cantidad < 0 ? _cantidad * -1 : _cantidad;
                if (_cantidad >= 10)
                {
                    Success = true;
                    Message = "" + _cantidad;
                }
                else
                {
                    Success = false;
                    Message = "" + _cantidad;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }

            return Ok(new { Success = Success, Message = Message });
        }



        [HttpGet]
        [Route("ValidarCaptura/{id_sucursal}/")]
        public async Task<ActionResult> ValidarCaptura(int id_sucursal)
        {
            Boolean capturar = false;
            int? ids; 
            try
            {
                SqlConnection connection = (SqlConnection)_context.Database.GetDbConnection();
                SqlCommand cmd = connection.CreateCommand();
                connection.Open();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "SPS_INV_TEORICO";
                cmd.Parameters.Add("@SERIE", System.Data.SqlDbType.Int, 2).Value = id_sucursal;
                cmd.CommandTimeout = 120;
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {

                    ids = (int)reader["IDFRONT"];
                    if (ids != null)
                    {
                        capturar = true;
                    }

                }
                connection.Close();

                return Ok(new { capturarInv = capturar });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }



        /// <summary>
        /// POST Para actualizar Stock y hacer regularizacion en ICG
        /// </summary>
        /// <param name="dataBase">dataBase base de datos que se obtiene de login</param>
        /// <returns></returns>
        [HttpPost("AddRegularizate", Name = "AddRegularizate")]
        public async Task<ActionResult> AddRegularizate([FromForm] int codArticulo, [FromForm] string codAlmacen, [FromForm] double cantidad)
        {
            try
            {
                var response = _fnInv.UpdateStock(codArticulo, codAlmacen, cantidad);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }

          
        }

        [HttpPost("AddRegularizateV", Name = "AddRegularizateV")]
        public async Task<ActionResult> AddRegularizateV([FromForm] int codArticulo, [FromForm] string codAlmacen, [FromForm] double cantidad)
        {

            try
            {
                var response = _fnInv.UpdateStockV(codArticulo, codAlmacen, cantidad);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }

        [HttpPost("AddInventario")]
        public async Task<ActionResult> AddInventario([FromBody] InventarioDto data)
        {

            try
            {

                DashboardApi.ModelsDBRebel.Inventario inventario = new DashboardApi.ModelsDBRebel.Inventario();

                inventario.Branch = data.Branch;
                inventario.InvInicial = data.InvInicial;
                inventario.InvReg = data.InvReg;
                inventario.Diferencia = data.Diferencia;
                inventario.Intentos = data.Intentos;
                inventario.Articulo = data.Articulo;
                inventario.CreatedBy = data.CreatedBy;
                inventario.CreatedDate = data.CreatedDate;
                inventario.UpdatedBy = data.UpdatedBy;
                inventario.UpdatedDate = data.UpdatedDate;

                _dbRebelContext.Inventarios.Add(inventario);
                await _dbRebelContext.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }

        [HttpGet]
        [Route("getUmedidas/{codArticulo}/")]
        public async Task<ActionResult> getUmedidas(int codArticulo)
        {
            try
            {
                var registros = _contextPrueba.ItProductos.Where(x => x.Codarticulo == codArticulo).ToList(); 
                var registrosfiltro = registros.Select(o => o.Iuds).Distinct().ToList();
                return Ok(registrosfiltro);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }



    }
    public class StockDto
    {
        public string Codalmacen { get; set; }
        public string Descripcion { get; set; }
        public int Codarticulo { get; set; }
        public string? Regulariza { get; set; }
        public string? Unidadessat { get; set; }
        public string? Unidadmedida { get; set; }
        public double? Stock1 { get; set; }
        public DateTime? Ultfecha { get; set; }
        public string? RegularizaSemanal { get; set; }
        public string? InventarioMensual { get; set; }
        public int? Orden { get; set; }
    }

    public class InventarioDto
    {

        public int Id { get; set; }
        public int Branch { get; set; }
        public decimal InvInicial { get; set; }
        public decimal InvReg { get; set; }
        public decimal Diferencia { get; set; }
        public int Intentos { get; set; }
        public string? Articulo { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

}
