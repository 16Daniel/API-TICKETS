using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsBD2Prueba
{
    public partial class ShowHorario
    {
        public ShowHorario()
        {
            ShowHorariofronts = new HashSet<ShowHorariofront>();
            ShowPresentacionhorarios = new HashSet<ShowPresentacionhorario>();
        }

        public int Idhorario { get; set; }
        public string? Descripcion { get; set; }

        public virtual ICollection<ShowHorariofront> ShowHorariofronts { get; set; }
        public virtual ICollection<ShowPresentacionhorario> ShowPresentacionhorarios { get; set; }
    }
}
