using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsBD2Prueba
{
    public partial class MiscubosolapUsuario
    {
        public int Idusuario { get; set; }
        public int Idcubo { get; set; }

        public virtual Miscubosolap IdcuboNavigation { get; set; } = null!;
    }
}
