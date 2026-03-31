using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsTickets
{
    public partial class CatTurno
    {
        public int ClaTurno { get; set; }
        public string Nombre { get; set; } = null!;
        public int? ClaEmpresa { get; set; }
        public string? Alias { get; set; }
    }
}
