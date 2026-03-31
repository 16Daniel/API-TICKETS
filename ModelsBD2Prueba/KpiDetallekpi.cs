using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsBD2Prueba
{
    public partial class KpiDetallekpi
    {
        public int Idkpi { get; set; }
        public int Iddetalle { get; set; }
        public int Posicion { get; set; }

        public virtual Kpi IdkpiNavigation { get; set; } = null!;
    }
}
