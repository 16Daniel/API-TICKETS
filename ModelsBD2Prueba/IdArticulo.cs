using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsBD2Prueba
{
    public partial class IdArticulo
    {
        public int Codarticulo { get; set; }
        public Guid Guidarticulo { get; set; }
        public int? Newcodarticulo { get; set; }
        public Guid? Newguidarticulo { get; set; }
        public int? Quienunifica { get; set; }
        public DateTime? Fechaunifica { get; set; }
    }
}
