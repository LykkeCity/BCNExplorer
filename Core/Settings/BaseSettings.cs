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
        public DbSettings Db { get; set; }

        [Required]
        public JobsSettings Jobs { get; set; }

        [Required]
        public NinjaIndexerCredentials NinjaIndexerCredentials { get; set; }
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
