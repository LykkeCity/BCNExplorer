using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Settings
{
    public class BaseSettings
    {
        public string NinjaUrl { get; set; }
        public DbSettings Db { get; set; }
        public JobsSettings Jobs { get; set; }
        public NinjaIndexerCredentials NinjaIndexerCredentials { get; set; }
    }

    public class DbSettings
    {
        public string LogsConnString { get; set; }
        public string AssetsConnString { get; set; }
    }

    public class JobsSettings
    {
        public bool IsDebug { get; set; }
    }

    public class NinjaIndexerCredentials
    {
        public string AzureName { get; set; }
        public string AzureKey { get; set; }
    }
}
