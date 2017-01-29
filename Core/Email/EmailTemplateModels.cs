using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Email
{
    public class BalanceReportingTemplateModel
    {
        public const string EmailSubject = "Lykke Digital Asset Portfolio Report";
        public const string TemplateName = "ReportTemplate";

        public string Year { get; set; }

        public string ReportDate { get; set; }

        public string ClientName { get; set; }

        public static BalanceReportingTemplateModel Create(DateTime reportingDate, string clientName)
        {
            return new BalanceReportingTemplateModel
            {
                ClientName = clientName,
                ReportDate = reportingDate.ToString("dd  MMMM yyyy"),
                Year = DateTime.Now.Year.ToString()
            };
        }
    }
}
