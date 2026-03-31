using DashboardApi.ModelsBD1;
using DashboardApi.ModelsBD2;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Data;
using TICKETSAPI.Controllers;
using TICKETSAPI.ModelsTickets;

namespace TICKETSAPI.Jobs
{
 
    public class JobRemisiones : IJob
    {
        protected BD2Context _contextdb2;
        protected BD1Context _contextdb1;
        protected TicketsContext _tdbContext;
        public string connectionString = string.Empty;
        private readonly IConfiguration _configuration;
        private readonly ILogger<JobRemisiones> _logger;
        private static readonly HttpClient client = new HttpClient();
        string URLBASE = "https://localhost:7165/api/";

        public JobRemisiones(ILogger<JobRemisiones> logger, BD2Context db2c, IConfiguration configuration, BD1Context BDC, TicketsContext tdbContext)
        {
            _logger = logger;
            _contextdb2 = db2c;
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("DefaultConnection");
            _contextdb1 = BDC;
            _tdbContext = tdbContext;
        }

        public async Task Execute(IJobExecutionContext context)
        {

            try
            {

                await actualizarRemisiones(); 
            }
            catch (Exception ex)
            {
            }
        }
        public async Task actualizarRemisiones() 
        {
            var resultados = new List<RemisionAceite>();
            var connectionString = _tdbContext.Database.GetDbConnection().ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                using (var command = new SqlCommand("SP_GET_REMINISIONES_ACEITE", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@FI", SqlDbType.DateTime).Value = DateTime.Now.Date;
                    command.Parameters.Add("@FF", SqlDbType.DateTime).Value = DateTime.Now.Date.Date;

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new RemisionAceite
                            {
                                CodAlmacen = reader["CODALMACEN"].ToString(),
                                NombreAlmacen = reader["NOMBREALMACEN"].ToString(),
                                Compras = Convert.ToDecimal(reader["COMPRAS"]),
                                Consumos = Convert.ToDecimal(reader["CONSUMOS"]),
                                Descripcion = reader["DESCRIPCION"].ToString(),
                                Referencia = reader["REFERENCIA"].ToString(),
                                CodigoInterno = reader["CODIGO_INTERNO"].ToString(),
                                Marca = reader["MARCA"].ToString(),
                                Fecha = Convert.ToDateTime(reader["FECHA"])
                            };

                            resultados.Add(item);
                        }
                    }
                }
            }

            foreach (var item in resultados)
            {
                int idf = -1;
                try { idf = int.Parse(item.CodAlmacen); } catch (Exception ex) { idf = -1; }
                if (idf > -1)
                {
                    var reg = _tdbContext.ControlAceites.Where(x => x.IdSucursal == idf && x.Fecha.Date == item.Fecha.Date && x.Manual == null).FirstOrDefault();
                    if (reg == null)
                    {
                        _tdbContext.ControlAceites.Add(new ControlAceite()
                        {
                            IdSucursal = idf,
                            Fecha = item.Fecha,
                            EntregaCedis = (double)item.Compras,
                            Status = 1,
                            Fecharecoleccion = item.Fecha
                        });
                        await _tdbContext.SaveChangesAsync();
                    }
                }
            }

        }
    }
}
