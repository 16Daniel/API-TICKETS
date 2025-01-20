using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsTickets
{
    public partial class CatCategoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public int Idarea { get; set; }
    }
}
