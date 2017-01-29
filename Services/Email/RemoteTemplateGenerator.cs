using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.HttpRemoteRequests;
using Core.Email;
using Core.Settings;

namespace Services.Email
{
    public class RemoteTemplateGenerator : ITemplateGenerator
    {
        private readonly EmailGeneratorSettings _emailGeneratorSettings;
        private readonly HttpRequestClient _httpRequestClient;
        public RemoteTemplateGenerator(BaseSettings baseSettings, HttpRequestClient httpRequestClient)
        {
            _emailGeneratorSettings = baseSettings.EmalGeneratorSettings;
            _httpRequestClient = httpRequestClient;
        }

        public async Task<string> GenerateAsync<T>(string templateName, T templateVm)
        {

            Uri baseUri = new Uri(_emailGeneratorSettings.EmailTemplatesHost);
            Uri templateUri = new Uri(baseUri, templateName + ".html");

            var emailTemplate = GetEmailTemplate(templateUri.AbsoluteUri);

            var emailTemplateWithData = InsertData(await emailTemplate, templateVm);

            return emailTemplateWithData;
        }

        private async Task<string> GetEmailTemplate(string emailTemplateUri)
        {
            return await _httpRequestClient.GetRequest(emailTemplateUri);
        }

        private string InsertData<T>(string emailTemplate, T templateVm)
        {
            StringBuilder sb = new StringBuilder(emailTemplate);

            foreach (var prop in templateVm.GetType().GetProperties())
            {
                // in the email template, placeholders look like this: @[propertyName]
                if (prop.GetValue(templateVm, null) != null)
                    sb.Replace("@[" + prop.Name + "]", prop.GetValue(templateVm, null).ToString());
            }

            return sb.ToString();
        }
    }
}
