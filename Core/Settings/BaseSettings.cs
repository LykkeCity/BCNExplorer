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
    }

    public class DbSettings
    {

        public string LogsConnString { get; set; }
    }
}
