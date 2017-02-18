using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Core.Asset;
using Core.Transaction;

namespace BCNExplorer.Web.Models
{
    public class TransactionViewModel
    {
        public string TransactionId { get; set; }
        public bool IsColor { get; set; }
        public bool IsCoinBase { get; set; }
        public int AssetsCount { get; set; }
        public bool IsConfirmed { get; set; }
        public BlockViewModel Block { get; set; }
        public BitcoinAsset Bitcoin { get; set; }
        public IEnumerable<ColoredAsset> ColoredAssets { get; set; }
        public int InputsCount { get; set; }
        public int OutputsCount { get; set; }
        
        #region Classes 

        public class BlockViewModel
        {
            public double Confirmations { get; set; }

            public DateTime Time { get; set; }
     
            public double Height { get; set; }
            public string BlockId { get; set; }

            public static BlockViewModel Create(IBlockMinInfo blockMinInfo)
            {
                if (blockMinInfo != null)
                {
                    return new BlockViewModel
                    {
                        Confirmations = blockMinInfo.Confirmations,
                        Height = blockMinInfo.Height,
                        Time = blockMinInfo.Time,
                        BlockId = blockMinInfo.BlockId
                    };
                }

                return null;
            }
        }

        public class BitcoinAsset
        {
            public bool IsCoinBase { get; set; }
            
            public IEnumerable<AggregatedInOut<AssetInOutBase>> AggregatedIns { get; set; }
            public IEnumerable<AggregatedInOut<AssetInOutBase>> AggregatedOuts { get; set; }

            public IEnumerable<AggregatedInOut<AssetInOutBase>> AggregatedInsWithoutChange { get; set; }
            public IEnumerable<AggregatedInOut<AssetInOutBase>> AggregatedOutsWithoutChange { get; set; }

            public double Fees { get; set; }
            public string FeesDescription => Fees.ToStringBtcFormat();

            public double ReleasedFromColorValue => ColoredEquivalentValue;
            public bool ShowReleasedFromColor => ColoredEquivalentValue < 0;

            public double ConsumedForColorValue => ColoredEquivalentValue;

            public double ColoredEquivalentValue { get; set; }
            public bool ShowConsumedForColor => ColoredEquivalentValue > 0;

            public double Total { get; set; }

            public bool ShowWithoutChange { get; set; }

            public static IEnumerable<In> Group(IEnumerable<In> source)
            {
                var groupedByAddress = source.GroupBy(p => p.Address);
                return groupedByAddress.Select(p =>
                {
                    var result = p.First();
                    result.Value = p.Sum(x => x.Value);

                    return result;
                });
            } 

            public static BitcoinAsset Create(double fees,
                bool isCoinBase,
                IEnumerable<IInOut> ninjaIn,
                IEnumerable<IInOut> ninjaOuts,
                AssetDictionary assetDictionary)
            {
                var feesBtc = BitcoinUtils.SatoshiToBtc(fees);

                IEnumerable<AssetInOutBase> ins = In.Create(ninjaIn, assetDictionary).ToList();
                IEnumerable<AssetInOutBase> outs = Out.Create(ninjaOuts, assetDictionary).ToList();

                IEnumerable<AssetInOutBase> insWithoutChange = ins.Select(p => p.Clone<In>()).ToList();
                IEnumerable<AssetInOutBase> outsWithoutChange = outs.Select(p => p.Clone<Out>()).ToList();

                decimal releasedFromColor = 0;
                ins = AssetHelper.RemoveColored(ins, out releasedFromColor).ToList();

                decimal consumedForColor = 0;
                outs = AssetHelper.RemoveColored(outs, out consumedForColor).ToList();
                

                bool showChange = false;
                AssetHelper.CalculateWithReturnedChange(ins, outs, ref showChange);

                if (ins.All(p => p.Value == 0) && outs.All(p => p.Value == 0))
                {
                    ins = ins.Take(1).ToList();
                    outs = outs.Take(1).ToList();
                }
                else
                {
                    ins = ins.Where(p => p.Value != 0).ToList();
                    outs = outs.Where(p => p.Value != 0).ToList();
                }

                return new BitcoinAsset
                {
                    Fees = feesBtc,
                    IsCoinBase = isCoinBase,
                    AggregatedIns = AssetHelper.GroupByAddress(ins),
                    AggregatedOuts = AssetHelper.GroupByAddress(outs),
                    AggregatedInsWithoutChange = AssetHelper.GroupByAddress(insWithoutChange),
                    AggregatedOutsWithoutChange = AssetHelper.GroupByAddress(outsWithoutChange),
                    Total = outs.Sum(p => p.Value) + feesBtc,
                    ShowWithoutChange = showChange || consumedForColor != 0 || releasedFromColor !=0,
                    ColoredEquivalentValue = Convert.ToDouble(consumedForColor + releasedFromColor)
                };
            }

