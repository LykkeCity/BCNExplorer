using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureRepositories.BalanceReport;
using Common;
using Common.IocContainer;

namespace TestConsole.BalanceReport
{
    public static class PDFTasksProducer
    {
        public static async Task Run(IoC container)
        {
            var producer=container.GetObject<SendBalanceReportCommandQueryProducer>();
            Console.WriteLine("Reading File");
            var users = File.ReadAllText("./users.json").DeserializeJson<UserModel[]>();

            var counter = users.Length;
            var semaphore = new SemaphoreSlim(100);
            var tasks = new List<Task>();
            foreach (var userModel in users)
            {
                await semaphore.WaitAsync();

                try
                {
                   tasks.Add(producer.CreaseSendBalanceReportCommandAsync("netsky@bk.ru", userModel.FullName, userModel.Addresses,new DateTime(2016, 12, 31)).ContinueWith(
                       p =>
                       {
                           Console.WriteLine(counter);
                           counter--;
                       }));
                }
                finally
                {
                    semaphore.Release();
                }


            }

            await Task.WhenAll(tasks);

            Console.WriteLine("All done");
            Console.ReadLine();
        }

        public class UserModel
        {
            public string Email { get; set; }

            public string FullName { get; set; }

            public string[] Addresses { get; set; }
        }
    }
}
