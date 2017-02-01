using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage.Queue;
using Common.Log;

namespace AzureRepositories.BalanceReport
{
    public class SendBalanceReportCommand
    {
        public const string Id = "SendBalanceReportCommand";
        
        public string Email { get; set; }

        public string[] Addresses { get; set; }

        public DateTime ReportingDate { get; set; }

        public string ClientName { get; set; }

        public static SendBalanceReportCommand Create(string email, string clientName, IEnumerable<string> addresses, DateTime reportingDatetime)
        {
            return new SendBalanceReportCommand
            {
                Email = email,
                Addresses = addresses?.ToArray(),
                ReportingDate = reportingDatetime,
                ClientName = clientName
            };
        }
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


        public async Task CreaseSendBalanceReportCommandAsync(string email, string clientName, IEnumerable<string> addresses, DateTime reportingDatetime)
        {
            await _queueExt.PutMessageAsync(new QueueRequestModel<SendBalanceReportCommand>
            {
                Data = SendBalanceReportCommand.Create(email, clientName, addresses, reportingDatetime)
            });
        }
    }

}

