using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsTickets
{
    public partial class ClientesDelivery
    {
        public int Id { get; set; }
        public string Marca { get; set; } = null!;
        public string Plataforma { get; set; } = null!;
        public int Codcliente { get; set; }
        public string? DiseñoTicket { get; set; }
    }
}
