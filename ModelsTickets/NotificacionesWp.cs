using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsTickets
{
    public partial class NotificacionesWp
    {
        public int Id { get; set; }
        public string Idticket { get; set; } = null!;
        public DateTime Fecha { get; set; }
        public int Nivel { get; set; }
        public string TipoNotificacion { get; set; } = null!;
    }
}
