using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsBD2Prueba
{
    public partial class IeJerarquia
    {
        public int IdDimension { get; set; }
        public int IdJerarquia { get; set; }
        public string NameJerarquia { get; set; } = null!;
        public string? IdTitulo { get; set; }

        public virtual IeDimensione IdDimensionNavigation { get; set; } = null!;
    }
}
