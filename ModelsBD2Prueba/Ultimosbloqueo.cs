using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsBD2Prueba
{
    public partial class Ultimosbloqueo
    {
        public int Idfront { get; set; }
        public string Terminal { get; set; } = null!;
        public int? Idbloqueo { get; set; }
    }
}
