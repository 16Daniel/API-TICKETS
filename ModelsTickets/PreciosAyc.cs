using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsTickets
{
    public partial class PreciosAyc
    {
        public int Id { get; set; }
        public int Ids { get; set; }
        public int CLunes { get; set; }
        public int CMartes { get; set; }
        public int CMiercoles { get; set; }
        public int CJueves { get; set; }
        public int CViernes { get; set; }
        public int CSabado { get; set; }
        public int CDomingo { get; set; }
        public string Grupo { get; set; } = null!;
    }
}