            public class In:AssetInOutBase
            {
                public In(double value, string address, string previousTransactionId, double coloredEquivalentQuantityQuantity, AssetViewModel coloredEquivalentAsset)
                {
                    Value = value;
                    Address = address;
                    PreviousTransactionId = previousTransactionId;
                    ColoredEquivalentQuantity = coloredEquivalentQuantityQuantity;
                    ColoredEquivalentAsset = coloredEquivalentAsset;
                }
                

                public override string PreviousTransactionId { get; }

                public override string ColoredEquivalentDescription
                    => FormatColoredEquivalent(ColoredEquivalentQuantity, ColoredEquivalentAsset);
                public override string ValueDescription => Value.ToStringBtcFormat();
                public override string AssetDescription => "";

                public override int AggregatedTransactionCount => _aggregatedTransactionCount;
                
                public static IEnumerable<In> Create(IEnumerable<IInOut> ins, AssetDictionary assetDictionary)
                {
                    return ins.Where(p => p.Value != 0)
                        .Select(p => new In(
                            value: BitcoinUtils.SatoshiToBtc(p.Value * (-1)), 
                            coloredEquivalentAsset:assetDictionary.Get(p.AssetId),
                            coloredEquivalentQuantityQuantity:p.Quantity,
                            address:p.Address, 
                            previousTransactionId:p.TransactionId));
                }
            }

            public class Out:AssetInOutBase
            {
                public Out(double value, string address, double coloredEquivalentQuantity, AssetViewModel coloredEquivalentAsset)
                {
                    Value = value;
                    Address = address;
                    ColoredEquivalentQuantity = coloredEquivalentQuantity;
                    ColoredEquivalentAsset = coloredEquivalentAsset;
                }
                
                public override string PreviousTransactionId => null;

                public override string ColoredEquivalentDescription => FormatColoredEquivalent(ColoredEquivalentQuantity, ColoredEquivalentAsset);
                public override string ValueDescription => Value.ToStringBtcFormat();
                public override string AssetDescription => "";

                public override int AggregatedTransactionCount => _aggregatedTransactionCount;
                
                public static IEnumerable<Out> Create(IEnumerable<IInOut> outs, AssetDictionary assetDictionary)
                {
                    return outs.Where(p => p.Value != 0).Select(p=> Create(p, assetDictionary.Get(p.AssetId)));
                }

                public static Out Create(IInOut @out, AssetViewModel coloredEquivalentAsset)
                {
                    return new Out(value: BitcoinUtils.SatoshiToBtc(@out.Value), address: @out.Address, coloredEquivalentAsset:coloredEquivalentAsset, coloredEquivalentQuantity: @out.Quantity);
                }
            }

            public static string FormatColoredEquivalent(double quantity, AssetViewModel assetViewModel)
            {
                var divisibility = assetViewModel?.Divisibility ?? 0;
                var nameShort = assetViewModel?.NameShort;

                return $"{nameShort} {BitcoinUtils.CalculateColoredAssetQuantity(quantity, divisibility).ToStringBtcFormat()}";
            }
        }

