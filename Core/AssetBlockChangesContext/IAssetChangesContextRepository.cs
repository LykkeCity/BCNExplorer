using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.AssetChanges
{

    public interface IAssetChangesContext
    {
        int BlockHeight { get; }
        string AssetId { get; }
        string BlockHash { get; }
        string AddressId { get; }
        
    }

    public interface IAssetChangesContextRepository
    {
    }
}
