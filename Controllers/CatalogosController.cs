
using DashboardApi.ModelsBD1;
using DashboardApi.ModelsBD2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Data;
using System.Globalization;
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
        private readonly IConfiguration _configuration;
        public string connectionString = string.Empty;

        public CatalogosController(ILogger<CatalogosController> logger, BD2Context db2c, IConfiguration configuration) 
        {
            _logger = logger;
            _contextdb2 = db2c;
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("DB2");
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


    }

 
}
