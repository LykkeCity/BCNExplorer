using AzureStorage.Queue;
using Common.Log;

namespace AzureRepositories.QueueReaders
{
    public interface IBalanceReportingQueueReader : IQueueReader
    {

    }

    public class BalanceReportingQueueReader : QueueReader, IBalanceReportingQueueReader
    {
        public BalanceReportingQueueReader(IQueueExt queueExt, string componentName, int periodMs, ILog log) : base(queueExt, componentName, periodMs, log)
        {
        }
    }
}
