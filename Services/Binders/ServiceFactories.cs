using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Log;
using Core.Settings;
using Lykke.EmailSenderProducer;
using Services.Email;

namespace Services.Binders
{
    public static class ServiceFactories
    {
        public static EmailSenderProducer CreateEmailSenderProducer(BaseSettings baseSettings, ILog log)
        {
            return new EmailSenderProducer(baseSettings.ServiceBusEmailSettings, log);
        }
    }
}
