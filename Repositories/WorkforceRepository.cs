using System.Data;
using System.Net.Http.Headers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TICKETSAPI.ModelsTickets;

namespace TICKETSAPI.Repositories
{
    public class WorkforceRepository : IWorkforceRepository
    {
        private const string WorkforceDatastreamEndpoint = "https://my.workforce.com/api/v2/storestats/for_datastream";

        private readonly TicketsContext _ticketsContext;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public WorkforceRepository(
            TicketsContext ticketsContext,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _ticketsContext = ticketsContext;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task ProcesarAsync(DateTime fecha, CancellationToken cancellationToken = default)
        {
            fecha = fecha.Date;

            var datastreams = await ObtenerDatastreamsSucursalesAsync(cancellationToken);
            var idfs = datastreams
                .Select(datastream => datastream.Idf)
                .Distinct()
                .ToList();

            foreach (int idf in idfs)
            {
                VentasDatastreamResult ventas = await ObtenerVentasDatastreamAsync(
                    idf,
                    fecha,
                    fecha,
                    cancellationToken);

                if (ventas.VentaSalon == 0 && ventas.VentaDelivery == 0)
                {
                    continue;
                }

                var datastreamsSucursal = datastreams
                    .Where(datastream => datastream.Idf == idf)
                    .ToList();

                for (DateTime fechaBloque = fecha;
                     fechaBloque < fecha.AddDays(1);
                     fechaBloque = fechaBloque.AddMinutes(30))
                {
                    long timeBloque = ObtenerTimeBloqueUtc(fechaBloque);

                    bool yaEnviado = await _ticketsContext.LogPoleoWorkforces
                        .AsNoTracking()
                        .AnyAsync(log =>
                            log.Idf == idf &&
                            log.Time == timeBloque &&
                            log.Enviado &&
                            log.Fecha.Date == DateTime.UtcNow.Date,
                            cancellationToken);

                    if (yaEnviado)
                    {
                        continue;
                    }

                    VentasDatastreamResult ventasBloque = await ObtenerVentasDatastreamBloqueAsync(
                        idf,
                        fechaBloque,
                        fechaBloque.AddMinutes(30).AddSeconds(-1),
                        cancellationToken);

                    var solicitudes = new List<WorkforceDatastreamRequest>();

                    AgregarSolicitud(solicitudes, datastreamsSucursal, 1, ventasBloque.VentaSalon, timeBloque, "sales");
                    AgregarSolicitud(solicitudes, datastreamsSucursal, 2, ventasBloque.VentaDelivery, timeBloque, "sales");
                    AgregarSolicitud(solicitudes, datastreamsSucursal, 3, ventasBloque.AlimentosSalon, timeBloque, "articulos");
                    AgregarSolicitud(solicitudes, datastreamsSucursal, 4, ventasBloque.BebidasSalon, timeBloque, "articulos");
                    AgregarSolicitud(solicitudes, datastreamsSucursal, 5, ventasBloque.AlimentosDelivery, timeBloque, "articulos");
                    AgregarSolicitud(solicitudes, datastreamsSucursal, 6, ventasBloque.BebidasDelivery, timeBloque, "articulos");

                    if (solicitudes.Count > 0)
                    {
                        await EnviarDatastreamAsync(solicitudes, idf, cancellationToken);
                    }
                }
            }
        }

        private static void AgregarSolicitud(
            ICollection<WorkforceDatastreamRequest> solicitudes,
            IReadOnlyCollection<DatastreamsSucursale> datastreamsSucursal,
            int tipo,
            decimal stat,
            long time,
            string type)
        {
            var datastream = datastreamsSucursal.FirstOrDefault(ds => ds.Tipo == tipo);

            if (datastream == null)
            {
                return;
            }

            solicitudes.Add(new WorkforceDatastreamRequest
            {
                DatastreamId = datastream.IdDatastream,
                Stat = stat,
                Time = time,
                Type = type
            });
        }

        private async Task<List<DatastreamsSucursale>> ObtenerDatastreamsSucursalesAsync(
            CancellationToken cancellationToken)
        {
            return await _ticketsContext.DatastreamsSucursales
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        private async Task<VentasDatastreamResult> ObtenerVentasDatastreamAsync(
            int idf,
            DateTime fechaIni,
            DateTime fechaFin,
            CancellationToken cancellationToken)
        {
            return await EjecutarProcedimientoVentasAsync(
                "dbo.GET_VENTAS_DATASTREAMS",
                idf,
                fechaIni,
                fechaFin,
                SqlDbType.Date,
                cancellationToken);
        }

        private async Task<VentasDatastreamResult> ObtenerVentasDatastreamBloqueAsync(
            int idf,
            DateTime fechaIni,
            DateTime fechaFin,
            CancellationToken cancellationToken)
        {
            return await EjecutarProcedimientoVentasAsync(
                "dbo.GET_VENTAS_DATASTREAMS_BLOQUE",
                idf,
                fechaIni,
                fechaFin,
                SqlDbType.DateTime,
                cancellationToken);
        }

        private async Task<VentasDatastreamResult> EjecutarProcedimientoVentasAsync(
            string procedimiento,
            int idf,
            DateTime fechaIni,
            DateTime fechaFin,
            SqlDbType tipoFecha,
            CancellationToken cancellationToken)
        {
            var connection = _ticketsContext.Database.GetDbConnection();
            bool cerrarConexion = connection.State == ConnectionState.Closed;

            if (cerrarConexion)
            {
                await connection.OpenAsync(cancellationToken);
            }

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = procedimiento;
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@IDF", SqlDbType.Int) { Value = idf });
                command.Parameters.Add(new SqlParameter("@FECHAINI", tipoFecha) { Value = fechaIni });
                command.Parameters.Add(new SqlParameter("@FECHAFIN", tipoFecha) { Value = fechaFin });

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);

                if (!await reader.ReadAsync(cancellationToken))
                {
                    return new VentasDatastreamResult();
                }

                return new VentasDatastreamResult
                {
                    VentaSalon = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                    VentaDelivery = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                    AlimentosSalon = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                    BebidasSalon = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                    AlimentosDelivery = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                    BebidasDelivery = reader.IsDBNull(5) ? 0 : reader.GetInt32(5)
                };
            }
            finally
            {
                if (cerrarConexion)
                {
                    await connection.CloseAsync();
                }
            }
        }

