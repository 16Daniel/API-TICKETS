using Dapper;
using DashboardApi.Mail;
using DashboardApi.ModelsBD1;
using DashboardApi.ModelsBD2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Data;
using System.Globalization;
using System.Text.Json;
using TICKETSAPI.Controllers;
using TICKETSAPI.Funciones;
using TICKETSAPI.ModelsTickets;

namespace TICKETSAPI.Jobs
{
    public class JobFaltasPersonal : IJob
    {
        protected BD2Context _contextdb2;
        protected BD1Context _contextdb1;
        protected TicketsContext _tdbContext;
        public string connectionString = string.Empty;
        private readonly IConfiguration _configuration;
        private readonly ILogger<JobRemisiones> _logger;
        public FuncionesNomina _fnn;
        public MailC _mail; 

        public JobFaltasPersonal(ILogger<JobRemisiones> logger, BD2Context db2c, IConfiguration configuration, BD1Context BDC, TicketsContext tdbContext, FuncionesNomina fnn, MailC mail)
        {
            _logger = logger;
            _contextdb2 = db2c;
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("TicketsConnection");
            _contextdb1 = BDC;
            _tdbContext = tdbContext;
            _fnn = fnn;
            _mail = mail;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            List<string> conteos = await getConteosCorreo(); 
            DateTime fechaini = DateTime.Now.AddDays(-24);
            DateTime fechafin = DateTime.Now.AddDays(-4);
            List<ResumenSemanal> data = new List<ResumenSemanal>();
            List<incidenciasModel> dataDia = new List<incidenciasModel>();
            try
            {
                DateTime fechainiO = fechaini;
                List<UbicacionModel> sucursales = await getUbicaciones();
                sucursales = sucursales.Where(x => x.nombre.Contains("REBEL WINGS")).ToList();

                //sucursales = sucursales.GetRange(0, 1); 
                foreach (var suc in sucursales)
                {
                    fechaini = fechainiO;
                    while (fechaini.Date <= fechafin.Date)
                    {
                        int empleadosRequeridos = 0;
                        List<MarcajesResponse> incidenciasFinales = new List<MarcajesResponse>();
                        var horarios = await _fnn.obtenerHorariosSuc(suc.idUbicacion, fechaini);
                        empleadosRequeridos = empleadosRequeridos + horarios.Where(x => x.cla_turno > 0).ToList().Count;
                        var marcajes = await _fnn.obtenerMarcajes(suc.idUbicacion, fechaini, fechaini);
                        var incidencias = marcajes.Where(x => (x.regbitacora != null || x.entrada == "" || x.salida == "" || x.incidencia.ToLower().Contains("injustificada")) && !x.incidencia.ToLower().Contains("ficticia")).ToList();
                        var asistencias = marcajes.Where(x => x.regbitacora == null && x.entrada != "" && x.salida != "" && !x.incidencia.ToLower().Contains("injustificada")).ToList();
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
                        incidenciasFinales = incidenciasFinales.Where(x => !x.incidencia.ToLower().Contains("vacaciones") && !x.incidencia.ToLower().Contains("incapacidad") && !x.incidencia.Contains("ficticia")).ToList();
                        dataDia.Add(new incidenciasModel(){ idUbicacion = suc.idUbicacion, nombreUbicacion = suc.nombre, faltas = incidenciasFinales.Count, empleadoRequeridos = empleadosRequeridos, fecha = fechaini });

                        fechaini = fechaini.AddDays(1);
                    }
                }

                data = dataDia.GroupBy(i => new
                {
                    i.idUbicacion,
                    i.nombreUbicacion,
                    Año = ISOWeek.GetYear(i.fecha),
                    NumeroSemana = GetWeekNumber(i.fecha)
                })
                                       .Select(g => new ResumenSemanal()
                                       {
                                           idUbicacion = g.Key.idUbicacion,
                                           nombreUbicacion = g.Key.nombreUbicacion,
                                           año = g.Key.Año,
                                           numeroSemana = g.Key.NumeroSemana,
                                           totalFaltas = g.Sum(x => x.faltas),
                                           totalEmpleadosRequeridos = g.Sum(x => x.empleadoRequeridos)
                                       })
                                       .ToList();

                List<DataTablaFaltas> datatabla = new List<DataTablaFaltas>();  
                foreach (var suc in sucursales)
                {  
                    DataTablaFaltas itemdt = new DataTablaFaltas(); 
                    var temp = data.Where(x => x.idUbicacion == suc.idUbicacion).OrderByDescending(x => x.numeroSemana).ToList();
                    int count = 1; 
                    
                    itemdt.idUbicacion = suc.idUbicacion;
                    itemdt.nombreUbicacion = suc.nombre; 

                    itemdt.w1 = temp.ElementAt(0) != null ? temp.ElementAt(0).totalFaltas : 0;
                    itemdt.w2 = temp.ElementAt(1) != null ? temp.ElementAt(1).totalFaltas : 0;
                    itemdt.w3 = temp.ElementAt(2) != null ? temp.ElementAt(2).totalFaltas : 0;

                    datatabla.Add(itemdt);  
                }

                string emailbody = await getBody(data, datatabla, conteos[0], conteos[1]);
               _mail.EnviarCorreoFaltas(emailbody, "Faltas del personal en sucursales"); 
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
            
        }

        public static int GetWeekNumber(DateTime date)
        {
            // Crear una cultura personalizada que inicie la semana en domingo
            CultureInfo culture = new CultureInfo("en-US");

            // Configurar el calendario para usar domingo como primer día de la semana
            culture.DateTimeFormat.FirstDayOfWeek = DayOfWeek.Sunday;

            // Obtener el número de semana
            Calendar calendar = culture.Calendar;
            int weekNumber = calendar.GetWeekOfYear(
                date,
                culture.DateTimeFormat.CalendarWeekRule,
                culture.DateTimeFormat.FirstDayOfWeek
            );

            return weekNumber;
        }

        public async Task<List<UbicacionModel>> getUbicaciones() 
        {
            try
            {
                List<UbicacionModel> ubicaciones = new List<UbicacionModel>();
                // Crear conexión
                string connectionstring = _tdbContext.Database.GetConnectionString();
                using (SqlConnection connection = new SqlConnection(connectionstring))
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
                    connection.Close();
                }

                return ubicaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return new List<UbicacionModel>();
            }
        }


