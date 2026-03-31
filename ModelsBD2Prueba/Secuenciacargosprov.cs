using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsBD2Prueba
{
    public partial class Secuenciacargosprov
    {
        public int Codproveedor { get; set; }
        public int Codcargodto { get; set; }
        public int Secuencia { get; set; }

        public virtual Proveedore CodproveedorNavigation { get; set; } = null!;
    }
}
