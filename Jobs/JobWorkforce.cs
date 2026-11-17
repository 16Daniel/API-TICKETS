using Quartz;
using TICKETSAPI.Repositories;

namespace TICKETSAPI.Jobs
{
    public class JobWorkforce : IJob
    {
        private readonly IWorkforceRepository _workforceRepository;

        public JobWorkforce(IWorkforceRepository workforceRepository)
        {
            _workforceRepository = workforceRepository;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            DateTime fecha = DateTime.Today.AddDays(-1).Date;
            try 
            {
                await _workforceRepository.ProcesarAsync(fecha, context.CancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al ejecutar el JobWorkforce: {ex.Message}");
            }
        }
    }
}