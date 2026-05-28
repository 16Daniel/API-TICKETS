using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsTickets
{
    public partial class DiccionarioDelivery
    {
        public int Id { get; set; }
        public string Tienda { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public int Codicg { get; set; }
        public bool Esmodificador { get; set; }
        public int? Codmodificador { get; set; }
    }
}
