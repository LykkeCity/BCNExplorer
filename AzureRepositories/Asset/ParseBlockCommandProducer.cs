using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage.Queue;
using Common.Log;

namespace AzureRepositories.Asset
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

    public class ParseBlockContext
    {
        public const string Id = "AssetParseBlockDataContext";

        public string BlockHash { get; set; }
    }

    public class ParseBlockCommandProducer
    {
        private readonly IQueueExt _queueExt;

        public ParseBlockCommandProducer(IQueueExt queueExt)
        {
            _queueExt = queueExt;

            _queueExt.RegisterTypes(QueueType.Create(ParseBlockContext.Id, typeof(QueueRequestModel<ParseBlockContext>)));
        }

        public async Task CreateParseBlockCommand(params string[] blockHashs)
        {
            var putInQueryTask = new List<Task>();
            
            foreach (var blockHash in blockHashs)
            {
                putInQueryTask.Add(_queueExt.PutMessageAsync(new QueueRequestModel<ParseBlockContext>
                {
                    Data = new ParseBlockContext
                    {
                        BlockHash = blockHash
                    }
                }));
            }

            await Task.WhenAll(putInQueryTask);
        } 
    }
}
