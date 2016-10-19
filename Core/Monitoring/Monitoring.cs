using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Monitoring
{
    public interface IMonitoringRecord
    {
        string ServiceName { get; }
        DateTime DateTime { get; }
    }

    public class MonitoringRecord : IMonitoringRecord
    {
        public MonitoringRecord(string serviceName, DateTime dateTime)
        {
            ServiceName = serviceName;
            DateTime = dateTime;
        }

        public string ServiceName { get; set; }
        public DateTime DateTime { get; set; }
    }

    public interface IServiceMonitoringRepository
    {
        Task ScanAllAsync(Func<IEnumerable<IMonitoringRecord>, Task> chunk);
        Task UpdateOrCreate(IMonitoringRecord record);
    }
}
