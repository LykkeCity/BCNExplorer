using System.Threading.Tasks;
using AzureRepositories.Asset;
using AzureStorage.Queue;
using Common;
using Common.Log;

namespace AssetScanner.QueueHandlers
{
    public class ParseBlockCommandQueueConsumer : IStarter
    {
        private readonly IParseBlockQueueReader _queueReader;
        private readonly ILog _log;

        public ParseBlockCommandQueueConsumer(ILog log, IParseBlockQueueReader queueReader)
        {
            _log = log;
            _queueReader = queueReader;

            _queueReader.RegisterPreHandler(async data =>
            {
                if (data == null)
                {
                    await _log.WriteInfo("ParseBlockCommandQueueConsumer", "InitQueues", null, "Queue had unknown message");
                    return false;
                }
                return true;
            });

            _queueReader.RegisterHandler<QueueRequestModel<ParseBlockContext>>(
                ParseBlockContext.Id, itm => ParseBlock(itm.Data));
        }

        private async Task ParseBlock(ParseBlockContext context)
        {
            await
                _log.WriteInfo("ParseBlockCommandQueueConsumer", "ParseBlock", context.ToJson(), "Parse block started");
        }
        

        public void Start()
        {
            _queueReader.Start();
            _log.WriteInfo("ParseBlockCommandQueueConsumer", "Start", null,
                $"Started:{_queueReader.GetComponentName()}");
        }
    }
}
