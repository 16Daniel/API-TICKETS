using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsTickets
{
    public partial class PedidosDelivery
    {
        public string Idpedido { get; set; } = null!;
        public string App { get; set; } = null!;
        public DateTime Fecha { get; set; }
        public int Idsuc { get; set; }
        public string Jdata { get; set; } = null!;
        public bool Procesado { get; set; }
    }
}
