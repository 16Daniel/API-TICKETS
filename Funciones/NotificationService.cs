using FirebaseAdmin.Messaging;
using Google.Cloud.Firestore;
using System.Text.Json;
using TICKETSAPI.Controllers;

namespace TICKETSAPI.Funciones
{
    public class TicketNotificationRequest
    {
        public string TicketId { get; set; } = string.Empty;
        public string Folio { get; set; } = string.Empty;
        public string IdSucursal { get; set; } = string.Empty;
        public string IdAlerta { get; set; } = string.Empty;
        public List<string> Usuarios { get; set; } = new();
    }

    [FirestoreData]
    public class HistorialNotificacion
    {
        [FirestoreProperty] public string ticketId { get; set; } = string.Empty;
        [FirestoreProperty] public DateTime fecha { get; set; }
        [FirestoreProperty] public string idAlerta { get; set; } = string.Empty;
        [FirestoreProperty] public List<string> usuarios { get; set; } = new();
        [FirestoreProperty] public Dictionary<string, bool> lecturas { get; set; } = new();
    }

    public interface INotificationService
    {
        Task<PushNotificationResult> EnviarPushAsync(PushNotificationRequest request, CancellationToken cancellationToken = default);
        Task<PushNotificationResult?> ProcesarAlertaAsync(TicketNotificationRequest request, CancellationToken cancellationToken = default);
    }

    public class PushNotificationResult
    {
        public bool Exito { get; set; }
        public int Enviados { get; set; }
        public int Fallidos { get; set; }
        public List<string> TokensInvalidos { get; set; } = new();
        public string Error { get; set; } = string.Empty;
    }

    public class NotificationService : INotificationService
    {
        private readonly FirebaseMessaging _messaging;
        private readonly ILogger<NotificationService> _logger;
        private readonly FirestoreDb _firestoreDb;

        public NotificationService(FirebaseMessaging messaging, ILogger<NotificationService> logger, FirestoreDb firestoreDb)
        {
            _messaging = messaging;
            _logger = logger;
            _firestoreDb = firestoreDb;
        }

        public async Task<PushNotificationResult?> ProcesarAlertaAsync(TicketNotificationRequest request, CancellationToken cancellationToken = default)
        {
            var usuarios = request.Usuarios
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Distinct()
                .ToList();

            var query = _firestoreDb.Collection("historial_alertas")
                .WhereEqualTo("ticketId", request.TicketId)
                .WhereEqualTo("idAlerta", request.IdAlerta)
                .Limit(1);

            var existente = await query.GetSnapshotAsync(cancellationToken);
            if (existente.Documents.Count > 0)
                return null;

            var historial = new HistorialNotificacion
            {
                ticketId = request.TicketId,
                fecha = DateTime.UtcNow,
                idAlerta = request.IdAlerta,
                usuarios = usuarios,
                lecturas = usuarios.ToDictionary(id => id, _ => false)
            };

            await _firestoreDb.Collection("historial_alertas").AddAsync(historial, cancellationToken);

            if (usuarios.Count == 0)
                return null;


            var tokens = await ObtenerTokensAsync(usuarios, cancellationToken);
            if (tokens.Count == 0)
                return new PushNotificationResult { Exito = true };

            var sucursal = await _firestoreDb.Collection("cat_sucursales")
                .Document(request.IdSucursal)
                .GetSnapshotAsync(cancellationToken);

            sucursal.TryGetValue("nombre", out string nombreSucursal);

            var alerta = await _firestoreDb.Collection("cat_alertas")
                .Document(request.IdAlerta)
                .GetSnapshotAsync(cancellationToken);

            alerta.TryGetValue("nombre", out string nombreAlerta);

            var resultado = await EnviarPushAsync(new PushNotificationRequest
            {
                tokens = tokens,
                Titulo = nombreAlerta,
                Mensaje = request.Folio + "\n" + (nombreSucursal ?? request.IdSucursal),
                usuarios = usuarios
            }, cancellationToken);

            await RemoverTokensInvalidosAsync(resultado.TokensInvalidos, cancellationToken);

            _logger.LogInformation(
                "Alerta {Alerta} procesada para el Ticket {TicketId}: {Exitos} enviados, {Fallos} fallidos.",
                request.IdAlerta,
                request.TicketId,
                resultado.Enviados,
                resultado.Fallidos);

            return resultado;
        }

        private async Task<List<string>> ObtenerTokensAsync(IEnumerable<string> usuarios, CancellationToken cancellationToken)
        {
            var tokens = new List<string>();

            foreach (var usuarioId in usuarios)
            {
                var snapshot = await _firestoreDb.Collection("usuarios")
                    .Document(usuarioId)
                    .GetSnapshotAsync(cancellationToken);

                if (snapshot.Exists && snapshot.TryGetValue("fcmToken", out List<string> usuarioTokens) && usuarioTokens != null)
                    tokens.AddRange(usuarioTokens);
            }

            return tokens
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct()
                .ToList();
        }

        private async Task RemoverTokensInvalidosAsync(IEnumerable<string> tokensInvalidos, CancellationToken cancellationToken)
        {
            foreach (var token in tokensInvalidos.Distinct())
            {
                var usuarios = await _firestoreDb.Collection("usuarios")
                    .WhereArrayContains("fcmToken", token)
                    .GetSnapshotAsync(cancellationToken);

                foreach (var usuario in usuarios.Documents)
                {
                    await usuario.Reference.UpdateAsync(new Dictionary<string, object>
                    {
                        ["fcmToken"] = FieldValue.ArrayRemove(token)
                    }, null, cancellationToken);
                }
            }
        }

        public async Task<PushNotificationResult> EnviarPushAsync(PushNotificationRequest request, CancellationToken cancellationToken = default)
        {
            if (request?.tokens == null || !request.tokens.Any())
            {
                return new PushNotificationResult { Exito = false, Error = "Debe proporcionar al menos un token." };
            }

            var tokensValidos = request.tokens
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct()
                .ToList();

            if (!tokensValidos.Any())
            {
                return new PushNotificationResult { Exito = false, Error = "Ningún token proporcionado es válido." };
            }

            int totalExitos = 0;
            int totalFallos = 0;
            var tokensInvalidos = new List<string>();

            const int batchSize = 500;
            foreach (var batch in tokensValidos.Chunk(batchSize))
            {
                var message = new MulticastMessage
                {
                    Tokens = batch.ToList(),
                    Data = new Dictionary<string, string>
                    {
                        ["usuarios"] = JsonSerializer.Serialize(request.usuarios ?? new List<string>()),
                        ["title"] = request.Titulo,
                        ["body"] = request.Mensaje
                    }
                };

                BatchResponse batchResponse = await _messaging.SendEachForMulticastAsync(message, cancellationToken);
                totalExitos += batchResponse.SuccessCount;
                totalFallos += batchResponse.FailureCount;

                for (int i = 0; i < batchResponse.Responses.Count; i++)
                {
                    var sendResponse = batchResponse.Responses[i];
                    if (!sendResponse.IsSuccess && sendResponse.Exception != null)
                    {
                        var errorCode = sendResponse.Exception.MessagingErrorCode;
                        if (errorCode == MessagingErrorCode.Unregistered || errorCode == MessagingErrorCode.InvalidArgument)
                            tokensInvalidos.Add(batch[i]);
                    }
                }
            }

            return new PushNotificationResult
            {
                Exito = true,
                Enviados = totalExitos,
                Fallidos = totalFallos,
                TokensInvalidos = tokensInvalidos
            };
        }
    }
}
