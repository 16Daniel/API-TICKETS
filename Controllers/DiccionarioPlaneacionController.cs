using DashboardApi.ModelsBD2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TICKETSAPI.ModelsBD2Prueba; 
namespace TICKETSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiccionarioPlaneacionController : ControllerBase
    {
        //protected BD2Context _context;
        protected BD2ContextPrueba _context;
        private readonly IConfiguration _configuration;
        

        public DiccionarioPlaneacionController(BD2ContextPrueba context, IConfiguration configuration)
        {
           _context = context;
           _configuration = configuration;
        }

        [HttpGet]
        [Route("getDiccionario")]
        public async Task<ActionResult> getDiccionario() 
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DB2");
                var productos = new List<ProductoConsulta>();

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

//                    var sql = @"
//                  SELECT PROV.NOMPROVEEDOR,ITP.Rfc,ITP.NoIdentificacion,ITP.codarticulo,ITP.UMEDIDA,ITP.UDS,
//ART.DESCRIPCION,ART.REFPROVEEDOR,ART.MEDIDAREFERENCIA,ART.UNIDADMEDIDA, ARTCL.PLANEACION 
//from IT_PRODUCTOS AS ITP
//LEFT JOIN ARTICULOS AS ART ON ITP.codarticulo = ART.CODARTICULO
//LEFT JOIN PROVEEDORES PROV ON ITP.Rfc COLLATE Latin1_General_CS_AI = PROV.NIF20
//LEFT JOIN ARTICULOSCAMPOSLIBRES ARTCL ON ITP.codarticulo = ARTCL.CODARTICULO";

                    var sql = @" 
 SELECT PROV.NOMPROVEEDOR,ITP.Rfc,ITP.NoIdentificacion,ITP.codarticulo,ITP.UMEDIDA,ITP.UDS,
ART.DESCRIPCION,ART.REFPROVEEDOR,ART.MEDIDAREFERENCIA,ART.UNIDADMEDIDA, ARTCL.PLANEACION,ITP.PUDS,ITP.PUMEDIDA,
ITP.IUDS,ITP.IUMEDIDA
from [BD2_PRUEBA].[dbo].IT_PRODUCTOS AS ITP
LEFT JOIN [BD2_PRUEBA].[dbo].ARTICULOS AS ART ON ITP.codarticulo = ART.CODARTICULO
LEFT JOIN [BD2_PRUEBA].[dbo].PROVEEDORES PROV ON ITP.Rfc COLLATE Latin1_General_CS_AI = PROV.NIF20
LEFT JOIN [BD2_PRUEBA].[dbo].ARTICULOSCAMPOSLIBRES ARTCL ON ITP.codarticulo = ARTCL.CODARTICULO
WHERE ART.DESCATALOGADO = 'F' AND PROV.DESCATALOGADO = 'F'
    "; 

                    using (var command = new SqlCommand(sql, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string temp = reader["PLANEACION"] == DBNull.Value ? "" : reader["PLANEACION"].ToString();
                                Boolean planeacion = temp.Equals("T") ? true : false;
                                var producto = new ProductoConsulta
                                {
                                    nomProveedor = reader["NOMPROVEEDOR"] == DBNull.Value ? null : reader["NOMPROVEEDOR"].ToString(),
                                    Rfc = reader["Rfc"] == DBNull.Value ? null : reader["Rfc"].ToString(),
                                    NoIdentificacion = reader["NoIdentificacion"] == DBNull.Value ? null : reader["NoIdentificacion"].ToString(),
                                    Codarticulo = reader["codarticulo"] == DBNull.Value ? null : reader["codarticulo"].ToString(),
                                    Umedida = reader["UMEDIDA"] == DBNull.Value ? null : reader["UMEDIDA"].ToString(),
                                    Uds = reader["UDS"] == DBNull.Value ? null : reader["UDS"].ToString(),
                                    Descripcion = reader["DESCRIPCION"] == DBNull.Value ? null : reader["DESCRIPCION"].ToString(),
                                    Refproveedor = reader["REFPROVEEDOR"] == DBNull.Value ? null : reader["REFPROVEEDOR"].ToString(),
                                    Medidareferencia = reader["MEDIDAREFERENCIA"] == DBNull.Value ? null : reader["MEDIDAREFERENCIA"].ToString(),
                                    Unidadmedida = reader["UNIDADMEDIDA"] == DBNull.Value ? null : reader["UNIDADMEDIDA"].ToString(),
                                    planeacion = planeacion,
                                    p_uds = reader["PUDS"] == DBNull.Value ? null : reader["PUDS"].ToString(),
                                    p_umedida = reader["PUMEDIDA"] == DBNull.Value ? null : reader["PUMEDIDA"].ToString(),
                                    i_uds = reader["IUDS"] == DBNull.Value ? null : reader["IUDS"].ToString(),
                                    i_umedida = reader["IUMEDIDA"] == DBNull.Value ? null : reader["IUMEDIDA"].ToString(),
                                };

                                productos.Add(producto);
                            }
                        }
                    }
                }

                return Ok(productos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }


        [HttpPost]
        [Route("UpdateMedidaUds")]
        public async Task<ActionResult> updateumedida([FromForm] string rfc, [FromForm] string numid, [FromForm] double uds, [FromForm] string umedida,
            [FromForm] double puds, [FromForm] string pumedida, [FromForm] double iuds, [FromForm] string iumedida, [FromForm] string planeacion)
        {
            try
            {
                var regumedida = _context.ItProductos.Where(x => x.Rfc == rfc && x.NoIdentificacion == numid).FirstOrDefault();
                regumedida.Umedida = umedida;
                regumedida.Uds = (decimal?)uds;
                regumedida.Puds = (decimal?)puds;
                regumedida.Pumedida = pumedida;
                regumedida.Iuds = (decimal?)iuds;
                regumedida.Iumedida = iumedida; 

                _context.ItProductos.Update(regumedida);
                await _context.SaveChangesAsync();

                var artcl = _context.Articuloscamposlibres.Where(x => x.Codarticulo == regumedida.Codarticulo).FirstOrDefault();
                if (artcl != null) 
                {
                    artcl.Planeacion = planeacion;
                    _context.Articuloscamposlibres.Update(artcl);
                    await _context.SaveChangesAsync();
                }
                return StatusCode(StatusCodes.Status200OK);

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.ToString() });
            }


        }

        [HttpDelete]
        [Route("EliminarMedidaUds/{rfc}/{id}")]
        public async Task<ActionResult> deleteumedida(string rfc,string id)
        {
            try
            {
                var umedida = _context.ItProductos.Where(x => x.Rfc == rfc && x.NoIdentificacion == id).FirstOrDefault();
                _context.ItProductos.Remove(umedida);
                await _context.SaveChangesAsync();
                return StatusCode(StatusCodes.Status200OK, umedida);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.ToString() });
            }


        }
    }

    public class ProductoConsulta
    {
        public string nomProveedor { get; set; }
        public string Rfc { get; set; }
        public string NoIdentificacion { get; set; }
        public string Codarticulo { get; set; }
        public string Umedida { get; set; }
        public string Uds { get; set; }
        public string Descripcion { get; set; }
        public string Refproveedor { get; set; }
        public string Medidareferencia { get; set; }
        public string Unidadmedida { get; set; }
        public Boolean planeacion { get; set; }

        public string p_uds { get; set; }
        public string p_umedida {  get; set; }
        public string i_uds { get; set; }
        public string i_umedida { get; set; }
    }
}
