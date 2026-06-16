using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsTickets
{
    public partial class CombosDelivery
    {
        public int Id { get; set; }
        public int Idcombo { get; set; }
        public string Articulos { get; set; } = null!;
        public int? Idmarca { get; set; }
    }
}
