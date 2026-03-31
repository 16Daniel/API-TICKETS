using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsBD2Prueba
{
    public partial class Tiposreserva
    {
        public int Codigo { get; set; }
        public string? Descripcion { get; set; }
        public bool? Cargos { get; set; }
        public bool Descatalogado { get; set; }
    }
}
