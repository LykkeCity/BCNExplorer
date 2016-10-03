using System;
using System.Threading.Tasks;
using Common.Log;
using JobsCommon;
using Microsoft.Azure.WebJobs;

namespace AssetScanner.Functions
{
    public class AssetCreatorFunctions
    {
        private readonly ILog _log;

        public AssetCreatorFunctions(ILog log)
        {
            _log = log;
        }

        public async Task CreateAssets([QueueTrigger(JobsQueues.AddNewAssetsQueueName)] string message, DateTimeOffset insertionTime)
        {
            await _log.WriteInfo("asda", "sadasd", "sdas", $" {message} started {DateTime.Now} ");
            await Task.Delay(10*1000);
        }
    }
}
