using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsCommon
{
    public static class JobsConnectionStringSettings
    {
        public static string ConnectionString => !string.IsNullOrEmpty(ConfigurationManager.AppSettings["ConnectionString"]) 
            ? ConfigurationManager.AppSettings["ConnectionString"] 
            : "UseDevelopmentStorage=true";
    }
}
