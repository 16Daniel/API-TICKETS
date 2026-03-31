using DashboardApi.ModelsBD2;
using DashboardApi.ModelsDBRebel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace TICKETSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MermasController : ControllerBase
    {
        protected BD2Context _context;
        protected DBRebelContext _dbRebelContext; 
        private readonly IConfiguration _configuration;
        

        public MermasController(BD2Context context, IConfiguration configuration, DBRebelContext dbrebl)
        {
            _context = context;
            _configuration = configuration;
            _dbRebelContext = dbrebl;   
        }

        [HttpPost]
        [Route("getMermas")]
        public async Task<ActionResult> getMemras([FromForm] DateTime fechaini, [FromForm] DateTime fechafin, [FromForm] string jdataSuc)
        {
            try
            {
                int[] sucursales = System.Text.Json.JsonSerializer.Deserialize<int[]>(jdataSuc);

                var connectionString = _configuration.GetConnectionString("DBREBELWINGS");
                var mermas = new List<Merma>();
                List<ItemMerma> data = new List<ItemMerma>();

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    foreach (int suc in sucursales)
                    {
                        var sucursal = _context.RemFronts.Where(x => x.Idfront == suc).FirstOrDefault();
                        var sql = @" 
                        SELECT SUCURSAL,CODARTICULO,JUSTIFICACION,PRECIO,UNIDADES
                        FROM [db_rebel_wings].[dbo].[IT_MERMAS] where SUCURSAL = '"+sucursal.Titulo+ @"'
                        AND CONVERT(DATE,FECHA) >= CONVERT(DATE,'"+fechaini.ToString("yyyy-MM-dd")+ @"') AND CONVERT(date,FECHA) <= CONVERT(DATE,'"+fechafin.ToString("yyyy-MM-dd") + @"')  
                            ";

                        using (var command = new SqlCommand(sql, connection))
                        {
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {   

                                    var producto = new Merma()
                                    {
                                        sucursal = reader["SUCURSAL"].ToString(),
                                        codArticulo = int.Parse(reader["CODARTICULO"].ToString()),
                                        justificacion = reader["JUSTIFICACION"].ToString(),
                                        precio = double.Parse(reader["PRECIO"].ToString()),
                                        unidades = double.Parse(reader["UNIDADES"].ToString()),
                                        codSucursal = suc
                                    };

                                    mermas.Add(producto);
                                }
                            }
                        }
                    }
                }

                foreach (var sucursal in sucursales) 
                {
                    var datasuc = mermas.Where(x => x.codSucursal == sucursal).ToList(); 
                    List<int> distintosCodArticulo = datasuc
                    .Select(m => m.codArticulo)
                    .Distinct()
                    .ToList();

                    foreach (int codart in distintosCodArticulo) 
                    {   
                        var articulo = _context.Articulos1.Where(x=> x.Codarticulo == codart).FirstOrDefault();

                        var precio = datasuc.Where(x => x.codArticulo == codart).FirstOrDefault().precio; 

                        var mermaoperativa = datasuc
                                    .Where(m => m.codArticulo == codart && m.justificacion == "MERMA OPERATIVA")
                                    .Sum(m => m.unidades);

                        var mermaProveedor = datasuc
                                    .Where(m => m.codArticulo == codart && m.justificacion == "MERMA PROVEEDOR")
                                    .Sum(m => m.unidades);

                        data.Add(new ItemMerma() 
                        {
                            sucursal = datasuc[0].sucursal,
                            codSucursal = sucursal,
                            codArticulo = codart,
                            justificacion = "MERMA OPERATIVA",
                            unidades = mermaoperativa,
                            precio = precio,
                            totaldinero = mermaoperativa*precio,
                            articulo = articulo.Descripcion    
                        });

                        data.Add(new ItemMerma()
                        {
                            sucursal = datasuc[0].sucursal,
                            codSucursal = sucursal,
                            codArticulo = codart,
                            justificacion = "MERMA PROVEEDOR",
                            unidades = mermaProveedor,
                            precio = precio,
                            totaldinero = mermaProveedor * precio,
                            articulo = articulo.Descripcion

                        });
                    }
                }
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }

    public class Merma
    {
        public string sucursal { get; set; }
        public int codSucursal { get; set; }
        public int codArticulo { get; set; }
        public string justificacion { get; set; }
        public double unidades { get; set; }
        public double precio { get; set; }
    }

    public class ItemMerma
    {
        public string sucursal { get; set; }
        public int codSucursal { get; set; }
        public int codArticulo { get; set; }
        public string articulo { get; set; }
        public string justificacion { get; set; }
        public double unidades { get; set; }
        public double precio { get; set; }
        public double totaldinero { get; set; }
    }


}