        private static long ObtenerTimeBloqueUtc(DateTime fechaLocal)
        {
            TimeZoneInfo zonaMexico = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");
            DateTime fechaSinZona = DateTime.SpecifyKind(fechaLocal, DateTimeKind.Unspecified);
            DateTime fechaUtc = TimeZoneInfo.ConvertTimeToUtc(fechaSinZona, zonaMexico);

            return new DateTimeOffset(fechaUtc).ToUnixTimeSeconds();
        }

        private async Task<string> EnviarDatastreamAsync(
            List<WorkforceDatastreamRequest> datastreams,
            int idf,
            CancellationToken cancellationToken)
        {
            string? token = _configuration["Workforce:Token"];

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException("No se ha configurado Workforce:Token en appsettings.");
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                WorkforceDatastreamEndpoint);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = JsonContent.Create(new { stats = datastreams });

            using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
            string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            foreach (var datastream in datastreams)
            {
                _ticketsContext.LogPoleoWorkforces.Add(new LogPoleoWorkforce
                {
                    Idf = idf,
                    Iddatastream = datastream.DatastreamId,
                    Fecha = DateTime.UtcNow,
                    Enviado = response.IsSuccessStatusCode,
                    Msg = responseContent,
                    Time = datastream.Time,
                    Stat = datastream.Stat.ToString(),
                    Type = datastream.Type
                });
            }

            await _ticketsContext.SaveChangesAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Error al enviar datastream a Workforce. HTTP {(int)response.StatusCode}: {responseContent}");
            }

            return responseContent;
        }
    }
}
