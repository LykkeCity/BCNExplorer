using System;
using System.Threading.Tasks;
using Core.Monitoring;
using Microsoft.Azure.WebJobs;

namespace AssetDefinitionScanner.TimerFunctions
{
    public class SendMonitorData
    {
        private readonly IServiceMonitoringRepository _serviceMonitoringRepository;

        public SendMonitorData(IServiceMonitoringRepository serviceMonitoringRepository)
        {
            _serviceMonitoringRepository = serviceMonitoringRepository;
        }

        public async Task SendMonitorRecord([TimerTrigger("00:00:30", RunOnStartup = true)] TimerInfo timer)
        {
            await
                _serviceMonitoringRepository.UpdateOrCreate(new MonitoringRecord("BCNExpoler.AssetScanner", DateTime.UtcNow));
        }
    }
}
