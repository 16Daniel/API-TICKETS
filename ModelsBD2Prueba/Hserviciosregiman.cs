using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsBD2Prueba
{
    public partial class Hserviciosregiman
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; } = null!;
        public bool Descatalogado { get; set; }
    }
}
