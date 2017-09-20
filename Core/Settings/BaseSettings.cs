using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Settings
{
    public class BaseSettings
    {
        [Required]
        public string NinjaUrl { get; set; }
        [Required]
        public string OffchainNotificationsHandlerUrl { get; set; }
        public string LykkeAPIUrl { get; set; }

        public string Network { get; set; }

        [Required]
        public DbSettings Db { get; set; }

        [Required]
        public JobsSettings Jobs { get; set; }

        [Required]
        public NinjaIndexerCredentials NinjaIndexerCredentials { get; set; }

        public string ExplolerUrl { get; set; }

        public string Secret { get; set; }

        public bool CacheMainChainLocalFile { get; set; }

        public bool DisablePersistentCacheMainChain { get; set; }

        public bool DisableRedirectToHttps { get; set; }

        public bool GetTopFromNinja { get; set; }
    }

    public class DbSettings
    {
        [Required]
        public string LogsConnString { get; set; }

        [Required]
        public string AssetsConnString { get; set; }

        [Required]
        public string SharedStorageConnString { get; set; }
        
        [Required]
        public MongoDbCredentials AssetBalanceChanges { get; set; }
    }

    public class JobsSettings
    {
        public bool IsDebug { get; set; }
    }

    public class NinjaIndexerCredentials
    {
        [Required]
        public string AzureName { get; set; }

        [Required]
        public string AzureKey { get; set; }
    }

    public class MongoDbCredentials
    {
        [Required]
        public string ConnectionString { get; set; }

        [Required]
        public string DbName { get; set; }
    }
    

    public class GeneralSettings
    {
        public BaseSettings BcnExploler { get; set; }
    }
}
