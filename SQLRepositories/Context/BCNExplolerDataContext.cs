using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using SQLRepositories.DbModels;

namespace SQLRepositories.Context
{
    public class BcnExplolerDataContext:DbContext
    {
        public BcnExplolerDataContext(string connectionString):base(connectionString)
        {
            
        }

        public DbSet<AddressEntity> Addresses { get; set; }
    }
}
