using FirebaseAdmin.Messaging;
using TICKETSAPI.Controllers;

namespace TICKETSAPI.Funciones
{
    public interface INotificationService
    {
        Task<PushNotificationResult> EnviarPushAsync(PushNotificationRequest request, CancellationToken cancellationToken = default);
    }

    public class PushNotificationResult
    {
        public bool Exito { get; set; }
        public int Enviados { get; set; }
        public int Fallidos { get; set; }
        public List<string> TokensInvalidos { get; set; } = new();
        public string Error { get; set; }
    }

    public class NotificationService : INotificationService
    {
        private readonly FirebaseMessaging _messaging;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(FirebaseMessaging messaging, ILogger<NotificationService> logger)
        {
            _messaging = messaging;
            _logger = logger;
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
                    Notification = new Notification
                    {
                        Title = request.Titulo,
                        Body = request.Mensaje
                    },
                    Tokens = batch.ToList()
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
                        {
                            tokensInvalidos.Add(batch[i]);
                        }
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