        public async Task<List<String>> getConteosCorreo()
        {
            try
            {
                DateTime fechaFin = DateTime.Now.AddDays(-4);
                DateTime fechaIni = fechaFin.AddDays(-6);

                List<dataSemana> dataChecadasManuales = new List<dataSemana>();
                List<dataSemana> dataTurnosLargos = new List<dataSemana>();

                List<string> data = new List<string>();

                List<UbicacionModel> sucursales = await getUbicaciones();
                sucursales = sucursales.Where(x => x.nombre.Contains("REBEL WINGS")).ToList();
                // Crear conexión
                string connectionstring = _tdbContext.Database.GetConnectionString();
                using var connection = new SqlConnection(connectionString);

                for (int i = 1; i < 4; i++) 
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@FI", fechaIni.Date, DbType.DateTime);
                    parameters.Add("@FF", fechaFin.Date, DbType.DateTime);
                    var result = await connection.QueryAsync<ConteosCorreo>(
                        "dbo.SP_NOMINA_CHECADAS_MANUALES",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    dataChecadasManuales.Add(new dataSemana() { semana = i, data = result.ToList() });

                    var parameters2 = new DynamicParameters();
                    parameters2.Add("@FI", fechaIni.Date, DbType.DateTime);
                    parameters2.Add("@FF", fechaFin.Date, DbType.DateTime);
                    var result2 = await connection.QueryAsync<ConteosCorreo>(
                        "dbo.SP_NOMINA_TURNOS_LARGOS",
                        parameters2,
                        commandType: CommandType.StoredProcedure
                    );

                    dataTurnosLargos.Add(new dataSemana() { semana = i, data = result2.ToList() });

                    fechaIni = fechaIni.AddDays(-7);
                   fechaFin = fechaFin.AddDays(-7);
                }

                string rows = "";

                var dataw1 = dataChecadasManuales.Where(x => x.semana == 1).FirstOrDefault().data;
                var dataw2 = dataChecadasManuales.Where(x => x.semana == 2).FirstOrDefault().data;
                var dataw3 = dataChecadasManuales.Where(x => x.semana == 3).FirstOrDefault().data;
                List<DataTablaFaltas> datatablacm = new List<DataTablaFaltas>();

                foreach (var item in sucursales)
                {
                    int itemw1 = 0, itemw2 = 0, itemw3 = 0;
                    itemw1 = dataw1.FirstOrDefault(x => x.Cla_ubicacion == item.idUbicacion)?.Total ?? 0;
                    itemw2 = dataw2.FirstOrDefault(x => x.Cla_ubicacion == item.idUbicacion)?.Total ?? 0;
                    itemw3 = dataw3.FirstOrDefault(x => x.Cla_ubicacion == item.idUbicacion)?.Total ?? 0;

                    datatablacm.Add(new DataTablaFaltas() { idUbicacion = item.idUbicacion, nombreUbicacion = item.nombre,w1 = itemw1,w2 = itemw2,w3 = itemw3 });

                }

                foreach (var item in datatablacm.OrderByDescending(x=>x.w1))
                {
                    rows += "<tr>";
                    rows += "<td>" + item.nombreUbicacion.Replace("REBEL WINGS", "") + "</td>";
                    rows += "<td " + getColor(item.w1) + ">" + item.w1 + "</td>";
                    rows += "<td " + getColor(item.w2) + ">" + item.w2 + "</td>";
                    rows += "<td " + getColor(item.w3) + ">" + item.w3 + "</td>";
                    rows += "</tr>";
                }

                data.Add(rows);

                rows = "";

                dataw1 = dataTurnosLargos.Where(x => x.semana == 1).FirstOrDefault().data;
                dataw2 = dataTurnosLargos.Where(x => x.semana == 2).FirstOrDefault().data;
                dataw3 = dataTurnosLargos.Where(x => x.semana == 3).FirstOrDefault().data;

                List<DataTablaFaltas> datatablatl = new List<DataTablaFaltas>();
                foreach (var item in sucursales)
                {
                    int itemw1 = 0, itemw2 = 0, itemw3 = 0;
                    itemw1 = dataw1.FirstOrDefault(x => x.Cla_ubicacion == item.idUbicacion)?.Total ?? 0;
                    itemw2 = dataw2.FirstOrDefault(x => x.Cla_ubicacion == item.idUbicacion)?.Total ?? 0;
                    itemw3 = dataw3.FirstOrDefault(x => x.Cla_ubicacion == item.idUbicacion)?.Total ?? 0;

                    datatablatl.Add(new DataTablaFaltas() { idUbicacion = item.idUbicacion, nombreUbicacion = item.nombre, w1 = itemw1, w2 = itemw2, w3 = itemw3 });
                }

                foreach (var item in datatablatl.OrderByDescending(x => x.w1))
                {
                    rows += "<tr>";
                    rows += "<td>" + item.nombreUbicacion.Replace("REBEL WINGS", "") + "</td>";
                    rows += "<td " + getColor(item.w1) + ">" + item.w1 + "</td>";
                    rows += "<td " + getColor(item.w2) + ">" + item.w2 + "</td>";
                    rows += "<td " + getColor(item.w3) + ">" + item.w3 + "</td>";
                    rows += "</tr>";
                }

                data.Add(rows);

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return new List<string>();
            }
        }


