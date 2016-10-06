using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureStorage.Queue;

namespace AzureRepositories.Asset
{
    public class UpdateAssetDataCommandProducer
    {
        private readonly IQueueExt _queueExt;

        public UpdateAssetDataCommandProducer(IQueueExt queueExt)
        {
            _queueExt = queueExt;
        }

        public Task CreateUpdateAssetDataCommand(string url)
        {
            return _queueExt.PutMessageAsync(url);
        } 
    }
}
