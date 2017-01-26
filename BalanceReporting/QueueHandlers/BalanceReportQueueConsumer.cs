using System;
using System.Threading.Tasks;
using AzureRepositories.AssetCoinHolders;
using AzureRepositories.BalanceReport;
using AzureRepositories.QueueReaders;
using AzureStorage.Queue;
using Common;
using Common.Log;
using Core.Settings;
using Providers;
using Providers.Helpers;
using Services.BalanceChanges;
using Services.MainChain;

namespace BalanceReporting.QueueHandlers
{
    public class BalanceReportQueueConsumer : IStarter
    {
        private readonly IBalanceReportingQueueReader _queueReader;
        private readonly ILog _log;
        private readonly IndexerClientFactory _indexerClient;
        private readonly MainChainService _mainChainService;
        
        public BalanceReportQueueConsumer(ILog log,
            IBalanceReportingQueueReader queueReader,
            IndexerClientFactory indexerClient, 
            MainChainService mainChainService)
        {
            _log = log;
            _queueReader = queueReader;
            _indexerClient = indexerClient;
            _mainChainService = mainChainService;

            _queueReader.RegisterPreHandler(async data =>
            {
                if (data == null)
                {
                    await _log.WriteInfo("BalanceReportQueueConsumer", "InitQueues", null, "Queue had unknown message");
                    return false;
                }
                return true;
            });

            _queueReader.RegisterHandler<QueueRequestModel<SendBalanceReportCommand>>(
                SendBalanceReportCommand.Id, itm => SendBalanceReport(itm.Data));
        }

        private async Task SendBalanceReport(SendBalanceReportCommand context)
        {
            await _log.WriteInfo("BalanceReportQueueConsumer", "SendBalanceReport", context.ToJson(), "Started");
            try
            {

                await _log.WriteInfo("BalanceReportQueueConsumer", "SendBalanceReport", context.ToJson(), "Done");
            }
            catch (Exception e)
            {
                await
                    _log.WriteError("BalanceReportQueueConsumer", "SendBalanceReport", context.ToJson(), e);
                throw;
            }
        }
        

        public void Start()
        {
            _queueReader.Start();
            _log.WriteInfo("BalanceReportQueueConsumer", "Start", null,
                $"Started:{_queueReader.GetComponentName()}");
        }
    }
}
