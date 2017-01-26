using System;
using System.Threading.Tasks;
using AzureStorage.Queue;
using Common.Log;

namespace AzureRepositories.BalanceReport
{
    public class SendBalanceReportCommand
    {
        public const string Id = "SendBalanceReportCommand";
        
        public string Email { get; set; }

        public string Address { get; set; }

        public DateTime ReportingDate { get; set; }

        public string Currency { get; set; }
    }


    public class SendBalanceReportCommandQueryProducer
    {
        private readonly IQueueExt _queueExt;
        private readonly ILog _log;

        public SendBalanceReportCommandQueryProducer(IQueueExt queueExt, ILog log)
        {
            _queueExt = queueExt;
            _log = log;

            _queueExt.RegisterTypes(QueueType.Create(SendBalanceReportCommand.Id, typeof(QueueRequestModel<SendBalanceReportCommand>)));
        }


        public async Task CreaseSendBalanceReportCommandAsync(string email, string address, DateTime reportingDatetime, string currency)
        {
            await _queueExt.PutMessageAsync(new QueueRequestModel<SendBalanceReportCommand>
            {
                Data = new SendBalanceReportCommand
                {
                    Email = email,
                    Address = address,
                    ReportingDate = reportingDatetime,
                    Currency = currency
                }
            });
        }
    }

}

