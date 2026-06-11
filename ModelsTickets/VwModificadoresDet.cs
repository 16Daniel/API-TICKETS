using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsTickets
{
    public partial class VwModificadoresDet
    {
        public int Codmodificador { get; set; }
        public int Codigo { get; set; }
        public string? Descripcion { get; set; }
        public int Esarticulo { get; set; }
        public double? Dosis { get; set; }
        public double? Incprecio { get; set; }
        public int Posicion { get; set; }
        public int Tienemodif { get; set; }
        public double? Udselaboracion { get; set; }
    }
}
