
using DashboardApi.ModelsBD1;
using DashboardApi.ModelsBD2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Data;
using System.Drawing.Drawing2D;
using System.Globalization;
using TICKETSAPI.ModelsTickets;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DashboardApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class CatalogosController : ControllerBase
    {
        private readonly ILogger<CatalogosController> _logger;
        protected BD2Context _contextdb2;
        protected TicketsContext _tdbContext;
        private readonly IConfiguration _configuration;
        public string connectionString = string.Empty;

        public CatalogosController(ILogger<CatalogosController> logger, BD2Context db2c, IConfiguration configuration, TicketsContext tdbContext) 
        {
            _logger = logger;
            _contextdb2 = db2c;
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("DB2Connection");
            _tdbContext = tdbContext;
        }

        [HttpGet]
        [Route("getSucursales")]
        public async Task<ActionResult> GetSucursales()
        {
            try
            {
               
                string  query =@"
       SELECT RF.IDFRONT AS cod, RF.TITULO AS name
FROM ALMACEN ALM WITH(NOLOCK)
INNER JOIN REM_CAJASFRONT RCF WITH(NOLOCK) ON ALM.CODALMACEN COLLATE Latin1_General_CS_AI = RCF.CODALMVENTAS
INNER JOIN SERIESCAMPOSLIBRES SCL WITH(NOLOCK) ON RCF.SERIETIQUETS COLLATE Latin1_General_CS_AI = SCL.SERIE
INNER JOIN REM_FRONTS RF ON RF.IDFRONT = RCF.IDFRONT 
WHERE (ALM.NOTAS LIKE N'RW') AND (RCF.CAJAFRONT = 1)";

                List<Object> sucursales = new List<Object>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);

                        // Abrir la conexión
                        connection.Open();

                        // Ejecutar el comando y obtener los datos
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // Crear una tabla para almacenar los datos
                            DataTable dataTable = new DataTable();
                            dataTable.Load(reader);

                            // Imprimir los datos (para prueba)
                            foreach (DataRow row in dataTable.Rows)
                            {
                            sucursales.Add(new { cod = row[0], name = row[1] }); 
                            }
                        }

                }

                return StatusCode(200, sucursales);
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
        [Route("getSucursalesGrupo")]
        public async Task<ActionResult> GetSucursalesGrupo()
        {
            try
            {

                string query = @"
      SELECT RF.IDFRONT AS cod, RF.TITULO AS name,SC.REGION
FROM ALMACEN ALM WITH(NOLOCK)
INNER JOIN REM_CAJASFRONT RCF WITH(NOLOCK) ON ALM.CODALMACEN COLLATE Latin1_General_CS_AI = RCF.CODALMVENTAS
INNER JOIN SERIESCAMPOSLIBRES SCL WITH(NOLOCK) ON RCF.SERIETIQUETS COLLATE Latin1_General_CS_AI = SCL.SERIE
INNER JOIN REM_FRONTS RF ON RF.IDFRONT = RCF.IDFRONT 
INNER JOIN BD2.dbo.SERIESCAMPOSLIBRES SC WITH(NOLOCK) ON RCF.SERIETIQUETS COLLATE Modern_Spanish_CI_AS = SC.SERIE
WHERE (ALM.NOTAS LIKE N'RW') AND (RCF.CAJAFRONT = 1)";

                List<SucursalRegion> sucursales = new List<SucursalRegion>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);

                    // Abrir la conexión
                    connection.Open();

                    // Ejecutar el comando y obtener los datos
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Crear una tabla para almacenar los datos
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        // Imprimir los datos (para prueba)
                        foreach (DataRow row in dataTable.Rows)
                        {
                            string regionbd = row[2].ToString();
                            string region = "";
                            if (regionbd == "DF")
                            {
                                region = "CDMX";
                            }
                            else { region = regionbd; }
                            sucursales.Add(new SucursalRegion() { id = int.Parse(row[0].ToString()), name = row[1].ToString(), region = region });
                        }
                    }

                }

                var sucursalesfranquicias = _tdbContext.SucursalesFranquicias.ToList(); 
                foreach(var suc in  sucursalesfranquicias) 
                {
                    sucursales.Add(new SucursalRegion() { id = suc.Idf, name = suc.Nombre, region = suc.Grupo }); 
                }
                return StatusCode(200, sucursales);
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

    public class SucursalRegion
    {
        public int id { get; set; }
        public String name { get; set; }

        public String region { get; set; }

    }
}
