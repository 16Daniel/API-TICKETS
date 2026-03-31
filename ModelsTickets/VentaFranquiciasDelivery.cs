using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsTickets
{
    public partial class VentaFranquiciasDelivery
    {
        public int Id { get; set; }
        public string Sucursal { get; set; } = null!;
        public DateTime Fecha { get; set; }
        public double Uber { get; set; }
        public double Rappi { get; set; }
        public double Didi { get; set; }
    }
}