        public class ColoredAsset
        {
            public string AssetId { get; set; }
            public int Divisibility { get; set; }
            public string Name { get; set; }
            public string ShortName { get; set; }
            public string IconImageUrl { get; set; }
            public bool IsDestroyed => !AggregatedOuts.Any();
            public bool IsIssued => !AggregatedInsWithoutChange.Any();
            public bool IsKnown { get; set; }
            public double Total { get; set; }

            public double IssedQuantity => (-1) * AggregatedOuts.SelectMany(p => p.AggregatedTransactions).Sum(p => p.Value);
            public double DestroyedQuantity => (-1) * AggregatedIns.SelectMany(p => p.AggregatedTransactions).Sum(p => p.Value);
            public IEnumerable<AggregatedInOut<In>> AggregatedIns { get; set; }
            public IEnumerable<AggregatedInOut<Out>> AggregatedOuts { get; set; }

            public IEnumerable<AggregatedInOut<In>> AggregatedInsWithoutChange { get; set; }
            public IEnumerable<AggregatedInOut<Out>> AggregatedOutsWithoutChange { get; set; }

            public bool ShowWithoutChange { get; set; }

            #region Classes

            public class In:AssetInOutBase
            {
                public In(double value, string address, string previousTransactionId, string shortName)
                {
                    Value = value;
                    Address = address;
                    PreviousTransactionId = previousTransactionId;
                    ShortName = shortName;
                }

                public override string PreviousTransactionId { get; }

                public override string ColoredEquivalentDescription
                {
                    get { throw new InvalidOperationException(); }
                }

                public override string ValueDescription => Value.ToStringBtcFormat();
                public override string AssetDescription => ShortName;

                public override int AggregatedTransactionCount => _aggregatedTransactionCount;

                public string ShortName { get; set; }

                public static In Create(IInOut sourceIn, int divisibility,  IEnumerable<IInOut> outs, string shortName)
                {
                    return new In(
                        value: BitcoinUtils.CalculateColoredAssetQuantity(sourceIn.Quantity *(-1), divisibility), 
                        address: sourceIn.Address, 
                        previousTransactionId: sourceIn.TransactionId,
                        shortName: shortName);
                }
            }

            public class Out:AssetInOutBase
            {
                public Out(double value, string address, string shortName)
                {
                    Value = value;
                    Address = address;
                    ShortName = shortName;
                }

                public override string PreviousTransactionId => null;

                public override string ColoredEquivalentDescription
                {
                    get
                    {
                        throw new InvalidOperationException();
                    }
                }

                public override string ValueDescription => Value.ToStringBtcFormat();
                public override string AssetDescription => ShortName;


                public override int AggregatedTransactionCount => _aggregatedTransactionCount;
                

                public string ShortName { get; set; }

                public static Out Create(IInOut sourceOut, int divisibility, string shortName)
                {
                    return new Out(
                        value: BitcoinUtils.CalculateColoredAssetQuantity(sourceOut.Quantity, divisibility), 
                        address: sourceOut.Address,
                        shortName: shortName);
                }

                public static Out Create(In @in, string shortName)
                {
                    return new Out(
                        value: @in.Value,
                        address: @in.Address,
                        shortName: shortName);
                }
            }
            
            #endregion

