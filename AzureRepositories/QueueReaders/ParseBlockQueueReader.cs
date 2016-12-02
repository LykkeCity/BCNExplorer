using AzureStorage.Queue;
using Common.Log;

namespace AzureRepositories.QueueReaders
{
    public interface IParseBlockQueueReader : IQueueReader
    {

    }

    public class ParseBlockQueueReader : QueueReader, IParseBlockQueueReader
    {
        public ParseBlockQueueReader(IQueueExt queueExt, string componentName, int periodMs, ILog log) : base(queueExt, componentName, periodMs, log)
        {
        }
    }
}
