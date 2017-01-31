using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
using Providers;
using Providers.Helpers;
using Providers.Providers.Lykke.API;
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
        private readonly IReportRenderer _reportRenderer;
        private readonly IAddressService _addressService;
        private readonly IAssetService _assetService;
        private readonly IBlockService _blockService;
        private readonly IEmailSender _emailSender;
        private readonly ITemplateGenerator _templateGenerator;
        private readonly LykkeAPIProvider _lykkeApiProvider;
        private readonly FiatRatesService _fiatRatesService;

        public BalanceReportQueueConsumer(ILog log,
            IBalanceReportingQueueReader queueReader,
            IndexerClientFactory indexerClient, 
            MainChainService mainChainService, 
            IReportRenderer reportRenderer, 
            IAddressService addressService, 
            IAssetService assetService, 
            IBlockService blockService, 
            IEmailSender emailSender, 
            ITemplateGenerator templateGenerator, 
            LykkeAPIProvider lykkeApiProvider, 
            FiatRatesService fiatRatesService)
        {
            _log = log;
            _queueReader = queueReader;
            _indexerClient = indexerClient;
            _mainChainService = mainChainService;
            _reportRenderer = reportRenderer;
            _addressService = addressService;
            _assetService = assetService;
            _blockService = blockService;
            _emailSender = emailSender;
            _templateGenerator = templateGenerator;
            _lykkeApiProvider = lykkeApiProvider;
            _fiatRatesService = fiatRatesService;

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
                if (context.Addresses == null || context.Addresses.Length == 0)
                {
                    await _log.WriteInfo("BalanceReportQueueConsumer", "SendBalanceReport", context.ToJson(), "No addresses found");
                    return;
                }

                var currencies = new[] { "USD", "CHF", "EUR", "GBP" };

                var assetsToTrack =  await _lykkeApiProvider.GetAssetsAsync();

                var mainChain = await _mainChainService.GetMainChainAsync();
                var at = mainChain.GetClosestToTimeBlock(context.ReportingDate);
                var blockHeader = await _blockService.GetBlockHeaderAsync(at.Height.ToString());

                #region ClientBalance

                var clientBalance = ClientBalance.Create();
                foreach (var addressId in context.Addresses)
                {

                    var ninjaBalance = await _addressService.GetBalanceAsync(addressId, blockHeader.Height);
                    clientBalance.Add(ninjaBalance, assetsToTrack.Select(x => x.BitcoinAssetId));
                }

                #endregion

                var fiatPrices = await _fiatRatesService.GetRatesAsync(blockHeader.Time, currencies, clientBalance.Assets);

                var assetDefinitionDictionary = await _assetService.GetAssetDefinitionDictionaryAsync();
                
                var attachments = new List<EmailAttachment>();
                foreach (var fiatRate in fiatPrices)
                {
                    using (var strm = new MemoryStream())
                    {
                        _reportRenderer.RenderBalance(strm,
                           Client.Create(context.Email, context.ClientName),
                           blockHeader,
                           fiatRate,
                           clientBalance,
                           assetDefinitionDictionary);

                        strm.Position = 0;
                        attachments.Add(new EmailAttachment
                        {
                            FileName = "BalanceReport-" + fiatRate.CurrencyName + ".pdf",
                            ContentType = "application/pdf",
                            Data = strm.ToArray()
                        });
                    }
                }

                var mes = new EmailMessage
                {
                    Subject = BalanceReportingTemplateModel.EmailSubject,
                    Body = await _templateGenerator.GenerateAsync(BalanceReportingTemplateModel.TemplateName, BalanceReportingTemplateModel.Create(blockHeader.Time, context.ClientName)),
                    IsHtml = true,
                    Attachments = attachments.ToArray()
                };

                await _emailSender.SendEmailAsync(context.Email, mes);

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
