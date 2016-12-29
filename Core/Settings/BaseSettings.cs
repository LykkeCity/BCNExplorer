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
        

        public string Network { get; set; }

        [Required]
        public DbSettings Db { get; set; }

        [Required]
        public JobsSettings Jobs { get; set; }

        [Required]
        public NinjaIndexerCredentials NinjaIndexerCredentials { get; set; }

        //[Required]
        //public AuthenticationSettings Authentication { get; set; }
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

        public string SqlConnString { get; set; }

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
}
