using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsTickets
{
    public partial class BitacoraPersonal
    {
        public int Id { get; set; }
        public int Idsucursal { get; set; }
        public int Idemp { get; set; }
        public string? Solucion { get; set; }
        public string? Comentariosucursal { get; set; }
        public bool? Status { get; set; }
        public DateTime? Fecha { get; set; }
    }
}
