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
    }

    public class DbSettings
    {
        public string LogsConnString { get; set; }
        public string AssetsConnString { get; set; }
        public string GrabConnString { get; set; }
    }

    public class JobsSettings
    {
        public bool IsDebug { get; set; }
        public int MaxGrabTransactionAttemptCount { get; set; }
    }
}
