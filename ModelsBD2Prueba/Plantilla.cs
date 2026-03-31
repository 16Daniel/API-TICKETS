using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsBD2Prueba
{
    public partial class Plantilla
    {
        public string Tipoplantilla { get; set; } = null!;
        public string Tipocolumna { get; set; } = null!;
        public string? Titulocolumna { get; set; }
        public string? Descripcion { get; set; }
    }
}
