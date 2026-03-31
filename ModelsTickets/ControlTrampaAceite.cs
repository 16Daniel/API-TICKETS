using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsTickets
{
    public partial class ControlTrampaAceite
    {
        public int Id { get; set; }
        public int IdSucursal { get; set; }
        public DateTime Fecha { get; set; }
        public double EntregaCedis { get; set; }
        public double? EntregaSucursal { get; set; }
        public double? Porcentaje75 { get; set; }
        public int? Intercambio { get; set; }
        public string? Diferencia { get; set; }
        public string? ComentariosCedis { get; set; }
        public string? ComentariosSucursal { get; set; }
        public int? Status { get; set; }
        public bool? Manual { get; set; }
        public DateTime? Fecharecoleccion { get; set; }
    }
}
