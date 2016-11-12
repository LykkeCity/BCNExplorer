using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Asset;
using SQLRepositories.Context;

namespace SQLRepositories.Repositories
{
    public class ParsedAddressBlockRepository: IAssetChangesParsedBlockRepository
    {
        private readonly BcnExplolerFactory _bcnExplolerFactory;

        public ParsedAddressBlockRepository(BcnExplolerFactory bcnExplolerFactory)
        {
            _bcnExplolerFactory = bcnExplolerFactory;
        }

  
        public async Task<int> GetLastParsetBlockHeightAsync()
        {
            using (var db = _bcnExplolerFactory.GetContext())
            {
                return await db.ParsedAddressBlockEntities.MaxAsync(p => p.BlockEntity.Height);
            }
        }
    }
}
