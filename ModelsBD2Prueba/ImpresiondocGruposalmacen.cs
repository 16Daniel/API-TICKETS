using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsBD2Prueba
{
    public partial class ImpresiondocGruposalmacen
    {
        public int Grupo { get; set; }
        public int Id { get; set; }
        public int Codgrupoalmacen { get; set; }

        public virtual Impresiondoc Impresiondoc { get; set; } = null!;
    }
}
