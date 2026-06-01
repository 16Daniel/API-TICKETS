using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsTickets
{
    public partial class VentaFranquicia
    {
        public int Id { get; set; }
        public string Sucursal { get; set; } = null!;
        public double? VentaSalon { get; set; }
        public double? VentaDelivery { get; set; }
        public double VentaTotal { get; set; }
        public DateTime? Fecha { get; set; }
        public int? Cobros { get; set; }
        public int? Refiles { get; set; }
    }
}
