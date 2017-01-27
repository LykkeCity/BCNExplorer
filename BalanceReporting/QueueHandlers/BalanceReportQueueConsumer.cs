using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureRepositories.AssetCoinHolders;
using AzureRepositories.BalanceReport;
using AzureRepositories.QueueReaders;
using AzureStorage.Queue;
using Common;
using Common.Log;
using Core.AddressService;
using Core.Asset;
using Core.BalanceReport;
using Core.Block;
using Core.Email;
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
        private readonly IReportRender _reportRender;
        private readonly IAddressService _addressService;
        private readonly IAssetService _assetService;
        private readonly IBlockService _blockService;
        private readonly IEmailSender _emailSender;

        public BalanceReportQueueConsumer(ILog log,
            IBalanceReportingQueueReader queueReader,
            IndexerClientFactory indexerClient, 
            MainChainService mainChainService, 
            IReportRender reportRender, 
            IAddressService addressService, 
            IAssetService assetService, 
            IBlockService blockService, 
            IEmailSender emailSender)
        {
            _log = log;
            _queueReader = queueReader;
            _indexerClient = indexerClient;
            _mainChainService = mainChainService;
            _reportRender = reportRender;
            _addressService = addressService;
            _assetService = assetService;
            _blockService = blockService;
            _emailSender = emailSender;

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
                var assetsToTrack = new[]
                {
                    "AWm6LaxuJgUQqJ372qeiUxXhxRWTXfpzog",
                    "AXkedGbAH1XGDpAypVzA5eyjegX4FaCnvM",
                    "AYeENupK7A9LZ5BsQiXnp22tHHquoASsFc",
                    "AJPMQpygd8V9UCAxwFYYHYXLHJ7dUkQJ5w",
                    "ASzmrSxhHjioWMYivoawap9yY4cxAfAMxR",
                    "AKi5F8zPm7Vn1FhLqQhvLdoWNvWqtwEaig",
                    "Ab8mNRBmrPJCmghHDoMsq26GP5vxm7hZpP"
                };

                var fiatPrices = FiatPrice.Create(context.Currency, new Dictionary<string, decimal>
                {
                    {"AJPMQpygd8V9UCAxwFYYHYXLHJ7dUkQJ5w", 0.981345m },//chf
                    {"ASzmrSxhHjioWMYivoawap9yY4cxAfAMxR", 1.05204m },//eur
                    {"AKi5F8zPm7Vn1FhLqQhvLdoWNvWqtwEaig", 1.23412m },//gbp
                    {"Ab8mNRBmrPJCmghHDoMsq26GP5vxm7hZpP", 0.008546m}, //jpy
                    {"AWm6LaxuJgUQqJ372qeiUxXhxRWTXfpzog", 1 },//usd
                    {"BTC", 945.492m },//btc
                    {"AYeENupK7A9LZ5BsQiXnp22tHHquoASsFc", 0.07967449m }//solar
                });

                var mainChain = await _mainChainService.GetMainChainAsync();
                var at = mainChain.GetClosestToTimeBlock(context.ReportingDate);
                var blockHeader = await _blockService.GetBlockHeaderAsync(at.Height.ToString());

                var ninjaBalance = await _addressService.GetBalanceAsync(context.Address, blockHeader.Height);

                var balances = new List<AssetBalance>();
                balances.Add(new AssetBalance
                {
                    AssetId = "BTC",
                    Quantity = Convert.ToDecimal(BitcoinUtils.SatoshiToBtc(ninjaBalance.Balance))
                });

                foreach (var assetBalance in ninjaBalance.ColoredBalances.Where(p => assetsToTrack.Contains(p.AssetId)))
                {
                    balances.Add(new AssetBalance
                    {
                        AssetId = assetBalance.AssetId,
                        Quantity = Convert.ToDecimal(assetBalance.Quantity)
                    });
                }

                var assetDic = await _assetService.GetAssetDefinitionDictionaryAsync();
                
                using (var strm = new MemoryStream())
                {
                    _reportRender.RenderBalance(strm,
                        Client.Create(context.Email, context.Address),
                        blockHeader,
                        fiatPrices,
                        balances,
                        assetDic);
                    
                    var mes = new EmailMessage
                    {
                        Subject = "Lykke Digital Asset Portfolio Report",
                        Body = " ",
                        Attachments = new[]
                        {
                            new EmailAttachment
                            {
                               FileName = "BalanceReport.pdf",
                               ContentType = "application/pdf",
                               Stream = strm 
                            }
                        }
                    };

                    await _emailSender.SendEmailAsync(context.Email, mes);
                }

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
