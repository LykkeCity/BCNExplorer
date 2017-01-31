using System.IO;
using System.Threading.Tasks;

namespace Core.Email
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, EmailMessage emailMessage, string sender = null);
    }

    public class EmailAttachment
    {
        public string ContentType { get; set; }
        public string FileName { get; set; }
        public byte[] Data { get; set; }
    }

    public class EmailMessage
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsHtml { get; set; }
        public EmailAttachment[] Attachments { get; set; }
    }

    public interface IServiceBusEmailSettings
    {
        string NamespaceUrl { get; set; }
        string PolicyName { get; set; }
        string Key { get; set; }
        string QueueName { get; set; }
    }
}
