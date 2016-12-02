using AzureStorage.Queue;
using Common.Log;

namespace AzureRepositories.QueueReaders
{
    public interface ICoinholderIndexesQueueReader : IQueueReader
    {

    }

    public class CoinholderIndexesQueueReader : QueueReader, ICoinholderIndexesQueueReader
    {
        public CoinholderIndexesQueueReader(IQueueExt queueExt, string componentName, int periodMs, ILog log) : base(queueExt, componentName, periodMs, log)
        {
        }
    }
}
