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
    public class CalendarioNominaController : ControllerBase
    {

        private readonly ILogger<CalendarioNominaController> _logger;
        protected TicketsContext _tdbContext;

        public CalendarioNominaController(ILogger<CalendarioNominaController> logger,TicketsContext tkc)
        {
            _logger = logger;
            _tdbContext = tkc;
        }

        [HttpGet]
        [Route("getDepartamentos")]
        public async Task<ActionResult> GetDepartamentos()
        {
            try
            {
                List<object> departamentos = new List<Object>();
                // Crear conexión
                using (SqlConnection connection = (SqlConnection)_tdbContext.Database.GetDbConnection())
                {
                    connection.Open();

                    // Crear comando para ejecutar el procedimiento almacenado
                    using (SqlCommand command = new SqlCommand("SP_GET_DEPARTAMENTOS", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        try
                        {
                            // Ejecutar el procedimiento almacenado
                            SqlDataReader reader = command.ExecuteReader();

                            while (reader.Read())
                            {
                                departamentos.Add(new
                                {
                                    idpuesto = int.Parse(reader["CLA_PUESTO"].ToString()),
                                    nombre = reader["NOM_PUESTO"].ToString()
                                });
                            }

                            reader.Close();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error al ejecutar el procedimiento almacenado: " + ex.Message);
                        }
                    }
                }

                return StatusCode(200, departamentos);
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
        [Route("getUbicaciones")]
        public async Task<ActionResult> GetUbicaciones()
        {
            try
            {
                List<UbicacionModel> ubicaciones = new List<UbicacionModel>();
                // Crear conexión
                using (SqlConnection connection = (SqlConnection)_tdbContext.Database.GetDbConnection())
                {
                    connection.Open();

                    // Crear comando para ejecutar el procedimiento almacenado
                    using (SqlCommand command = new SqlCommand("SP_GET_UBICACIONES", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        try
                        {
                            // Ejecutar el procedimiento almacenado
                            SqlDataReader reader = command.ExecuteReader();

                            while (reader.Read())
                            {
                                ubicaciones.Add(new UbicacionModel
                                {
                                    idUbicacion = int.Parse(reader["CLA_UBICACION"].ToString()),
                                    idEmpresa = int.Parse(reader["CLA_EMPRESA"].ToString()),
                                    nombre = reader["NOM_UBICACION"].ToString()
                                });
                            }

                            reader.Close();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error al ejecutar el procedimiento almacenado: " + ex.Message);
                        }
                    }
                }

                return StatusCode(200, ubicaciones);
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
        [Route("getTurnos")]
        public async Task<ActionResult> GetTurnos()
        {
            try
            {
                List<TurnoModel> turnos = new List<TurnoModel>();
                // Crear conexión
                using (SqlConnection connection = (SqlConnection)_tdbContext.Database.GetDbConnection())
                {
                    connection.Open();

                    // Crear comando para ejecutar el procedimiento almacenado
                    using (SqlCommand command = new SqlCommand("SP_GET_TURNOS", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        try
                        {
                            // Ejecutar el procedimiento almacenado
                            SqlDataReader reader = command.ExecuteReader();

                            while (reader.Read())
                            {
                                turnos.Add(new TurnoModel
                                {
                                    idTurno = int.Parse(reader["CLA_TURNO"].ToString()),
                                    idEmpresa = int.Parse(reader["CLA_EMPRESA"].ToString()),
                                    nombre = reader["NOM_TURNO"].ToString()
                                });
                            }

                            reader.Close();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error al ejecutar el procedimiento almacenado: " + ex.Message);
                        }
                    }
                }

                return StatusCode(200, turnos);
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
        [Route("getTurnosdb")]
        public async Task<ActionResult> GetTurnosdb()
        {
            try
            {
                List<TurnodbModel> turnos = new List<TurnodbModel>();
                var turnosdb = _tdbContext.CatTurnos.ToList();

                foreach (var item in turnosdb)
                {
                    turnos.Add(new TurnodbModel() 
                    {
                        idTurno = item.ClaTurno,
                        idEmpresa = (int)item.ClaEmpresa,
                        nombre = item.Nombre,
                        alias = item.Alias
                    });
                }
                return StatusCode(200, turnos);
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


        [HttpPost]
        [Route("guardarTurnodb")]
        public async Task<ActionResult> guardarturnodb([FromForm] int cla_turno, [FromForm] string nombre, [FromForm] string alias)
        {
            try
            {
                var reg = _tdbContext.CatTurnos.Where(x => x.ClaTurno == cla_turno).FirstOrDefault();
                if (reg == null)
                {
                    _tdbContext.CatTurnos.Add(new CatTurno()
                    {
                        ClaTurno = cla_turno,
                        ClaEmpresa = 1,
                        Nombre = nombre,
                        Alias = alias
                    });
                }
                else 
                {
                    reg.Nombre = nombre;
                    _tdbContext.CatTurnos.Update(reg); 
                }
                await _tdbContext.SaveChangesAsync();
                return StatusCode(200);
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

        [HttpDelete]
        [Route("borrarTurnodb/{idt}")]
        public async Task<ActionResult> borrarTurnosdb(int idt)
        {
            try
            {
                var reg = _tdbContext.CatTurnos.Where(x => x.ClaTurno == idt).FirstOrDefault();
                if (reg != null) 
                {
                    _tdbContext.CatTurnos.Remove(reg); 
                    await _tdbContext.SaveChangesAsync();   
                }
                return StatusCode(200);
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
        [Route("getEmpleados/{idUbicacion}")]
        public async Task<ActionResult> GetEmpleados(int idUbicacion)
        {
            try
            {
                List<EmpleadoModel> empleados = new List<EmpleadoModel>();
                // Crear conexión
                using (SqlConnection connection = (SqlConnection)_tdbContext.Database.GetDbConnection())
                {
                    connection.Open();

                    // Crear comando para ejecutar el procedimiento almacenado
                    using (SqlCommand command = new SqlCommand("SP_EMPLEADOS_NOMINA", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@ID_UBICACION", SqlDbType.Int).Value = idUbicacion;
                        try
                        {
                            // Ejecutar el procedimiento almacenado
                            SqlDataReader reader = command.ExecuteReader();

                            while (reader.Read())
                            {
                                empleados.Add(new EmpleadoModel
                                {
                                    id = int.Parse(reader["CLA_TRAB"].ToString()),
                                    nombre = reader["NOM_TRAB"].ToString() +" "+ reader["AP_PATERNO"].ToString()+" "+ reader["AP_MATERNO"].ToString(),
                                    departamento = int.Parse(reader["CLA_PUESTO"].ToString()),
                                });
                            }

                            reader.Close();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error al ejecutar el procedimiento almacenado: " + ex.Message);
                        }
                    }
                }

                return StatusCode(200, empleados);
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

        [HttpPost("guardarTurnos")]
        public async Task<ActionResult> GuardarTurnos([FromBody] List<GuardarTurnoRequest> requests)
        {
            SqlTransaction transaction = null;
            SqlConnection connection = null;

            try
            {
                connection = (SqlConnection)_tdbContext.Database.GetDbConnection();
                connection.Open();

                // Iniciar transacción
                transaction = connection.BeginTransaction();

                foreach (var request in requests)
                {
                    request.Fecha = request.Fecha.Date;
                    using (var command = new SqlCommand("SP_GUARDAR_TURNO", connection, transaction))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;

                        // Agregar parámetros
                        command.Parameters.AddWithValue("@CLA_TRAB", request.ClaTrab);
                        command.Parameters.AddWithValue("@CLA_EMPRESA", request.ClaEmpresa);
                        command.Parameters.AddWithValue("@CLA_TURNO", request.ClaTurno);
                        command.Parameters.AddWithValue("@FECHA", request.Fecha);

                        command.ExecuteNonQuery();
                    }
                }

                // Si todo va bien, hacer commit
                transaction.Commit();

                return Ok(new { success = true, message = $"Se guardaron {requests.Count} turnos correctamente" });
            }
            catch (Exception ex)
            {
                // Si hay error, hacer rollback
                transaction?.Rollback();

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al guardar los turnos. Se realizó rollback de todos los cambios.",
                    error = ex.Message
                });
            }
            finally
            {
                // Cerrar conexión
                connection?.Close();
                connection?.Dispose();
                transaction?.Dispose();
            }
        }

    }

    public class EmpleadoModel
    {
        public int id { get; set; }
        public string nombre { get; set; }
        public int departamento { get; set; }
    }

    public class UbicacionModel
    {
        public int idUbicacion { get; set; }
        public int idEmpresa { get; set; }
        public string nombre { get; set;}
    }

    public class TurnoModel
    {
        public int idTurno { get; set; }
        public int idEmpresa { get; set; }
        public string nombre { get; set; }
    }

    public class TurnodbModel
    {
        public int idTurno { get; set; }
        public int idEmpresa { get; set; }
        public string nombre { get; set; }
        public string alias { get; set; }   
    }


    public class GuardarTurnoRequest
    {
        public int ClaTrab { get; set; }
        public int ClaEmpresa { get; set; }
        public int ClaTurno { get; set; }
        public DateTime Fecha { get; set; }
    }

}
