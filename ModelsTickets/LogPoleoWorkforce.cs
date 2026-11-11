using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsTickets
{
    public partial class LogPoleoWorkforce
    {
        public int Id { get; set; }
        public int Idf { get; set; }
        public int Iddatastream { get; set; }
        public DateTime Fecha { get; set; }
        public bool Enviado { get; set; }
        public string? Msg { get; set; }
        public long? Time { get; set; }
        public string? Stat { get; set; }
        public string? Type { get; set; }
    }
}
