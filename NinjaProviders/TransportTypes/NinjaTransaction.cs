using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NinjaProviders.Contracts;

namespace NinjaProviders.TransportTypes
{
    public class NinjaTransaction
    {
        public string TransactionId { get; set; }

        public bool IsCoinBase { get; set; }
        public bool IsColor { get; set; }
        public string Hex { get; set; }
        public double Fees { get; set; }
        public BlockMinInfo Block { get; set; }
        public IEnumerable<InOutsByAsset> TransactionsByAssets { get; set; } 
        
        #region Classes 

        public class InOut
        {
            public string TransactionId { get; set; }
            public string Address { get; set; }
            public int Index { get; set; }
            public double Value { get; set; }
            public string AssetId { get; set; }
            public double Quantity { get; set; }

            public static IEnumerable<InOut> Create(IEnumerable<TransactionContract.BitCoinInOutContract> inOut)
            {
                return inOut.Select(x => new InOut()
                {
                    TransactionId = x.TransactionId,
                    Address = x.Address,
                    Index = x.Index,
                    Value = x.Value,
                    AssetId = x.AssetId ?? "",
                    Quantity = x.Quantity
                });
            } 
        }

        public class BlockMinInfo
        {
            public string BlockId { get; set; }
            public double Height { get; set; }
            public DateTime Time { get; set; }
            public double Confirmations { get; set; }

            public static BlockMinInfo Create(TransactionContract.BlockContract blockContract)
            {
                return new BlockMinInfo
                {
                    BlockId = blockContract.BlockId,
                    Confirmations = blockContract.Confirmations,
                    Height = blockContract.Height,
                    Time = blockContract.BlockTime
                };
            }
        }

        public class InOutsByAsset
        {
            public bool IsColored { get; set; }
            public string AssetId { get; set; }
            public IEnumerable<InOut> TransactionIn { get; set; }
            public IEnumerable<InOut> TransactionsOut { get; set; }

            //TODO рефакторить (копипаст со старого проекта)
            public static IEnumerable<InOutsByAsset> Create(
                IEnumerable<TransactionContract.BitCoinInOutContract> transactionsIns,
                IEnumerable<TransactionContract.BitCoinInOutContract> transactionsOut)
            {
                var inputs = InOut.Create(transactionsIns);
                var outputs = InOut.Create(transactionsOut);

                //item1 = input, item2 = output
                var dict = new Dictionary<string, Tuple<IList<InOut>, IList<InOut>>>();

                foreach (var input in inputs)
                {
                    if (!dict.ContainsKey(input.AssetId))
                    {
                        var newTuple = new Tuple<IList<InOut>, IList<InOut>>(new List<InOut>(), new List<InOut>());
                        dict.Add(input.AssetId, newTuple);
                    }

                    dict[input.AssetId].Item1.Add(input);
                }

                foreach (var output in outputs)
                {
                    if (!dict.ContainsKey(output.AssetId))
                    {
                        var newTuple = new Tuple<IList<InOut>, IList<InOut>>(new List<InOut>(), new List<InOut>());
                        dict.Add(output.AssetId, newTuple);
                    }

                    dict[output.AssetId].Item2.Add(output);
                }

                return dict.Select(p => new InOutsByAsset
                {
                    AssetId = p.Key,
                    IsColored = !string.IsNullOrEmpty(p.Key),
                    TransactionIn = p.Value.Item1,
                    TransactionsOut = p.Value.Item2
                });
            }
        }

        #endregion
    }
}
