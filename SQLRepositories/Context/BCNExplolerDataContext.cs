using System.Data.Entity;
using SQLRepositories.DbModels;

namespace SQLRepositories.Context
{
    public class BcnExplolerDataContext:DbContext
    {
        public BcnExplolerDataContext(string connectionString):base(connectionString)
        {
            
        }

        public DbSet<AddressEntity> Addresses { get; set; }
        public DbSet<TransactionEntity> Transactions { get; set; } 
        public DbSet<BlockEntity> Blocks { get; set; } 
        public DbSet<BalanceChangeEntity> BalanceChanges { get; set; } 
        public DbSet<ParsedAddressBlockEntity> ParsedAddressBlockEntities { get; set; } 
    }
}
