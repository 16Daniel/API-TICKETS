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
             
                CollectionReference ticketsRef = _firestoreDb.Collection("tickets");
                Google.Cloud.Firestore.Query query = ticketsRef
                  //  .WhereEqualTo("idArea", "1")
                 .WhereNotIn("idEstatusTicket", new[] { "3" });

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

                    if (!document.TryGetValue<string>("idArea", out string idArea))
                    {
                        idArea = string.Empty;
                    }

                    if (!document.TryGetValue<string>("idSucursal", out string idSucursal))
                    {
                        idSucursal = string.Empty;
                    }


                    if (!document.TryGetValue<string>("folio", out string folio))
                    {
                        folio = string.Empty;
                    }

                    if (idresponsable != "") { involucrados.Add(idresponsable); }

                    var organigrama = await ObtenerOrganigramaPorAreaAsync(idArea, context.CancellationToken);
                    string idRespdireccion = "";
                    string idRespgerente = "";

                    if (organigrama?.Raiz?.Children != null && organigrama.Raiz.Children.Count > 0)
                    {
                        OrganigramaNodo direccion = organigrama.Raiz;
                        idRespdireccion = direccion.PersonaId ?? "";

                        if (direccion.Children != null && direccion.Children.Count > 0)
                        {
                            OrganigramaNodo gerente = direccion.Children[0];
                            idRespgerente = gerente.PersonaId ?? "";
                        }
                    }

                    string iddireccion = await ObtenerIdUsuarioPorIdResponsableAsync(idRespdireccion, context.CancellationToken);
                    string idgerente = await ObtenerIdUsuarioPorIdResponsableAsync(idRespgerente, context.CancellationToken);

                    string idAlerta;
                    var usuariosAlerta = involucrados.Distinct().ToList();

                    double horasTranscurridasCreacion = ObtenerHorasTranscurridas(fechacreacion);
                    double horasTranscurridasAtencion = ObtenerHorasTranscurridas(fechaAtencion);

                    if (statusTicket == "1"
                        && horasTranscurridasCreacion >= (horasAtencion - 0.25)
                        && horasTranscurridasCreacion < horasAtencion)
                    {
                        idAlerta = "WTA";
                        usuariosAlerta.Clear();
                    }
                    else if (statusTicket == "1"
                        && horasTranscurridasCreacion < (horasAtencion + 24)
                        && horasTranscurridasCreacion > horasAtencion)
                    {
                        idAlerta = "2";
                        if (!string.IsNullOrWhiteSpace(idgerente)) usuariosAlerta.Add(idgerente);
                    }
                    else if (statusTicket == "1" && horasTranscurridasCreacion > (horasAtencion + 24))
                    {
                        idAlerta = "4";
                        if (!string.IsNullOrWhiteSpace(idgerente)) usuariosAlerta.Add(idgerente);
                        if (!string.IsNullOrWhiteSpace(iddireccion)) usuariosAlerta.Add(iddireccion);
                    }
                    else if (int.TryParse(statusTicket, out int estado)
                        && estado > 1
                        && estado != 8
                        && horasTranscurridasAtencion >= (horasResolucion - 0.25)
                        && horasTranscurridasAtencion < horasResolucion)
                    {
                        idAlerta = "WTR";
                        usuariosAlerta.Clear();
                    }
                    else if (int.TryParse(statusTicket, out estado)
                        && estado > 1
                        && estado != 8
                        && horasTranscurridasAtencion < (horasResolucion + 24)
                        && horasTranscurridasAtencion > horasResolucion)
                    {
                        idAlerta = "5";
                        if (!string.IsNullOrWhiteSpace(idgerente)) usuariosAlerta.Add(idgerente);
                    }
                    else if (int.TryParse(statusTicket, out estado)
                        && estado > 1
                        && estado != 8
                        && horasTranscurridasAtencion > (horasResolucion + 24))
                    {
                        idAlerta = "6";
                        if (!string.IsNullOrWhiteSpace(idgerente)) usuariosAlerta.Add(idgerente);
                        if (!string.IsNullOrWhiteSpace(iddireccion)) usuariosAlerta.Add(iddireccion);
                    }
                    else
                    {
                        continue;
                    }

                    usuariosAlerta = usuariosAlerta.Distinct().ToList();

                    await _notificationService.ProcesarAlertaAsync(new TicketNotificationRequest
                    {
                        TicketId = ticketId,
                        Folio = folio,
                        IdSucursal = idSucursal,
                        IdAlerta = idAlerta,
                        Usuarios = usuariosAlerta
                    }, context.CancellationToken);

                    if (idAlerta is "2" or "5" or "WTA" or "WTR")
                        if (idAlerta is "WTA" or "WTR")
                    {
                        int opcionNotificacion = idAlerta switch
                        {
                            "2" => 1,
                            "5" => 2,
                            "WTA" => 3,
                            "WTR" => 4,
                            _ => 0
                        };

                        var grupo = await ObtenerGrupoNotificacionAreaAsync(
                            _firestoreDb,
                            ticketId,
                            opcionNotificacion,
                            context.CancellationToken);

                        if (!string.IsNullOrWhiteSpace(grupo))
                        {
                            string mensaje = await GenerarMensajeWAsync(
                                _firestoreDb,
                                ticketId,
                                opcionNotificacion);
                            await SendWhatsAppMessageAsync(new WhatsAppRequestDto { ToNumber = grupo, Message = mensaje });
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

        public async Task<string> ObtenerIdUsuarioPorIdResponsableAsync(
            string idResponsable,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(idResponsable))
                return string.Empty;

            try
            {
                Google.Cloud.Firestore.Query query = _firestoreDb
                    .Collection("responsables")
                    .WhereEqualTo("id", idResponsable)
                    .Limit(1);

                QuerySnapshot snapshot = await query.GetSnapshotAsync(cancellationToken);

                if (snapshot.Documents.Count == 0)
                {
                    _logger.LogWarning(
                        "No se encontró el responsable con ID: {IdResponsable}",
                        idResponsable);
                    return string.Empty;
                }

                DocumentSnapshot responsable = snapshot.Documents[0];

                if (responsable.TryGetValue<string>("idUsuario", out string idUsuario) &&
                    !string.IsNullOrWhiteSpace(idUsuario))
                {
                    return idUsuario;
                }

                _logger.LogWarning(
                    "El responsable {IdResponsable} no contiene idUsuario",
                    idResponsable);
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al obtener el idUsuario del responsable {IdResponsable}",
                    idResponsable);
                return string.Empty;
            }
        }

        public async Task<AreaOrganigrama?> ObtenerOrganigramaPorAreaAsync(
            string areaId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(areaId))
                return null;

            try
            {
                Google.Cloud.Firestore.Query query = _firestoreDb
                    .Collection("areas_organigrama")
                    .WhereEqualTo("areaId", areaId)
                    .Limit(1);

                QuerySnapshot snapshot = await query.GetSnapshotAsync(cancellationToken);

                if (snapshot.Documents.Count == 0)
                {
                    _logger.LogWarning("No se encontró el organigrama del área {AreaId}", areaId);
                    return null;
                }

                return snapshot.Documents[0].ConvertTo<AreaOrganigrama>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar el organigrama del área {AreaId}", areaId);
                return null;
            }
        }

        public static async Task<string> GenerarMensajeWAsync(
            FirestoreDb db,
            string ticketId,
            int opcionNotificacion)
        {
            ArgumentNullException.ThrowIfNull(db);

            string titulo = opcionNotificacion switch
            {
                1 => "🔴 TA: SUPERADO",
                2 => "🔴 TR: SUPERADO",
                3 => "🔴 TA: POR VENCER",
                4 => "🔴 TR: POR VENCER",
                _ => throw new ArgumentOutOfRangeException(
                    nameof(opcionNotificacion),
                    "Debe ser 1 (superaTA), 2 (superaTR), 3 (taPorVencer) o 4 (trPorVencer).")
            };

            if (string.IsNullOrWhiteSpace(ticketId))
                throw new ArgumentException("El ID del ticket es requerido.", nameof(ticketId));

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
            titulo,
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
            int opcionNotificacion,
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

            var opcion = opcionNotificacion switch
            {
                1 => "superaTA",
                2 => "superaTR",
                3 => "taPorVencer",
                4 => "trPorVencer",
                _ => null
            };

            if (opcion == null)
                return null;

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
    public class CategoriaAlerta
    {
        [FirestoreProperty("id")]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty("categoria")]
        public string Categoria { get; set; } = string.Empty;

        [FirestoreProperty("nombre")]
        public string Nombre { get; set; } = string.Empty;
    }

    [FirestoreData]
    public class AreaOrganigrama
    {
        [FirestoreProperty("areaId")]
        public string AreaId { get; set; } = string.Empty;

        [FirestoreProperty("raiz")]
        public OrganigramaNodo? Raiz { get; set; }
    }

    [FirestoreData]
    public class OrganigramaNodo
    {
        [FirestoreProperty("key")]
        public string Key { get; set; } = string.Empty;

        [FirestoreProperty("id")]
        public int Id { get; set; }

        [FirestoreProperty("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [FirestoreProperty("personaId")]
        public string PersonaId { get; set; } = string.Empty;

        [FirestoreProperty("nombreResponsable")]
        public string NombreResponsable { get; set; } = string.Empty;

        [FirestoreProperty("children")]
        public List<OrganigramaNodo> Children { get; set; } = new List<OrganigramaNodo>();
    }
}
