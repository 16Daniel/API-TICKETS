using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsBD2Prueba
{
    public partial class Icgcentralitaerr
    {
        public int Idlog { get; set; }
        public DateTime? Fecha { get; set; }
        public DateTime? Hora { get; set; }
        public string? Cadena { get; set; }
        public string? Error { get; set; }
    }
}
