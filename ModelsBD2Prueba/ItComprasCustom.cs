using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsBD2Prueba
{
    public partial class ItComprasCustom
    {
        public string Uuid { get; set; } = null!;
        public int Codarticulo { get; set; }
        public decimal Uds { get; set; }
        public DateTime? Fecha { get; set; }
    }
}
