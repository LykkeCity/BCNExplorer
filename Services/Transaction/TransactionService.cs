using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Transaction;
using NBitcoin.OpenAsset;
using Providers.Providers.Ninja;

namespace Services.Transaction
{
    public class Transaction : ITransaction
    {
        public string TransactionId { get; set; }

        public bool IsCoinBase { get; set; }
        public bool IsColor { get; set; }
        public string Hex { get; set; }
        public double Fees { get; set; }
        public IBlockMinInfo Block { get; set; }
        public IEnumerable<IInOutsByAsset> TransactionsByAssets { get; set; }

        #region Classes 

        public class InOut : IInOut
        {
            public string TransactionId { get; set; }
            public string Address { get; set; }
            public int Index { get; set; }
            public double Value { get; set; }
            public string AssetId { get; set; }
            public double Quantity { get; set; }

            public static IEnumerable<InOut> Create(IEnumerable<NinjaTransaction.InOut> inOut)
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

        public class BlockMinInfo : IBlockMinInfo
        {
            public string BlockId { get; set; }
            public double Height { get; set; }
            public DateTime Time { get; set; }
            public double Confirmations { get; set; }

            public static BlockMinInfo Create(NinjaTransaction.BlockMinInfo blockContract)
            {
                if (blockContract != null)
                {
                    return new BlockMinInfo
                    {
                        BlockId = blockContract.BlockId,
                        Confirmations = blockContract.Confirmations,
                        Height = blockContract.Height,
                        Time = blockContract.BlockTime
                    };
                }

                return null;
            }

        }

        public class InOutsByAsset : IInOutsByAsset
        {
            public bool IsColored { get; set; }
            public string AssetId { get; set; }
            public IEnumerable<IInOut> TransactionIn { get; set; }
            public IEnumerable<IInOut> TransactionsOut { get; set; }

            //TODO рефакторить (копипаст со старого проекта)
            public static IEnumerable<IInOutsByAsset> Create(
                IEnumerable<NinjaTransaction.InOut> transactionsIns,
                IEnumerable<NinjaTransaction.InOut> transactionsOut)
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

    public class TransactionService : ITransactionService
    {
        private readonly NinjaTransactionProvider _ninjaTransactionProvider;

        public TransactionService(NinjaTransactionProvider ninjaTransactionProvider)
        {
            _ninjaTransactionProvider = ninjaTransactionProvider;
        }

        public async Task<ITransaction> GetAsync(string id, bool calculateInputsWithReturnedChange = true)
        {
            var responce = await _ninjaTransactionProvider.GetAsync(id, calculateInputsWithReturnedChange);
            var transactionInfo = NBitcoin.Transaction.Parse(responce.Hex);

            var inputs = responce.Inputs.ToList();
            var outputs = responce.Outputs.ToList();

            #region CalculateInputsWithReturnedChange

            if (calculateInputsWithReturnedChange)
            {
                foreach (var input in inputs.Where(inp => outputs.Any(x => x.Address == inp.Address)))
                {
                    foreach (var output in outputs.Where(x => x.Address == input.Address && x.AssetId == input.AssetId).ToList())
                    {
                        input.Value -= output.Value;
                        input.Quantity -= output.Quantity;

                        outputs.Remove(output);
                    }
                }
            }

            #endregion

            var result = new Transaction
            {
                TransactionId = responce.TransactionId,
                Hex = transactionInfo.ToHex(),
                IsCoinBase = transactionInfo.IsCoinBase,
                IsColor = transactionInfo.HasValidColoredMarker(),
                Block = Transaction.BlockMinInfo.Create(responce.Block),
                Fees = responce.Fees,
                TransactionsByAssets = Transaction.InOutsByAsset.Create(inputs, outputs)
            };

            return result;
        }
    }
}
