namespace TICKETSAPI.Jobs
{
    using Google.Cloud.Firestore;
    using Quartz;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Text;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using TICKETSAPI.Controllers;
    using TICKETSAPI.Funciones;
    using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

    [DisallowConcurrentExecution] 
    public class RevisarTicketsVencidosJob : IJob
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly INotificationService _notificationService;
        private readonly ILogger<RevisarTicketsVencidosJob> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public RevisarTicketsVencidosJob(
            FirestoreDb firestoreDb,
            INotificationService notificationService,
            ILogger<RevisarTicketsVencidosJob> logger,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _firestoreDb = firestoreDb;
            _notificationService = notificationService;
            _logger = logger;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Iniciando revisión de tickets vencidos: {Time}", DateTimeOffset.Now);

            try
            {
                //await AsegurarCatalogoAlertasExisteAsync(context.CancellationToken); 

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
                    
                    if (!document.TryGetValue<string>("idEstatusTicket", out string statusTicket))
                    {
                        statusTicket = string.Empty;
                    }

                    if (!document.TryGetValue<double>("horasAtencion", out double horasAtencion))
                    {
                        horasAtencion = 0;
                    }

                    if (!document.TryGetValue<double>("horasResolucion", out double horasResolucion))
                    {
                        horasResolucion = 0;
                    }

                    if (!document.TryGetValue<DateTime>("fecha", out DateTime fechacreacion))
                    {
                        fechacreacion = DateTime.Now;
                    }

                    if (!document.TryGetValue<DateTime>("fechaAtencion", out DateTime fechaAtencion))
                    {
                        fechaAtencion = DateTime.Now;
                    }

                    if (!document.TryGetValue<string>("idResponsable", out string idresponsable))
                    {
                        idresponsable = string.Empty;
                    }

                    if (!document.TryGetValue<List<string>>("idInvolucrados", out List<string> involucrados) || involucrados == null)
                    {
                        involucrados = new List<string>();
                    }

                    if (idresponsable != "") { involucrados.Add(idresponsable); }

                    var notificacion = new HistorialNotificacion
                    {
                        ticketId = ticketId,
                        fecha = DateTime.UtcNow,
                        idAlerta = "",
                        usuarios = involucrados,
                        lecturas = involucrados.Distinct().ToList().ToDictionary(id => id, _ => false)
                    };

                    if (statusTicket == "1")
                    {
                        if (ObtenerHorasTranscurridas(fechacreacion) < 24 && ObtenerHorasTranscurridas(fechacreacion) > horasAtencion)
                        {
                            notificacion.idAlerta = "2";
                            Boolean nuevanotificacion = await NoExisteAlertaRegistradaAsync(notificacion.ticketId, notificacion.idAlerta, context.CancellationToken);
                            if (nuevanotificacion)
                            {
                                await _firestoreDb.Collection("historial_alertas").AddAsync(notificacion);
                                List<string> tokens = new List<string>();
                                foreach (string item in involucrados)
                                {
                                    var tokensUsuario = await ObtenerTokensUsuarioAsync(item, context.CancellationToken);
                                    tokens.AddRange(tokensUsuario);
                                }
                                if (tokens.Count > 0)
                                {
                                    await DispararNotificacionAsync(ticketId, tokens, "Nueva alerta en el ticket", notificacion.idAlerta, involucrados, context.CancellationToken);
                                }
                                var grupo = await RevisarTicketsVencidosJob.ObtenerGrupoNotificacionAreaAsync(_firestoreDb, ticketId, true);
                                if (!string.IsNullOrWhiteSpace(grupo))
                                {
                                    string mensaje = await GenerarMensajeWAsync(_firestoreDb, ticketId, true);
                                    await SendWhatsAppMessageAsync(new WhatsAppRequestDto() { ToNumber = grupo, Message = mensaje });
                                }
                            }
                            continue;
                        }

                        if (ObtenerHorasTranscurridas(fechacreacion) > 24)
                        {
                            notificacion.idAlerta = "4";
                            Boolean nuevanotificacion = await NoExisteAlertaRegistradaAsync(notificacion.ticketId, notificacion.idAlerta, context.CancellationToken);
                            if (nuevanotificacion)
                            {
                                await _firestoreDb.Collection("historial_alertas").AddAsync(notificacion);
                                List<string> tokens = new List<string>();
                                foreach (string item in involucrados)
                                {
                                    var tokensUsuario = await ObtenerTokensUsuarioAsync(item, context.CancellationToken);
                                    tokens.AddRange(tokensUsuario);
                                }
                                if (tokens.Count > 0)
                                {
                                    await DispararNotificacionAsync(ticketId, tokens, "Nueva alerta en el ticket", notificacion.idAlerta, involucrados, context.CancellationToken);
                                }
                            }
                            continue;
                        }
                    }

                    if (int.Parse(statusTicket) > 1 && !int.Parse(statusTicket).Equals("8")) 
                    {
                        if (ObtenerHorasTranscurridas(fechaAtencion) < 24 && ObtenerHorasTranscurridas(fechaAtencion) > horasResolucion)
                        {
                            notificacion.idAlerta = "5";
                            Boolean nuevanotificacion = await NoExisteAlertaRegistradaAsync(notificacion.ticketId, notificacion.idAlerta, context.CancellationToken);
                            if (nuevanotificacion)
                            {
                                await _firestoreDb.Collection("historial_alertas").AddAsync(notificacion);
                                List<string> tokens = new List<string>();
                                foreach (string item in involucrados)
                                {
                                    var tokensUsuario = await ObtenerTokensUsuarioAsync(item, context.CancellationToken);
                                    tokens.AddRange(tokensUsuario);
                                }
                                if (tokens.Count > 0)
                                {
                                    await DispararNotificacionAsync(ticketId, tokens, "Nueva alerta en el ticket", notificacion.idAlerta, involucrados, context.CancellationToken);
                                }
                                var grupo = await RevisarTicketsVencidosJob.ObtenerGrupoNotificacionAreaAsync(_firestoreDb, ticketId, true);
                                if (!string.IsNullOrWhiteSpace(grupo))
                                {
                                    string mensaje = await GenerarMensajeWAsync(_firestoreDb, ticketId, false);
                                    await SendWhatsAppMessageAsync(new WhatsAppRequestDto() { ToNumber = grupo, Message = mensaje });
                                }
                            }
                            continue;
                        }

                        if (ObtenerHorasTranscurridas(fechaAtencion) > 24)
                        {
                            notificacion.idAlerta = "6";
                            Boolean nuevanotificacion = await NoExisteAlertaRegistradaAsync(notificacion.ticketId, notificacion.idAlerta, context.CancellationToken);
                            if (nuevanotificacion)
                            {
                                await _firestoreDb.Collection("historial_alertas").AddAsync(notificacion);
                                List<string> tokens = new List<string>();
                                foreach (string item in involucrados)
                                {
                                    var tokensUsuario = await ObtenerTokensUsuarioAsync(item, context.CancellationToken);
                                    tokens.AddRange(tokensUsuario);
                                }
                                if (tokens.Count > 0)
                                {
                                    await DispararNotificacionAsync(ticketId, tokens, "Nueva alerta en el ticket", notificacion.idAlerta, involucrados, context.CancellationToken);
                                }
                            }
                            continue;
                        }
                    }
           
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error al procesar el Job de tickets vencidos.");
            }
        }

