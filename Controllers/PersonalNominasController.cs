using Dapper;
using DashboardApi.Mail;
using DashboardApi.ModelsBD2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;
using System.Xml.Linq;
using TICKETSAPI.Funciones;
using TICKETSAPI.ModelsTickets;

namespace TICKETSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonalNominasController : ControllerBase
    {
        private readonly ILogger<CalendarioNominaController> _logger;
        protected TicketsContext _tdbContext;
        public FuncionesNomina _fnn;
        public MailC _mail;
        protected BD2Context _contextdb2;
        public PersonalNominasController(ILogger<CalendarioNominaController> logger, TicketsContext tkc,FuncionesNomina fnn, MailC mail,BD2Context contextdb2)
        {
            _logger = logger;
            _tdbContext = tkc;
            _fnn = fnn;
            _mail = mail;
            _contextdb2 = contextdb2;
        }

        [HttpPost]
        [Route("getPersonalNominas")]
        public async Task<ActionResult> GetPersonalnominas([FromForm] int idUbicacion, [FromForm] DateTime fechaIni, [FromForm] DateTime fechaFin)
        {
            MarcajesRequest request = new MarcajesRequest();


            request.npEmpresa = 1; 
            request.npUbicacion = idUbicacion;
            request.npDepto = 0;
            request.npPeriodo = 0; 
            request.npArea = 0;
            request.npRoll = 0;
            request.npTrabajador = 0;

            string mes = fechaIni.Month < 10 ? "0" + fechaIni.Month : fechaIni.Month.ToString();
            string dia = fechaIni.Day < 10 ? "0" + fechaIni.Day : fechaIni.Day.ToString();
            request.Fecha_ini = "" + fechaIni.Year + mes + dia;

            mes = fechaFin.Month < 10 ? "0" + fechaFin.Month : fechaFin.Month.ToString();
            dia = fechaFin.Day < 10 ? "0" + fechaFin.Day : fechaFin.Day.ToString();
            request.Fecha_Fin = "" + fechaFin.Year + mes + dia;
            request.npUsuario = 1;

            List<MarcajesResponse> results = new List<MarcajesResponse>();

            try
            {
                using (SqlConnection connection = (SqlConnection)_tdbContext.Database.GetDbConnection())
                {
                    await connection.OpenAsync();

                    // Crear comando para ejecutar el procedimiento almacenado
                    using (SqlCommand command = new SqlCommand("[FORTIA_PRIME].[dbo].[GOPERA_ListadoMarcajesIncReloj]", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        try
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@npEmpresa", request.npEmpresa);
                            command.Parameters.AddWithValue("@npUbicacion", request.npUbicacion);
                            command.Parameters.AddWithValue("@npDepto", request.npDepto);
                            command.Parameters.AddWithValue("@npPeriodo", request.npPeriodo);
                            command.Parameters.AddWithValue("@npArea", request.npArea);
                            command.Parameters.AddWithValue("@npRoll", request.npRoll);
                            command.Parameters.AddWithValue("@npTrabajador", request.npTrabajador);
                            command.Parameters.AddWithValue("@Fecha_ini", request.Fecha_ini);
                            command.Parameters.AddWithValue("@Fecha_Fin", request.Fecha_Fin);
                            command.Parameters.AddWithValue("@npUsuario", request.npUsuario);


                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    MarcajesResponse row = new MarcajesResponse();

                                    int idp = await _fnn.obtenerIdPuesto(int.Parse(reader["CLA_TRAB"].ToString()));
                                    row.idpuesto = idp;
                                    row.cla_trab = int.Parse(reader["CLA_TRAB"].ToString());
                                    row.nombre = reader["NOMBRE"].ToString();
                                    row.turno = reader["TURNO1"].ToString();
                                    row.entrada = reader["ENTRADA1"].ToString();
                                    row.salida = reader["SALIDA1"].ToString();
                                    row.incidencia = reader["INCIDENCIA1"].ToString();

                                    var reg = _tdbContext.BitacoraPersonals.Where(x => x.Idemp == row.cla_trab && x.Fecha.Value.Date == DateTime.Now.Date).FirstOrDefault();
                                    row.regbitacora = reg; 
                                    results.Add(row);
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error al ejecutar el procedimiento almacenado: " + ex.Message);
                        }
                    }

                    connection.Close(); 
                }

                return StatusCode(200,results);
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

        [HttpPost("gethorariosSuc")]
        public async Task<IActionResult> GetHorariosEmpleados([FromForm]int idSuc,[FromForm]DateTime fecha)
        {
            var empleadosHorarios = new List<EmpleadoHorarioResponse>();
            string connectionString = _tdbContext.Database.GetConnectionString();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("SP_EMPLEADOS_RELOJ_HORARIO", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ID_UBICACION", idSuc);
                        command.Parameters.AddWithValue("@FECHA", fecha.ToString("dd/MM/yyyy"));

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var item = new EmpleadoHorarioResponse
                                {
                                    cla_trab = int.Parse(reader["CLA_TRAB"].ToString()),
                                    nom_trab = reader["NOM_TRAB"].ToString(),
                                    ap_paterno = reader["AP_PATERNO"].ToString(),
                                    ap_materno = reader["AP_MATERNO"].ToString(),
                                    cla_puesto = int.Parse(reader["CLA_PUESTO"].ToString()),
                                    nom_puesto = reader["NOM_PUESTO"].ToString(),
                                    cla_turno = int.Parse(reader["CLA_TURNO"].ToString()),
                                    entrada = reader["HORA_ENT1"].ToString(),
                                    salida = reader["HORA_SAL1"].ToString()
                                };

                                empleadosHorarios.Add(item);
                            }
                        }
                    }
                }

                return Ok(empleadosHorarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }


        [HttpPost("correoNotificacion")]
        public async Task<IActionResult> correoNotificacion([FromForm] int idSuc, [FromForm] string nombreRegional,[FromForm] string correo, [FromForm]string jdata)
        {
            try 
            { 
                var sucursal = _contextdb2.RemFronts.Where(x => x.Idfront == idSuc).FirstOrDefault(); 
                List<PersonalFaltante> data = JsonSerializer.Deserialize<List<PersonalFaltante>>(jdata);
                _mail.EnviarCorreo(correo,_mail.generarMailBodyPersonalNomina(data,sucursal.Titulo,nombreRegional), "PERSONAL INCOMPLETO: "+sucursal.Titulo);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }


        [HttpPost("registrarSolucion")]
        public async Task<IActionResult> registrarsolucion([FromForm] int idSuc, [FromForm] int idemp, [FromForm] string solucion)
        {
            try
            {
                _tdbContext.BitacoraPersonals.Add(new BitacoraPersonal()
                {
                    Idsucursal = idSuc,
                    Idemp = idemp,
                    Solucion = solucion,
                    Comentariosucursal = "",
                    Status = false,
                    Fecha = DateTime.Now,
                });

                 await _tdbContext.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {  
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("registrarComentarioSuc")]
        public async Task<IActionResult> registrarcomentarioSuc([FromForm] int idReg,[FromForm] string comentario)
        {
            try
            {
                var reg = _tdbContext.BitacoraPersonals.Where(x => x.Id == idReg).FirstOrDefault(); 
                if (reg != null) 
                {
                    reg.Comentariosucursal = comentario;
                    _tdbContext.BitacoraPersonals.Update(reg);
                    await _tdbContext.SaveChangesAsync();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }


        [HttpGet]
        [Route("confirmarSolucion/{idreg}")]
        public async Task<IActionResult> confirmarSolucion(int idreg)
        {
            try
            {
                var reg = _tdbContext.BitacoraPersonals.Where(x => x.Id == idreg).FirstOrDefault();
                if (reg != null)
                {
                    reg.Status = true;
                    _tdbContext.BitacoraPersonals.Update(reg);
                    await _tdbContext.SaveChangesAsync();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }


        [HttpPost("HistorialPersonal")]
        public async Task<IActionResult> HistorialPersonal([FromForm] DateTime fechaini, [FromForm] DateTime fechafin, [FromForm] string jdatasuc)
        {  
            List<Object> data = new List<Object>();
            List<Object> dataDia = new List<Object>();
            try
            {
                DateTime fechainiO = fechaini;
               int[] sucursales = JsonSerializer.Deserialize<int[]>(jdatasuc);

                foreach (int idsuc in sucursales)
                {
                   
                   
                    fechaini = fechainiO; 
                    while (fechaini.Date <= fechafin.Date)
                    {
                        int empleadosRequeridos = 0;
                        List<MarcajesResponse> incidenciasFinales = new List<MarcajesResponse>();
                        var horarios = await _fnn.obtenerHorariosSuc(idsuc, fechaini);
                        empleadosRequeridos = empleadosRequeridos + horarios.Where(x => x.cla_turno > 0).ToList().Count; 
                        var marcajes = await _fnn.obtenerMarcajes(idsuc, fechaini, fechaini);
                        var incidencias = marcajes.Where(x => (x.regbitacora != null || x.entrada == "" || x.salida == "" || x.incidencia.ToLower().Contains("injustificada")) && !x.incidencia.ToLower().Contains("ficticia")).ToList();
                        var asistencias = marcajes.Where(x => x.regbitacora == null && x.entrada != "" && x.salida != "" && !x.incidencia.ToLower().Contains("injustificada")).ToList();
                        var fficticias = marcajes.Where(x => x.incidencia.ToLower().Contains("ficticia")).ToList();
                        foreach (var item in incidencias) 
                        {
                            if (item.regbitacora != null)
                            {
                                incidenciasFinales.Add(item);
                            }
                            else 
                            {
                                var turnoemp = horarios.Where(x => x.cla_trab == item.cla_trab).FirstOrDefault();
                                if (turnoemp != null) 
                                {
                                    if (turnoemp.cla_turno > 0) 
                                    {
                                        incidenciasFinales.Add(item);
                                    }
                                }
                            }
                        }
                       
                        dataDia.Add(new { incidencias = incidenciasFinales, empleadosrequeridos = empleadosRequeridos, fecha = fechaini.ToString("dd/MM/yyyy"), asistencias = asistencias, todo = marcajes, ficticias = fficticias });

                        fechaini = fechaini.AddDays(1); 
                    }

                    data.Add(new { claubicacion = idsuc, data = dataDia});
                    dataDia = new List<Object>();
                }
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }


        [HttpPost("getChecadasManuales")]
        public async Task<IActionResult> getChecadasManuales([FromForm] DateTime fechaini, [FromForm] DateTime fechafin)
        {
            try
            {
                List<ChecadaManual> data = new List<ChecadaManual>();
                // Cadena de conexión a tu base de datos FORTIA_PRIME
                string connectionString = _tdbContext.Database.GetConnectionString(); 


                using (var db = new SqlConnection(connectionString))
                {
                    // Definir los parámetros del procedimiento
                    var parametros = new
                    {
                        FI = fechaini.Date,
                        FF = fechafin.Date
                    };

                    // Ejecutar el procedimiento almacenado
                    var resultado = db.Query<ChecadaManual>(
                        "[dbo].[GET_NOMINA_CHECADAS_MANUALES]",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                    data = resultado.ToList();
                }
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("getTurnosLargos")]
        public async Task<IActionResult> getTurnosLargos([FromForm] DateTime fechaini, [FromForm] DateTime fechafin)
        {
            try
            {
                List<turnoLargo> data = new List<turnoLargo>();
                // Cadena de conexión a tu base de datos FORTIA_PRIME
                string connectionString = _tdbContext.Database.GetConnectionString();


                using (var db = new SqlConnection(connectionString))
                {
                    // Definir los parámetros del procedimiento
                    var parametros = new
                    {
                        FI = fechaini.Date,
                        FF = fechafin.Date
                    };

                    // Ejecutar el procedimiento almacenado
                    var resultado = db.Query<turnoLargo>(
                        "[dbo].[GET_NOMINA_TURNOS_LARGOS]",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );

                    data = resultado.ToList();
                }
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }


    }

    public class MarcajesRequest
    {
        public int npEmpresa { get; set; }
        public int npUbicacion { get; set; }
        public int npDepto { get; set; }
        public int npPeriodo { get; set; }
        public int npArea { get; set; }
        public int npRoll { get; set; }
        public int npTrabajador { get; set; }
        public string Fecha_ini { get; set; }
        public string Fecha_Fin { get; set; }
        public int npUsuario { get; set; }
    }

    public class MarcajesResponse 
    {
        public int cla_trab { get; set; }
        public string nombre { get; set; }
        public int idpuesto { get; set; }
        public string turno { get; set; }
        public string entrada { get; set; }
        public string salida { get; set;}
        public string incidencia { get; set; }  
        public BitacoraPersonal regbitacora { get; set; }
        public string? fecha { get; set; }
    }

    public class EmpleadoHorarioResponse
    {
        public int cla_trab { get; set; }
        public string nom_trab { get; set; }
        public string ap_paterno { get; set; }
        public string ap_materno { get; set; }
        public int cla_puesto { get; set; }
        public string nom_puesto { get; set; }
        public int cla_turno { get; set; }
        public string entrada { get; set; }
        public string salida { get; set; }

    }

    public class PersonalFaltante
    {
        public string nombrepuesto { get; set; }
        public int empleadosRequeridos { get; set; }
        public int empleadosFaltantes { get; set; } 
    }

    public class ChecadaManual
    {
        public int CLA_UBICACION { get; set; }
        public string NOM_UBICACION { get; set; }
        public DateTime FECHA { get; set; }
        public int CLA_TRAB { get; set; }
        public string NOMBRE { get; set; }
        public string STATUS_TRAB { get; set; }
        public int ENTRADA { get; set; }   
        public int SALIDA { get; set; } 
    }

    public class turnoLargo
    {
        public int CLA_UBICACION { get; set; }
        public string NOM_UBICACION { get; set; }
        public DateTime FECHA { get; set; }
        public int CLA_TRAB { get; set; }
        public string NOMBRE { get; set; }
        public string STATUS_TRAB { get; set; }
    }

}
