using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsTickets
{
    public partial class ColoresAyc
    {
        public int Id { get; set; }
        public double Precio { get; set; }
        public string Color { get; set; } = null!;
    }
}