        public async Task<string> getBody(List<ResumenSemanal> data,List<DataTablaFaltas> datatabla,string bodycm,string bodytl) 
        {
            string body = @"<!DOCTYPE html>
<html lang=""es"">
<head>
  <meta charset=""UTF-8"">
  <title>Personal Faltante</title>
  <style>
    .table
    {
        width: 100%;
        table-layout: auto;
        text-align: center;
        vertical-align:top;
        border-collapse: collapse;
    }
    th,td{ padding: 0 px; 
           border: 1px solid #bbb;}
  </style>
</head>
<body style=""background-color: #f9f9f9; color: #333; padding: 0px; margin: 0px;"">
  <table style=""margin: auto; background-color: #ffffff; width: 100%;"">
       <tr style=""width: 100%; border: none;"">
    <td align=""center"" style=""padding-top: 20px; border: none;"">
      <img src=""https://rebelwings.mx/wp-content/uploads/2017/12/RW_LogoWEB.png"" alt=""logo"" style=""display: block;"" width=""150px"" border=""0""> 
    </td>
  </tr>
  
    <tr style=""border: none;"">
      <td style=""padding: 10px; padding-top: 0px; border: none;"">

        <div style=""margin: 0px;"">
            
            <table class=""table"">
                <thead>
                    <tr style=""background-color: rgb(219, 29, 29); color: white;"">
                        <th colspan=""4"">FALTAS</th>
                    </tr>
                    <tr>
                    <th scope=""col"">SUCURSAL</th>
                    <th scope=""col"">--w1</th>
                    <th scope=""col"">--w2</th>
                    <th scope=""col"">--w3</th>
                    </tr>
                </thead>
                <tbody>
                   --body
                </tbody>
            </table>
<br>
 <table class=""table"">
                <thead>
                    <tr style=""background-color: rgb(219, 29, 29); color: white;"">
                        <th colspan=""4"">CHECADAS MANUALES</th>
                    </tr>
                    <tr>
                    <th scope=""col"">SUCURSAL</th>
                    <th scope=""col"">--w1</th>
                    <th scope=""col"">--w2</th>
                    <th scope=""col"">--w3</th>
                    </tr>
                </thead>
                <tbody>
                   --cm
                </tbody>
            </table>
<br>
 <table class=""table"">
                <thead>
                    <tr style=""background-color: rgb(219, 29, 29); color: white;"">
                        <th colspan=""4"">TURNOS LARGOS</th>
                    </tr>
                    <tr>
                    <th scope=""col"">SUCURSAL</th>
                    <th scope=""col"">--w1</th>
                    <th scope=""col"">--w2</th>
                    <th scope=""col"">--w3</th>
                    </tr>
                </thead>
                <tbody>
                   --tl
                </tbody>
            </table>


        </div>
      </td>
    </tr>
  </table>
</body>
</html>";

            var semanasDistintas = data
                .Select(x => x.numeroSemana)
                .Distinct()
                .OrderByDescending(semana => semana)
                .ToList();

            for(int i = 1; i <= semanasDistintas.Count; i++) 
            {
                body = body.Replace("--w"+i, "W"+semanasDistintas.ElementAt(i-1).ToString()); 
            }

            string rows = "";
            
            datatabla = datatabla.OrderByDescending(x => x.w1).ToList();

            foreach (var item in datatabla) 
            {   
                rows += "<tr>";
                rows += "<td>" + item.nombreUbicacion.Replace("REBEL WINGS","") + "</td>";
                rows += "<td "+getColor(item.w1)+">" +item.w1+"</td>";
                rows += "<td "+getColor(item.w2)+">" + item.w2 + "</td>";
                rows += "<td "+getColor(item.w3)+">" + item.w3 + "</td>";
                rows += "</tr>";
            }

            body = body.Replace("--body", rows); 
            body = body.Replace("--cm", bodycm);
            body = body.Replace("--tl", bodytl);

            return body;
        }

        public string getColor(int faltas) 
        {
            string color = ""; 
            string colorRojo = "style='background-color:red; color:white;'";
            string colorAmarillo = "style='background-color:yellow; color:black;'";
            string colorVerde = "style='background-color:green; color:white;'";

            if (faltas < 4) { color = colorVerde; }
            if (faltas >= 4 && faltas < 7) { color = colorAmarillo; }
            if (faltas >= 7) { color = colorRojo; }
            return color; 
        }
    }

    public class incidenciasModel 
    {
        public int idUbicacion {  get; set; }
        public string nombreUbicacion { get; set; }
        public DateTime fecha { get; set;}
        public int faltas { get; set; }
        public int empleadoRequeridos { get; set; }
    }

    public class ResumenSemanal
    {
        public int idUbicacion { get; set; }
        public string nombreUbicacion { get; set; }
        public int año { get; set; }
        public int numeroSemana { get; set; }
        public int totalFaltas { get; set; }
        public int totalEmpleadosRequeridos { get; set; }
    }

    public class  DataTablaFaltas
    {
        public int idUbicacion { get; set;}
        public string nombreUbicacion { get;set; }
        public int w1 { get; set; }
        public int w2 { get; set;}  
        public int w3 { get; set;}
    }

    public class ConteosCorreo
    {
        public int Cla_ubicacion { get; set; }
        public string Nom_ubicacion { get; set; }
        public int Total { get; set; }
    }

    public class dataSemana
    {
        public int semana { get; set; }
        public List<ConteosCorreo> data { get; set; }

    }  

}
