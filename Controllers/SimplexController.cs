using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace TICKETSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SimplexController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public SimplexController(IConfiguration configuration) 
        {
            _configuration = configuration;
        }

        [HttpPost("ObtenerDatosSimplex")]
        public async Task<IActionResult> PostInformacionSimplex([FromForm] InformacionSimplexRequest request)
        {
            int HI, HF;
            if (request == null)
                return BadRequest("Request cannot be null");

            // Validaciones básicas
            if (request.Fi > request.Ff)
                return BadRequest("Fecha inicio no puede ser mayor que fecha fin");

            

            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                using var connection = new SqlConnection(connectionString);


                List<InformacionSimplexResponse> data = new List<InformacionSimplexResponse>();  
                DateTime hoy = DateTime.Now;    
                DateTime temp = request.Fi;

                while(temp.Date <= request.Ff.Date )
                {

                    if (temp.Date < hoy.Date)
                    {
                        HI = 0; HF = 14;
                        var parameters = new DynamicParameters();
                        parameters.Add("@IDS", request.Ids, DbType.Int32);
                        parameters.Add("@FI", temp, DbType.DateTime);
                        parameters.Add("@FF", temp, DbType.DateTime);
                        parameters.Add("@HI", HI, DbType.Int32);
                        parameters.Add("@HF", HF, DbType.Int32);

                        // Ejecutar el SP y mapear a la lista de respuestas
                        var result = await connection.QueryAsync<InformacionSimplexResponse>(
                            "dbo.INFORMACION_SIMPLEX",
                            parameters,
                            commandType: CommandType.StoredProcedure
                        );

                        HI = 14; HF = 17;
                        var parameters2 = new DynamicParameters();
                        parameters.Add("@IDS", request.Ids, DbType.Int32);
                        parameters.Add("@FI", temp, DbType.DateTime);
                        parameters.Add("@FF", temp, DbType.DateTime);
                        parameters.Add("@HI", HI, DbType.Int32);
                        parameters.Add("@HF", HF, DbType.Int32);

                        // Ejecutar el SP y mapear a la lista de respuestas
                        var result2 = await connection.QueryAsync<InformacionSimplexResponse>(
                            "dbo.INFORMACION_SIMPLEX",
                            parameters,
                            commandType: CommandType.StoredProcedure
                        );

                        HI = 17; HF = 18;
                        var parameters3 = new DynamicParameters();
                        parameters.Add("@IDS", request.Ids, DbType.Int32);
                        parameters.Add("@FI", temp, DbType.DateTime);
                        parameters.Add("@FF", temp, DbType.DateTime);
                        parameters.Add("@HI", HI, DbType.Int32);
                        parameters.Add("@HF", HF, DbType.Int32);

                        // Ejecutar el SP y mapear a la lista de respuestas
                        var result3 = await connection.QueryAsync<InformacionSimplexResponse>(
                            "dbo.INFORMACION_SIMPLEX",
                            parameters,
                            commandType: CommandType.StoredProcedure
                        );

                        HI = 18; HF = 22;
                        var parameters4 = new DynamicParameters();
                        parameters.Add("@IDS", request.Ids, DbType.Int32);
                        parameters.Add("@FI", temp, DbType.DateTime);
                        parameters.Add("@FF", temp, DbType.DateTime);
                        parameters.Add("@HI", HI, DbType.Int32);
                        parameters.Add("@HF", HF, DbType.Int32);

                        // Ejecutar el SP y mapear a la lista de respuestas
                        var result4 = await connection.QueryAsync<InformacionSimplexResponse>(
                            "dbo.INFORMACION_SIMPLEX",
                            parameters,
                            commandType: CommandType.StoredProcedure
                        );

                        HI = 22; HF = 24;
                        var parameters5 = new DynamicParameters();
                        parameters.Add("@IDS", request.Ids, DbType.Int32);
                        parameters.Add("@FI", temp, DbType.DateTime);
                        parameters.Add("@FF", temp, DbType.DateTime);
                        parameters.Add("@HI", HI, DbType.Int32);
                        parameters.Add("@HF", HF, DbType.Int32);

                        // Ejecutar el SP y mapear a la lista de respuestas
                        var result5 = await connection.QueryAsync<InformacionSimplexResponse>(
                            "dbo.INFORMACION_SIMPLEX",
                            parameters,
                            commandType: CommandType.StoredProcedure
                        );

                        data.Add(result.FirstOrDefault());
                        data.Add(result2.FirstOrDefault());
                        data.Add(result3.FirstOrDefault());
                        data.Add(result4.FirstOrDefault());
                        data.Add(result5.FirstOrDefault());
                    }
                    else 
                    {   
                        List<InformacionSimplexResponse> tempdata = new List<InformacionSimplexResponse> ();
                        DateTime fechatemp2 = temp;
                        while (fechatemp2.Date > hoy.Date)
                        {
                            fechatemp2 = fechatemp2.AddDays(-7);
                        }
                        for (int i = 0; i < 4; i++) 
                        {
                            
                            HI = 0; HF = 14;
                            var parameters = new DynamicParameters();
                            parameters.Add("@IDS", request.Ids, DbType.Int32);
                            parameters.Add("@FI", fechatemp2, DbType.DateTime);
                            parameters.Add("@FF", fechatemp2, DbType.DateTime);
                            parameters.Add("@HI", HI, DbType.Int32);
                            parameters.Add("@HF", HF, DbType.Int32);

                            // Ejecutar el SP y mapear a la lista de respuestas
                            var result = await connection.QueryAsync<InformacionSimplexResponse>(
                                "dbo.INFORMACION_SIMPLEX",
                                parameters,
                                commandType: CommandType.StoredProcedure
                            );
                            tempdata.Add(result.FirstOrDefault());
                            fechatemp2 = fechatemp2.AddDays(-7);
                        }
                        tempdata = tempdata.Where(x => x != null).ToList();
                        data.Add(new InformacionSimplexResponse()
                        {
                            Titulo = tempdata.FirstOrDefault().Titulo,
                            Venta = tempdata.Average(x => x.Venta),
                            Mesas = (int?)tempdata.Average(x => x.Mesas),
                            Comensales = (int?)tempdata.Average(x => x.Comensales),
                            Alimentos = tempdata.Average(x => x.Alimentos),
                            Bebidas = tempdata.Average(x => x.Bebidas),
                            Otros = tempdata.Average(x => x.Otros),
                            Productos = tempdata.Average(x => x.Productos),
                            Salon = tempdata.Average(x => x.Salon)
                        }); 
                        
                        tempdata = new List<InformacionSimplexResponse>();

                        //////////////////////////////////////////////////////////////////////////

                        fechatemp2 = temp;
                        while (fechatemp2.Date > hoy.Date)
                        {
                            fechatemp2 = fechatemp2.AddDays(-7);
                        }
                        for (int i = 0; i < 4; i++)
                        {                            
                            HI = 14; HF = 17;
                            var parameters = new DynamicParameters();
                            parameters.Add("@IDS", request.Ids, DbType.Int32);
                            parameters.Add("@FI", fechatemp2, DbType.DateTime);
                            parameters.Add("@FF", fechatemp2, DbType.DateTime);
                            parameters.Add("@HI", HI, DbType.Int32);
                            parameters.Add("@HF", HF, DbType.Int32);

                            // Ejecutar el SP y mapear a la lista de respuestas
                            var result = await connection.QueryAsync<InformacionSimplexResponse>(
                                "dbo.INFORMACION_SIMPLEX",
                                parameters,
                                commandType: CommandType.StoredProcedure
                            );
                            tempdata.Add(result.FirstOrDefault());
                            fechatemp2 = fechatemp2.AddDays(-7);
                        }
                        tempdata = tempdata.Where(x => x != null).ToList();
                        data.Add(new InformacionSimplexResponse()
                        {
                            Titulo = tempdata.FirstOrDefault().Titulo,
                            Venta = tempdata.Average(x => x.Venta),
                            Mesas = (int?)tempdata.Average(x => x.Mesas),
                            Comensales = (int?)tempdata.Average(x => x.Comensales),
                            Alimentos = tempdata.Average(x => x.Alimentos),
                            Bebidas = tempdata.Average(x => x.Bebidas),
                            Otros = tempdata.Average(x => x.Otros),
                            Productos = tempdata.Average(x => x.Productos),
                            Salon = tempdata.Average(x => x.Salon)
                        });

                        tempdata = new List<InformacionSimplexResponse>();


                        //////////////////////////////////////////////////////////////////////////

                        fechatemp2 = temp;
                        while (fechatemp2.Date > hoy.Date)
                        {
                            fechatemp2 = fechatemp2.AddDays(-7);
                        }
                        for (int i = 0; i < 4; i++)
                        {
                            HI = 14; HF = 18;
                            var parameters = new DynamicParameters();
                            parameters.Add("@IDS", request.Ids, DbType.Int32);
                            parameters.Add("@FI", fechatemp2, DbType.DateTime);
                            parameters.Add("@FF", fechatemp2, DbType.DateTime);
                            parameters.Add("@HI", HI, DbType.Int32);
                            parameters.Add("@HF", HF, DbType.Int32);

                            // Ejecutar el SP y mapear a la lista de respuestas
                            var result = await connection.QueryAsync<InformacionSimplexResponse>(
                                "dbo.INFORMACION_SIMPLEX",
                                parameters,
                                commandType: CommandType.StoredProcedure
                            );
                            tempdata.Add(result.FirstOrDefault());
                            fechatemp2 = fechatemp2.AddDays(-7);
                        }
                        tempdata = tempdata.Where(x => x != null).ToList();
                        data.Add(new InformacionSimplexResponse()
                        {
                            Titulo = tempdata.FirstOrDefault().Titulo,
                            Venta = tempdata.Average(x => x.Venta),
                            Mesas = (int?)tempdata.Average(x => x.Mesas),
                            Comensales = (int?)tempdata.Average(x => x.Comensales),
                            Alimentos = tempdata.Average(x => x.Alimentos),
                            Bebidas = tempdata.Average(x => x.Bebidas),
                            Otros = tempdata.Average(x => x.Otros),
                            Productos = tempdata.Average(x => x.Productos),
                            Salon = tempdata.Average(x => x.Salon)
                        });

                        tempdata = new List<InformacionSimplexResponse>();


                        //////////////////////////////////////////////////////////////////////////

                        fechatemp2 = temp;
                        while (fechatemp2.Date > hoy.Date)
                        {
                            fechatemp2 = fechatemp2.AddDays(-7);
                        }
                        for (int i = 0; i < 4; i++)
                        {
                            HI = 18; HF = 22;
                            var parameters = new DynamicParameters();
                            parameters.Add("@IDS", request.Ids, DbType.Int32);
                            parameters.Add("@FI", fechatemp2, DbType.DateTime);
                            parameters.Add("@FF", fechatemp2, DbType.DateTime);
                            parameters.Add("@HI", HI, DbType.Int32);
                            parameters.Add("@HF", HF, DbType.Int32);

                            // Ejecutar el SP y mapear a la lista de respuestas
                            var result = await connection.QueryAsync<InformacionSimplexResponse>(
                                "dbo.INFORMACION_SIMPLEX",
                                parameters,
                                commandType: CommandType.StoredProcedure
                            );
                            tempdata.Add(result.FirstOrDefault());
                            fechatemp2 = fechatemp2.AddDays(-7);
                        }
                        tempdata = tempdata.Where(x => x != null).ToList();
                        data.Add(new InformacionSimplexResponse()
                        {
                            Titulo = tempdata.FirstOrDefault().Titulo,
                            Venta = tempdata.Average(x => x.Venta),
                            Mesas = (int?)tempdata.Average(x => x.Mesas),
                            Comensales = (int?)tempdata.Average(x => x.Comensales),
                            Alimentos = tempdata.Average(x => x.Alimentos),
                            Bebidas = tempdata.Average(x => x.Bebidas),
                            Otros = tempdata.Average(x => x.Otros),
                            Productos = tempdata.Average(x => x.Productos),
                            Salon = tempdata.Average(x => x.Salon)
                        });

                        tempdata = new List<InformacionSimplexResponse>();



                        //////////////////////////////////////////////////////////////////////////

                        fechatemp2 = temp;
                        while (fechatemp2.Date > hoy.Date)
                        {
                            fechatemp2 = fechatemp2.AddDays(-7);
                        }
                        for (int i = 0; i < 4; i++)
                        {
                            HI = 22; HF = 24;
                            var parameters = new DynamicParameters();
                            parameters.Add("@IDS", request.Ids, DbType.Int32);
                            parameters.Add("@FI", fechatemp2, DbType.DateTime);
                            parameters.Add("@FF", fechatemp2, DbType.DateTime);
                            parameters.Add("@HI", HI, DbType.Int32);
                            parameters.Add("@HF", HF, DbType.Int32);

                            // Ejecutar el SP y mapear a la lista de respuestas
                            var result = await connection.QueryAsync<InformacionSimplexResponse>(
                                "dbo.INFORMACION_SIMPLEX",
                                parameters,
                                commandType: CommandType.StoredProcedure
                            );
                            tempdata.Add(result.FirstOrDefault());
                            fechatemp2 = fechatemp2.AddDays(-7);
                        }
                        tempdata = tempdata.Where(x=> x!= null).ToList();
                        data.Add(new InformacionSimplexResponse()
                        {
                            Titulo = tempdata.FirstOrDefault().Titulo,
                            Venta = tempdata.Average(x => x.Venta),
                            Mesas = (int?)tempdata.Average(x => x.Mesas),
                            Comensales = (int?)tempdata.Average(x => x.Comensales),
                            Alimentos = tempdata.Average(x => x.Alimentos),
                            Bebidas = tempdata.Average(x => x.Bebidas),
                            Otros = tempdata.Average(x => x.Otros),
                            Productos = tempdata.Average(x => x.Productos),
                            Salon = tempdata.Average(x => x.Salon)
                        });

                        tempdata = new List<InformacionSimplexResponse>();
                    }

                    temp = temp.AddDays( 1 );
                }

                return Ok(data);
            }
            catch (SqlException ex)
            {
                // Manejo de errores de base de datos
                return StatusCode(500, new { error = "Database error", details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }
    }

    public class InformacionSimplexRequest
    {
        [FromForm(Name = "ids")]
        public int Ids { get; set; }

        [FromForm(Name = "fi")]
        public DateTime Fi { get; set; }

        [FromForm(Name = "ff")]
        public DateTime Ff { get; set; }

    }

    public class InformacionSimplexResponse
    {
        public string Titulo { get; set; }
        public decimal? Venta { get; set; }
        public int? Mesas { get; set; }
        public int? Comensales { get; set; }
        public decimal? Alimentos { get; set; }
        public decimal? Bebidas { get; set; }
        public decimal? Otros { get; set; }
        public decimal? Productos { get; set; }
        public decimal? Salon { get; set; }
    }


}
