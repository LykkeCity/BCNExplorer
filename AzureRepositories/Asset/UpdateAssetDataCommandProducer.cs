using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureStorage.Queue;

namespace AzureRepositories.Asset
{
    public class CreateAssetDataContext
    {
        public const string Id = "CreateAssetDataContext";
        public string AssetDefinitionUrl { get; set; }
    }

    public class AssetDataCommandProducer
    {
        private readonly IQueueExt _queueExt;

        public AssetDataCommandProducer(IQueueExt queueExt)
        {
            _queueExt = queueExt;

            _queueExt.RegisterTypes(QueueType.Create(CreateAssetDataContext.Id, typeof(QueueRequestModel<CreateAssetDataContext>)));
        }

        public Task CreateUpdateAssetDataCommand(string url)
        {
            return _queueExt.PutMessageAsync(new QueueRequestModel<CreateAssetDataContext>
            {
                Data = new CreateAssetDataContext
                {
                    AssetDefinitionUrl = url
                }
            });
        } 
    }
}
