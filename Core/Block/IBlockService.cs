using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Block
{
    public interface IBlockHeader
    {
        string Hash { get;  }
        int Height { get;  }
        DateTime Time { get;  }
        long Confirmations { get;  }
    }

    public interface IBlock
    {
        string Hash { get; }
        long Height { get; }
        DateTime Time { get; }
        long Confirmations { get; }
        double Difficulty { get; }
        string MerkleRoot { get; }
        long Nonce { get; }
        int TotalTransactions { get; }
        string PreviousBlock { get; }
        IEnumerable<string> AllTransactionIds { get; }
        IEnumerable<string> ColoredTransactionIds { get;}
        IEnumerable<string> UncoloredTransactionIds { get;  }
    }

    public interface IBlockService
    {
        Task<IBlockHeader> GetBlockHeaderAsync(string id);
        Task<IBlockHeader> GetLastBlockHeaderAsync();
        Task<IBlock> GetBlockAsync(string id);
    }
}
