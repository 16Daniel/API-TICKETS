using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsBD2Prueba
{
    public partial class Falta
    {
        public string Digitocontrol { get; set; } = null!;
        public int Tipo { get; set; }
        public string Numero { get; set; } = null!;
    }
}
