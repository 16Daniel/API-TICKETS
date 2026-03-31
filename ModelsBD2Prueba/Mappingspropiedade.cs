using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsBD2Prueba
{
    public partial class Mappingspropiedade
    {
        public int Idmap { get; set; }
        public string Propiedad { get; set; } = null!;
        public string? Valor { get; set; }

        public virtual Mappingscab IdmapNavigation { get; set; } = null!;
    }
}
