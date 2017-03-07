using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Email;

namespace Core.Settings
{
    public class BaseSettings
    {
        [Required]
        public string NinjaUrl { get; set; }

        [Required]
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

        [Required]
        public EmailGeneratorSettings EmalGeneratorSettings { get; set; }

        [Required]
        public ServiceBusEmailSettings ServiceBusEmailSettings { get; set; }

        [Required]
        public BalanceReportSettings BalanceReportSettings { get; set; }
        //[Required]
        //public AuthenticationSettings Authentication { get; set; }
    }

    public class EmailGeneratorSettings
    {
        [Required]
        public string EmailTemplatesHost { get; set; }
    }

    public class AuthenticationSettings
    {
        [Required]
        public string ClientId { get; set; }

        [Required]
        public string ClientSecret { get; set; }

        [Required]
        public string RedirectUri { get; set; }

        [Required]
        public string PostLogoutRedirectUri { get; set; }

        [Required]
        public string Authority { get; set; }
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
        public AssetBalanceChangesDb AssetBalanceChanges { get; set; }
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

    public class AssetBalanceChangesDb
    {
        [Required]
        public string ConnectionString { get; set; }

        [Required]
        public string DbName { get; set; }
    }

    public class ServiceBusEmailSettings : IServiceBusEmailSettings
    {
        [Required]
        public string NamespaceUrl { get; set; }

        [Required]
        public string PolicyName { get; set; }

        [Required]
        public string Key { get; set; }

        [Required]
        public string QueueName { get; set; }
    }

    public class BalanceReportSettings
    {
        public string[] AssetIds { get; set; }
    }
}