        public static double ObtenerHorasTranscurridas(DateTime fechaInicial)
        {
            TimeSpan diferencia = DateTime.UtcNow - fechaInicial;

            return diferencia.TotalHours;
        }

        private async Task DispararNotificacionAsync(string ticketId, List<string> tokensFCM, string mensaje, string idAlerta,List<string>usuarios, CancellationToken ct)
        {
            var request = new PushNotificationRequest
            {
                tokens = tokensFCM,
                Titulo = "",
                Mensaje = mensaje,
                usuarios = usuarios
            };
            string nombre = await ObtenerNombreAlertaPorIdAsync(idAlerta); 
            request.Titulo = nombre;
            var resultado = await _notificationService.EnviarPushAsync(request, ct);
            _logger.LogInformation("Notificación enviada para Ticket {TicketId}: {Exitos} enviados, {Fallos} fallidos.",
                ticketId, resultado.Enviados, resultado.Fallidos);
        }

        public async Task<bool> NoExisteAlertaRegistradaAsync(string ticketId, string idAlerta, CancellationToken cancellationToken = default)
        {
            try
            {
                CollectionReference historialRef = _firestoreDb.Collection("historial_alertas");
                Google.Cloud.Firestore.Query query = historialRef
                    .WhereEqualTo("TicketId", ticketId)
                    .WhereEqualTo("idAlerta", idAlerta)
                    .Limit(1);

                QuerySnapshot snapshot = await query.GetSnapshotAsync(cancellationToken);
                return snapshot.Documents.Count == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de alerta para TicketId: {TicketId}, idAlerta: {IdAlerta}", ticketId, idAlerta);
                return false;
            }
        }

