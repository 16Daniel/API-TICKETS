using Quartz;

namespace TICKETSAPI.Jobs
{
    public class JobVacio : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            return Task.CompletedTask;
        }
    }
}