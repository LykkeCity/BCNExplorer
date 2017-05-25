using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Block
{
    public interface ICachedBlockService
    {
        Task<IBlock> GetBlockAsync(string id);
    }
}
