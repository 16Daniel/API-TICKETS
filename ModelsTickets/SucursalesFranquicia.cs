using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsTickets
{
    public partial class SucursalesFranquicia
    {
        public int Id { get; set; }
        public int Idf { get; set; }
        public string Nombre { get; set; } = null!;
        public string Grupo { get; set; } = null!;
    }
}
