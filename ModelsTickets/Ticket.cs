using System;
using System.Collections.Generic;

namespace TICKETSAPI.ModelsTickets
{
    public partial class Ticket
    {
        public string Idtk { get; set; } = null!;
        public DateTime? Fecha { get; set; }
        public string? Idsuc { get; set; }
        public string? Statussuc { get; set; }
        public string? Idprov { get; set; }
        public string? Idcat { get; set; }
        public string? Descripcion { get; set; }
        public string? Solicitante { get; set; }
        public string? Prioridadsuc { get; set; }
        public string? Prioridadprov { get; set; }
        public string? Status { get; set; }
        public string? Responsable { get; set; }
        public DateTime? Fechafin { get; set; }
        public string? Duracion { get; set; }
        public string? Tiposoporte { get; set; }
        public string? Iduser { get; set; }
        public string? Comentarios { get; set; }
        public string? Nombrecategoria { get; set; }
        public string? Comentariosfinales { get; set; }
    }
}
