using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsBD2Prueba
{
    public partial class RemVersionesdefront
    {
        public int Idfront { get; set; }
        public int Idtabla { get; set; }
        public long? Versionimp { get; set; }
        public long? Versionexp { get; set; }
    }
}
