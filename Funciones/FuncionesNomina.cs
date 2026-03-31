using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TICKETSAPI.Controllers;
using TICKETSAPI.ModelsTickets;

namespace TICKETSAPI.Funciones
{
    public class FuncionesNomina
    {
        private readonly ILogger<CalendarioNominaController> _logger;
        protected TicketsContext _tdbContext;

        public FuncionesNomina(ILogger<CalendarioNominaController> logger, TicketsContext tkc)
        {
            _logger = logger;
            _tdbContext = tkc;
        }

        public async Task<int> obtenerIdPuesto(int idEmpleado)
        {
            int idPuesto = 0;
            SqlConnection connection = null;
            SqlCommand command = null;
            SqlDataReader reader = null;

            try
            {
                connection = new SqlConnection(_tdbContext.Database.GetConnectionString());
                connection.Open();
                command = new SqlCommand("SP_GET_IDPUESTO", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@CLA_TRAB", idEmpleado);

                reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    idPuesto = reader.GetInt32(reader.GetOrdinal("CLA_PUESTO"));
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al ejecutar el procedimiento almacenado: " + ex.Message);
            }
            return idPuesto; 
        }

        public async Task<List<EmpleadoHorarioResponse>> obtenerHorariosSuc(int idSuc,DateTime fecha) 
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

                return empleadosHorarios;
            }
            catch (Exception ex)
            {
                return empleadosHorarios; 
            }
        }

        public async Task<List<MarcajesResponse>> obtenerMarcajes(int idUbicacion,DateTime fechaIni,DateTime fechaFin)
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
                string connectionstring = _tdbContext.Database.GetConnectionString();
                using (SqlConnection connection = new SqlConnection(connectionstring))
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

                                    int idp = await obtenerIdPuesto(int.Parse(reader["CLA_TRAB"].ToString()));
                                    row.idpuesto = idp;
                                    row.cla_trab = int.Parse(reader["CLA_TRAB"].ToString());
                                    row.nombre = reader["NOMBRE"].ToString();
                                    row.turno = reader["TURNO1"].ToString();
                                    row.entrada = reader["ENTRADA1"].ToString();
                                    row.salida = reader["SALIDA1"].ToString();
                                    row.incidencia = reader["INCIDENCIA1"].ToString();
                                    row.fecha = fechaIni.ToString("dd/MM/yyyy");
                                    var reg = _tdbContext.BitacoraPersonals.Where(x => x.Idemp == row.cla_trab && x.Fecha.Value.Date == fechaIni.Date && x.Idsucursal == idUbicacion).FirstOrDefault();
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

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return new List<MarcajesResponse>(); ; 
            }
        }
    }
}
