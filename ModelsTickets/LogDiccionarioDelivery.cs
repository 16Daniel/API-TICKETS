using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsTickets
{
    public partial class LogDiccionarioDelivery
    {
        public int Id { get; set; }
        public string Articulo { get; set; } = null!;
        public string Marca { get; set; } = null!;
        public string Plataforma { get; set; } = null!;
        public string? Modificador { get; set; }
        public bool Procesado { get; set; }
        public string? Sucursal { get; set; }
        public string? Idpedido { get; set; }
        public string? Jsonpedido { get; set; }
    }
}
