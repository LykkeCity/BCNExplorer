using System;
using System.Linq;
using System.Threading.Tasks;
using AzureRepositories.AssetDefinition;
using AzureStorage.Tables;
using Common;
using Common.IocContainer;
using Core.Settings;
using Microsoft.WindowsAzure.Storage.Table;
using NBitcoin.OpenAsset;
using Providers;
using Providers.Helpers;
using Services.MainChain;

namespace TestConsole.AssetScanner
{
    public class AddAssetDefinitionsHistory
    {
        public static async Task Run(IoC container)
        {
            Console.WriteLine("AddAssetDefinitionsHistory started");

            var baseSettings = container.GetObject<BaseSettings>();
            var mainChainRepo = container.GetObject<MainChainService>();
            var indexerClient = container.GetObject<IndexerClientFactory>().GetIndexerClient();
            AssetDataCommandProducer assetDataCommandProducer = container.GetObject<AssetDataCommandProducer>();

            var okParsedBlockRepo = new AzureTableStorage<Block>(baseSettings.Db.AssetsConnString, "OkBlock", null);
            var failedParsedBlockRepo = new AzureTableStorage<Block>(baseSettings.Db.AssetsConnString, "FailedBlock", null);
            var defUrlRepo = new AzureTableStorage<DefinitionUrl>(baseSettings.Db.AssetsConnString, "DefinitionUrl", null);

            var mainChain = await mainChainRepo.GetMainChainAsync();
            
            var fromBlock = okParsedBlockRepo.GetData().Select(p => p.Height).DefaultIfEmpty(274250).Max();
            var toBlock = 439808;

            var heightEnumerable = Enumerable.Range(fromBlock, toBlock - fromBlock);
            
            foreach (var h in heightEnumerable)
            {
                try
                {
                    Console.WriteLine(h);

                    var block = indexerClient.GetBlock(mainChain.GetBlock(h).Header.GetHash());
                    foreach (var transaction in block.Transactions.Where(p => p.HasValidColoredMarker()))
                    {
                        var assetDefUrl = transaction.TryGetAssetDefinitionUrl();

                        if (assetDefUrl != null)
                        {
                            await assetDataCommandProducer.CreateUpdateAssetDataCommand(assetDefUrl.AbsoluteUri);
                            await defUrlRepo.InsertOrReplaceAsync(DefinitionUrl.Create(assetDefUrl.AbsoluteUri));

                            Console.WriteLine("Add {0}", assetDefUrl.AbsoluteUri);
                        }
                    }

                    await okParsedBlockRepo.InsertOrReplaceAsync(Block.Create(h));
                }
                catch (Exception e)
                {
                    await failedParsedBlockRepo.InsertOrReplaceAsync(Block.Create(h, e.Message));
                    throw;
                }
            }

            Console.WriteLine("AddAssetDefinitionsHistory done");
            Console.ReadLine();
        }
    }

    public class Block : TableEntity
    {
        public int Height { get; set; }
        public string ExMessage { get; set; }

        public static Block Create(int height, string exMessage = null)
        {
            return new Block
            {
                PartitionKey = "Block",
                RowKey = height.ToString(),
                Height = height,
                ExMessage = exMessage
            };
        }
    }

    public class DefinitionUrl : TableEntity
    {
        public string Url { get; set; }

        public static DefinitionUrl Create(string url)
        {
            return new DefinitionUrl
            {
                PartitionKey = "DU",
                RowKey = url.ToBase64(),
                Url = url
            };
        }
    }
}
