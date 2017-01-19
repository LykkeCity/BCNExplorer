using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Transaction
{
    public interface ITransaction
    {
        string TransactionId { get;  }
        bool IsCoinBase { get;  }
        bool IsColor { get;  }
        string Hex { get;  }
        double Fees { get; }
        int InputsCount { get;  }
        int OutputsCount { get;  }
        IBlockMinInfo Block { get;  }
        IEnumerable<IInOutsByAsset> TransactionsByAssets { get;  }
    }

    public interface IInOut
    {
        string TransactionId { get;  }
        string Address { get;  }
        int Index { get;  }
        double Value { get;  }
        string AssetId { get;  }
        double Quantity { get;  }
    }

    public interface IBlockMinInfo
    {
        string BlockId { get;  }
        double Height { get;  }
        DateTime Time { get;  }
        double Confirmations { get;  }
    }

    public interface IInOutsByAsset
    {
        bool IsColored { get;  }
        string AssetId { get;  }
        IEnumerable<IInOut> TransactionIn { get;  }
        IEnumerable<IInOut> TransactionsOut { get;  }
    }


    public interface ITransactionService
    {
        Task<ITransaction> GetAsync(string id);
    }
}
