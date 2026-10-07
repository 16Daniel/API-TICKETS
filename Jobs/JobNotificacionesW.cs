namespace TICKETSAPI.Jobs
{
    using Google.Cloud.Firestore;
    using Quartz;
    using System.Text;
    using System.Text.Json;
    using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

    [DisallowConcurrentExecution] 
    public class RevisarTicketsVencidosJob : IJob
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RevisarTicketsVencidosJob> _logger;
        private readonly HttpClient _httpClient;

        public RevisarTicketsVencidosJob(
            FirestoreDb firestoreDb,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<RevisarTicketsVencidosJob> logger,
            HttpClient httpClient)
        {
            _firestoreDb = firestoreDb;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Iniciando revisión de tickets vencidos: {Time}", DateTimeOffset.Now);

            try
            {
                CollectionReference ticketsRef = _firestoreDb.Collection("tickets");

                // Consulta en Firestore: fechaLimite < UtcNow, no notificados y aún no resueltos
                Google.Cloud.Firestore.Query query = ticketsRef
                 //.WhereGreaterThan("fechaEstimacion", DateTime.Now)
                 .WhereNotIn("idEstatusTicket", new[] {"3"});

                QuerySnapshot snapshot = await query.GetSnapshotAsync(context.CancellationToken);

                _logger.LogInformation("Se encontraron {Count} tickets vencidos pendientes de notificación.", snapshot.Documents.Count);

                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    var ticket = document.ToDictionary();
                    string ticketId = document.Id;

                    string telefonoAgente = ticket.ContainsKey("telefonoResponsable") ? ticket["telefonoResponsable"].ToString() : string.Empty;
                    string titulo = ticket.ContainsKey("titulo") ? ticket["titulo"].ToString() : "Sin título";

                    if (string.IsNullOrWhiteSpace(telefonoAgente))
                    {
                        _logger.LogWarning("El ticket {TicketId} no tiene un teléfono registrado.", ticketId);
                        continue;
                    }

           
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error al procesar el Job de tickets vencidos.");
            }
        }


        public async Task<bool> SendWhatsAppMessageAsync(string toNumber, string message)
        {
            try
            {
                var instance = _configuration["UltraMsg:InstanceID"];
                var token = _configuration["UltraMsg:Token"];

                if (string.IsNullOrEmpty(instance) || string.IsNullOrEmpty(token))
                {
                    _logger.LogError("Configuración de UltraMsg incompleta en appsettings.json.");
                    return false;
                }

                var url = $"https://api.ultramsg.com/{instance}/messages/chat";

                var formData = new Dictionary<string, string>
            {
                { "token", token },
                { "to", toNumber },
                { "body", message }
            };

                var content = new FormUrlEncodedContent(formData);
                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error al enviar mensaje UltraMsg: {Response}", responseContent);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción al intentar enviar mensaje por UltraMsg");
                return false;
            }
        }

    }
}