            public static ColoredAsset Create(IInOutsByAsset inOutsByAsset, AssetDictionary assetDictionary)
            {
                var divisibility = assetDictionary.GetAssetProp(inOutsByAsset.AssetId, p => p.Divisibility, 0);
                var assetShortName = assetDictionary.GetAssetProp(inOutsByAsset.AssetId, p => p.NameShort, null);

                var ins = inOutsByAsset.TransactionIn.Select(p => In.Create(p, divisibility, inOutsByAsset.TransactionsOut, assetShortName)).ToList();
                var outs = inOutsByAsset.TransactionsOut.Select(p => Out.Create(p, divisibility, assetShortName)).ToList();
                
                var insWithoutChange = ins.Select(p => p.Clone<In>()).ToList();
                var outsWithoutChange = outs.Select(p => p.Clone<Out>()).ToList();
                var showChange = false;

                AssetHelper.CalculateWithReturnedChange(ins, outs, ref showChange);
                
                if (ins.All(p => p.Value == 0) && outs.All(p => p.Value == 0))
                {
                    ins = ins.Take(1).ToList();
                    outs = outs.Take(1).ToList();
                }
                else
                {
                    ins = ins.Where(p => p.Value != 0).ToList();
                    outs = outs.Where(p => p.Value != 0).ToList();
                }

                var total = outs.Any() ? outs.Sum(p => p.Value) : ins.Sum(p => p.Value);

                var result = new ColoredAsset
                {
                    AssetId = inOutsByAsset.AssetId,
                    Divisibility = divisibility,
                    Name = assetDictionary.GetAssetProp(inOutsByAsset.AssetId, p => p.Name, inOutsByAsset.AssetId),
                    IconImageUrl = assetDictionary.GetAssetProp(inOutsByAsset.AssetId, p => p.IconUrl, null),

                    AggregatedIns = AssetHelper.GroupByAddress(ins),
                    AggregatedOuts = AssetHelper.GroupByAddress(outs),

                    AggregatedInsWithoutChange = AssetHelper.GroupByAddress(insWithoutChange),
                    AggregatedOutsWithoutChange = AssetHelper.GroupByAddress(outsWithoutChange),

                    ShowWithoutChange = showChange,

                    ShortName = assetDictionary.GetAssetProp(inOutsByAsset.AssetId, p => p.NameShort, null),
                    Total = total,
                    IsKnown = assetDictionary.Dic.ContainsKey(inOutsByAsset.AssetId)
                };

                return result;
            }
        }

        #endregion

        public static TransactionViewModel Create(ITransaction ninjaTransaction, IDictionary<string, IAssetDefinition> assetDictionary)
        {
            var bc = ninjaTransaction.TransactionsByAssets.First(p => !p.IsColored);
            var colored = ninjaTransaction.TransactionsByAssets.Where(p => p.IsColored).OrderBy(p => p.AssetId);
            var assetDic = AssetDictionary.Create(assetDictionary);

            var result = new TransactionViewModel
            {
                TransactionId = ninjaTransaction.TransactionId,
                IsCoinBase = ninjaTransaction.IsCoinBase,
                IsColor = ninjaTransaction.IsColor,
                Block = BlockViewModel.Create(ninjaTransaction.Block),
                AssetsCount = ninjaTransaction.TransactionsByAssets.Count(p => p.IsColored),
                IsConfirmed = ninjaTransaction.Block != null,
                Bitcoin = BitcoinAsset.Create(ninjaTransaction.Fees, ninjaTransaction.IsCoinBase, bc.TransactionIn.Union(colored.SelectMany(p => p.TransactionIn)), bc.TransactionsOut.Union(colored.SelectMany(p=>p.TransactionsOut)), assetDic ),
                ColoredAssets = colored.Select(p => ColoredAsset.Create(p, assetDic)),
                InputsCount = ninjaTransaction.InputsCount,
                OutputsCount = ninjaTransaction.OutputsCount
            };
            
            return result;
        }
    }

    public abstract class AssetInOutBase: IInOutViewModel
    {
        public string Address { get; set; }
        public double Value { get; set; }

