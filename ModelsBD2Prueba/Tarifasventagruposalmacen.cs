using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsBD2Prueba
{
    public partial class Tarifasventagruposalmacen
    {
        public int Idtarifav { get; set; }
        public int Idgrupo { get; set; }

        public virtual Tarifasventum IdtarifavNavigation { get; set; } = null!;
    }
}
