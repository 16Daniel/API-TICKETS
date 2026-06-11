using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsTickets
{
    public partial class VwModificadore
    {
        public int Codarticulo { get; set; }
        public int? Posicion { get; set; }
        public int Codmodificador { get; set; }
        public int? Limite { get; set; }
        public int? Orden { get; set; }
        public string? Multiselec { get; set; }
        public short? Minimo { get; set; }
        public bool? Auto { get; set; }
        public int? Gratis { get; set; }
        public string? Descripcion { get; set; }
    }
}