        public async Task<string> ObtenerNombreAlertaPorIdAsync(string idAlerta, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(idAlerta))
                return string.Empty;

            try
            {
                DocumentReference docRef = _firestoreDb.Collection("cat_alertas").Document(idAlerta);
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync(cancellationToken);

                if (snapshot.Exists && snapshot.TryGetValue("nombre", out string nombre))
                {
                    return nombre;
                }

                _logger.LogWarning("No se encontró la alerta con ID: {IdAlerta}", idAlerta);
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar la alerta con ID: {IdAlerta}", idAlerta);
                return string.Empty;
            }
        }

        public async Task<List<string>> ObtenerTokensUsuarioAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("El ID del usuario no puede estar vacío.", nameof(userId));
            }

            // Referencia directa al documento por su ID dentro de la colección 'usuarios'
            DocumentReference docRef = _firestoreDb.Collection("usuarios").Document(userId);

            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync(cancellationToken);

            if (!snapshot.Exists)
            {
                // El usuario no existe en la colección
                return new List<string>();
            }

            // Verifica si el campo existe y es un array/lista
            if (snapshot.TryGetValue("fcmToken", out List<string> tokens) && tokens != null)
            {
                return tokens;
            }

            // Si la propiedad existe como array de objetos genéricos en Firestore,
            // también puedes extraerla de forma segura así:
            if (snapshot.TryGetValue("fcmToken", out List<object> rawList) && rawList != null)
            {
                return rawList.ConvertAll(token => token?.ToString() ?? string.Empty);
            }

            return new List<string>();
        }

        public static async Task<string> GenerarMensajeWAsync(
       FirestoreDb db,
       string ticketId,Boolean ta)
        {
            var ticket = await ObtenerDocumentoAsync(db, "tickets", ticketId);

            if (ticket == null)
                throw new KeyNotFoundException($"No se encontró el ticket '{ticketId}'.");

            var idArea = ObtenerTexto(ticket, "idArea");
            var idSucursal = ObtenerTexto(ticket, "idSucursal");
            var idResponsable = ObtenerTexto(ticket, "idResponsable");

            var area = await ObtenerDocumentoAsync(db, "cat_areas", idArea);
            var sucursal = await ObtenerDocumentoAsync(db, "cat_sucursales", idSucursal);
            var responsable = await ObtenerDocumentoAsync(db, "usuarios", idResponsable);

            var nombreArea = ObtenerTexto(area, "nombre", idArea ?? "Sin área");
            var nombreSucursal = ObtenerTexto(sucursal, "nombre", idSucursal ?? "Sin sucursal");

            var nombreResponsable = responsable == null
                ? "Sin asignar"
                : string.Join(" ", System.Array.FindAll(new[]
                {
                ObtenerTexto(responsable, "nombre"),
                ObtenerTexto(responsable, "apellidoP"),
                ObtenerTexto(responsable, "apellidoM")
                }, nombre => !string.IsNullOrWhiteSpace(nombre)));

            if (string.IsNullOrWhiteSpace(nombreResponsable))
                nombreResponsable = "Sin asignar";

            var categoria = ObtenerTexto(ticket, "nombreCategoria", "Sin categoría");
            var subcategoria = ObtenerTexto(ticket, "nombreSubcategoria");

            var categoriaCompleta = string.IsNullOrWhiteSpace(subcategoria)
                ? categoria
                : $"{categoria} - {subcategoria}";

            var descripcion = LimpiarDescripcion(
                ObtenerTexto(ticket, "descripcion"));

            var tiempoAtencion = FormatearTiempo(
                ticket, "tiempoAtencion", "unidadAtencion");

            var tiempoResolucion = FormatearTiempo(
                ticket, "tiempoResolucion", "unidadResolucion");

            var descripcionCorta = descripcion.Length > 500
                ? descripcion.Substring(0, 497) + "..."
                : descripcion;

            return string.Join("\n", new[]
            {
            "======================",
            ta ? "🟠 TA: SUPERADO":"🔴 TR: SUPERADO",
            "======================",
            $"Area: {nombreArea}",
            $"Sucursal: {nombreSucursal}",
            $"Ticket: {ObtenerTexto(ticket, "folio", "Sin folio")}",
            $"Responsable: {nombreResponsable}",
            $"Prioridad: {ObtenerTexto(ticket, "prioridad", "Sin especificar")}",
            $"TA: {tiempoAtencion}",
            $"TR: {tiempoResolucion}",
            $"Categoria: {categoriaCompleta}",
            $"Descripcion: {(string.IsNullOrWhiteSpace(descripcionCorta) ? "Sin descripción" : descripcionCorta)}",
            "======================"
        });
        }

        public static async Task<string?> ObtenerGrupoNotificacionAreaAsync(
            FirestoreDb db,
            string ticketId,
            bool ta,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(db);

            if (string.IsNullOrWhiteSpace(ticketId))
                throw new ArgumentException("El ID del ticket es requerido.", nameof(ticketId));

            var ticket = await ObtenerDocumentoAsync(db, "tickets", ticketId, cancellationToken);
            if (ticket == null)
                throw new KeyNotFoundException($"No se encontró el ticket '{ticketId}'.");

            var idArea = ObtenerTexto(ticket, "idArea");
            if (string.IsNullOrWhiteSpace(idArea))
                return null;

            var area = await ObtenerDocumentoAsync(db, "cat_areas", idArea, cancellationToken);
            if (area == null)
                return null;

            var grupoNotificacion = ObtenerTexto(area, "gruponotificacion");
            if (string.IsNullOrWhiteSpace(grupoNotificacion) ||
                !area.TryGetValue("notificacionesTicket", out var configuracion) ||
                configuracion is not IDictionary<string, object> opciones)
            {
                return null;
            }

            var opcion = ta ? "superaTA" : "superaTR";
            return opciones.TryGetValue(opcion, out var valor) &&
                valor is bool habilitada &&
                habilitada
                    ? grupoNotificacion
                    : null;
        }

        private static async Task<IDictionary<string, object>> ObtenerDocumentoAsync(
            FirestoreDb db,
            string coleccion,
            string id,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            var snapshot = await db.Collection(coleccion)
                .Document(id)
                .GetSnapshotAsync(cancellationToken);
            return snapshot.Exists ? snapshot.ToDictionary() : null;
        }

        private static string ObtenerTexto(
            IDictionary<string, object> documento,
            string campo,
            string predeterminado = "")
        {
            if (documento == null ||
                !documento.TryGetValue(campo, out var valor) ||
                valor == null)
            {
                return predeterminado;
            }

            return Convert.ToString(valor, CultureInfo.InvariantCulture)?.Trim()
                ?? predeterminado;
        }

        private static string FormatearTiempo(
            IDictionary<string, object> ticket,
            string campoTiempo,
            string campoUnidad)
        {
            var tiempo = ObtenerTexto(ticket, campoTiempo);
            if (string.IsNullOrWhiteSpace(tiempo))
                return "N/D";

            var unidad = ObtenerTexto(ticket, campoUnidad);
            return tiempo + unidad;
        }

        private static string LimpiarDescripcion(string descripcion)
        {
            var texto = Regex.Replace(
                descripcion ?? "",
                @"<br\s*/?>|</p>",
                "\n",
                RegexOptions.IgnoreCase);

            texto = Regex.Replace(texto, @"<[^>]*>", "");
            return WebUtility.HtmlDecode(texto).Trim();
        }

        private async Task AsegurarCatalogoAlertasExisteAsync(CancellationToken cancellationToken)
        {
            try
            {
                CollectionReference catAlertasRef = _firestoreDb.Collection("cat_alertas");
                QuerySnapshot snapshot = await catAlertasRef.Limit(1).GetSnapshotAsync(cancellationToken);

                // Si ya existen registros, no se realiza ninguna acción
                if (snapshot.Documents.Count > 0)
                {
                    return;
                }

                _logger.LogInformation("La colección 'cat_alertas' no existe o está vacía. Poblando catálogo inicial...");

                var catalogoInicial = new List<CategoriaAlerta>
        {
            new CategoriaAlerta { Id = "1", Categoria = "UR", Nombre = "NUEVO TICKET DE URGENCIA" },
            new CategoriaAlerta { Id = "2", Categoria = "TA", Nombre = "TICKET SIN ATENCION DEL AREA" },
            new CategoriaAlerta { Id = "3", Categoria = "TA", Nombre = "TICKET SIN ATENCION DE COORDINADOR" },
            new CategoriaAlerta { Id = "4", Categoria = "TA", Nombre = "TICKET SIN ATENCION TOTAL" },
            new CategoriaAlerta { Id = "5", Categoria = "TR", Nombre = "SLA SIN RESOLVER" },
            new CategoriaAlerta { Id = "6", Categoria = "TR", Nombre = "SLA VENCIDO TOTAL" },
            new CategoriaAlerta { Id = "7", Categoria = "RR", Nombre = "4 INCIDENCIAS SEGUIDAS" },
            new CategoriaAlerta { Id = "8", Categoria = "MT", Nombre = "NUEVO TICKET EN MITIGACION" },
            new CategoriaAlerta { Id = "9", Categoria = "CM", Nombre = "NUEVO TICKET DE COMITE" }
        };

                // Uso de Batch Write para insertar de forma atómica y eficiente
                WriteBatch batch = _firestoreDb.StartBatch();

                foreach (var item in catalogoInicial)
                {
                    // Usamos el ID de la alerta como ID del documento en Firestore
                    DocumentReference docRef = catAlertasRef.Document(item.Id);
                    batch.Set(docRef, item);
                }

                await batch.CommitAsync(cancellationToken);
                _logger.LogInformation("Catálogo 'cat_alertas' creado exitosamente con {Count} registros.", catalogoInicial.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar o poblar el catálogo 'cat_alertas'.");
            }
        }

        public async Task SendWhatsAppMessageAsync(
    WhatsAppRequestDto dto,
    CancellationToken cancellationToken = default)
        {
            var instance = _configuration["UltraMsg:InstanceID"];
            var token = _configuration["UltraMsg:Token"];

            if (string.IsNullOrWhiteSpace(instance) || string.IsNullOrWhiteSpace(token))
            {
                _logger.LogError("Configuración de UltraMsg incompleta en appsettings.json");
                throw new InvalidOperationException("Configuración de UltraMsg incompleta en appsettings.json");
            }

            var url = $"https://api.ultramsg.com/{instance}/messages/chat";

            var formData = new Dictionary<string, string>
    {
        { "token", token },
        { "to", dto.ToNumber },
        { "body", dto.Message }
    };

            using var content = new FormUrlEncodedContent(formData);

            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = string.IsNullOrWhiteSpace(responseContent)
                    ? $"Error al comunicarse con UltraMsg (HTTP {(int)response.StatusCode})"
                    : responseContent;

                _logger.LogError("Fallo al enviar mensaje WhatsApp a {To}: {Error}", dto.ToNumber, errorMessage);
                throw new HttpRequestException(errorMessage);
            }

            _logger.LogInformation("Mensaje de WhatsApp enviado correctamente a {To}", dto.ToNumber);
        }

    }

    [FirestoreData]
    public class HistorialNotificacion
    {
        [FirestoreProperty]
        public string ticketId { get; set; }

        [FirestoreProperty]
        public DateTime fecha { get; set; }
        
        [FirestoreProperty]
        public string idAlerta { get; set; }
        [FirestoreProperty] 
        public List<string> usuarios { get; set;}

        [FirestoreProperty]
        public Dictionary<string, bool> lecturas { get; set; } = new();

    }

    [FirestoreData]
    public class CategoriaAlerta
    {
        [FirestoreProperty("id")]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty("categoria")]
        public string Categoria { get; set; } = string.Empty;

        [FirestoreProperty("nombre")]
        public string Nombre { get; set; } = string.Empty;
    }
}
