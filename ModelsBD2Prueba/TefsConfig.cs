using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsBD2Prueba
{
    public partial class TefsConfig
    {
        public int Idtef { get; set; }
        public int Idconfig { get; set; }
        public string? Nombre { get; set; }
        public string? Config { get; set; }

        public virtual Tef IdtefNavigation { get; set; } = null!;
    }
}