        public abstract string PreviousTransactionId { get; }
        public bool ShowPreviousTransaction => PreviousTransactionId != null;
        public bool HasColoredEquivalent => ColoredEquivalentQuantity != 0;
        public AssetViewModel ColoredEquivalentAsset { get; set; }
        public abstract string ColoredEquivalentDescription { get; }
        public double ColoredEquivalentQuantity { get; set; }
        public abstract string ValueDescription { get; }
        public abstract string AssetDescription { get; }
        public bool ShowAggregatedTransactions => AggregatedTransactionCount > 1;
        public abstract int AggregatedTransactionCount { get; }

        protected int _aggregatedTransactionCount;
        public virtual void SetAggregatedTransactionCount(int count)
        {
            _aggregatedTransactionCount = count;
        }

        public bool IsUnrecoginzedAddress => string.IsNullOrEmpty(Address);

        public virtual T Clone<T>() where T:IInOutViewModel
        {
            var result = (T)this.MemberwiseClone();
            return result;
        }
    }

    public class AggregatedInOut<T> where T: AssetInOutBase
    {
        public T TitleItem { get; set; }

        public IEnumerable<T> AggregatedTransactions { get; set; }

        public bool ShowAggregatedTransactions => AggregatedTransactions?.Count() > 1;

        public int Count => AggregatedTransactions?.Count() ?? 0;
    }

    public static class AssetHelper
    {
        public static IEnumerable<AggregatedInOut<T>> GroupByAddress<T>(IEnumerable<T> source) where T: AssetInOutBase
        {
            return source
                .GroupBy(p => p.Address)
                .Select(p =>
                {
                    var titleItem = p.First().Clone<T>();
                    
                    var allItems = p.ToList();

                    //floation point summary hack
                    titleItem.Value = Convert.ToDouble(p.Sum(ti => Convert.ToDecimal(ti.Value)));

                    titleItem.SetAggregatedTransactionCount(allItems.Count);

                    var result = new AggregatedInOut<T>
                    {
                        TitleItem = titleItem,
                        AggregatedTransactions = allItems
                    };

                    if (result.ShowAggregatedTransactions)
                    {
                        titleItem.ColoredEquivalentQuantity = 0;
                    }

                    return result;
                });
        }

        public static void CalculateWithReturnedChange(IEnumerable<AssetInOutBase> ins,
            IEnumerable<AssetInOutBase> outs, ref bool showChange)
        {
            foreach (var input in ins.Where(inp => outs.Any(x => x.Address == inp.Address)).ToList())
            {
                foreach (var output in outs.Where(x => x.Address == input.Address).OrderBy(p => p.Value).ToList())
                {
                    showChange = true;

                    var exactInput = Convert.ToDecimal(input.Value);
                    var exactOutput = Convert.ToDecimal(output.Value);

                    if (Math.Abs(exactInput) >= Math.Abs(exactOutput))
                    {
                        exactInput = exactInput + exactOutput;
                        exactOutput = 0;
                    }
                    else
                    {
                        exactOutput = exactInput + exactOutput;
                        exactInput = 0;
                    }

                    input.Value = Convert.ToDouble(exactInput);
                    output.Value = Convert.ToDouble(exactOutput);
                }
            }
        }

        public static IEnumerable<AssetInOutBase> RemoveColored(IEnumerable<AssetInOutBase> assetInOuts, out decimal totalColoredBtc)
        {
            totalColoredBtc = assetInOuts.Where(p => p.HasColoredEquivalent).Sum(p => Convert.ToDecimal(p.Value));

            return assetInOuts.Where(p => !p.HasColoredEquivalent).ToList();
        }
    }

    public interface IInOutViewModel
    {
        bool HasColoredEquivalent { get; }
        AssetViewModel ColoredEquivalentAsset { get; set; }
        string ColoredEquivalentDescription { get; }
        string ValueDescription { get; } 
        string AssetDescription { get; }
        bool ShowAggregatedTransactions { get;  }
        int AggregatedTransactionCount { get;  }
        bool IsUnrecoginzedAddress { get; }
        string Address { get; }
        bool ShowPreviousTransaction { get; }
        string PreviousTransactionId { get; }
    }
}