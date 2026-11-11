using TICKETSAPI.ModelsTickets;

namespace TICKETSAPI.Repositories
{
    public interface IWorkforceRepository
    {
        Task ProcesarAsync(DateTime fecha, CancellationToken cancellationToken = default);
    }
}
