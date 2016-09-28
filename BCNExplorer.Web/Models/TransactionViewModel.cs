using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Providers.TransportTypes;
using Providers.TransportTypes.Asset;
using Providers.TransportTypes.Ninja;

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
        
        #region Classes 

        public class BlockViewModel
        {
            public double Confirmations { get; set; }

            public DateTime Time { get; set; }
     
            public double Height { get; set; }
            public string BlockId { get; set; }

            public static BlockViewModel Create(NinjaTransaction.BlockMinInfo blockMinInfo)
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


            public IEnumerable<AggregatedInOut<In>> AggregatedIns { get; set; }
            public IEnumerable<AggregatedInOut<Out>> AggregatedOuts { get; set; }

            public double Fees { get; set; }

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
                IEnumerable<NinjaTransaction.InOut> ninjaIn,
                IEnumerable<NinjaTransaction.InOut> ninjaOuts)
            {
                var ins = In.Create(ninjaIn);
                var outs = Out.Create(ninjaOuts);

                return new BitcoinAsset
                {
                    Fees = BitcoinUtils.SatoshiToBtc(fees),
                    IsCoinBase = isCoinBase,
                    AggregatedIns = AssetHelper.GroupByAddress(ins),
                    AggregatedOuts = AssetHelper.GroupByAddress(outs)
                };
            }

            public class In:AssetInOut
            {
                public string PreviousTransactionId { get; set; }
                public bool IsUnrecoginzedAddress => string.IsNullOrEmpty(Address);
                public static IEnumerable<In> Create(IEnumerable<NinjaTransaction.InOut> ins)
                {
                    return ins.Where(p => p.Value != 0).Select(p => new In
                    {
                        Value = BitcoinUtils.SatoshiToBtc(p.Value * (-1)), 
                        Address = p.Address,
                        PreviousTransactionId = p.TransactionId
                    });
                }
            }

            public class Out:AssetInOut
            {
                public bool IsUnrecoginzedAddress => string.IsNullOrEmpty(Address);
                public static IEnumerable<Out> Create(IEnumerable<NinjaTransaction.InOut> outs)
                {
                    return outs.Where(p => p.Value != 0).Select(p => new Out
                    {
                        Value = BitcoinUtils.SatoshiToBtc(p.Value),
                        Address = p.Address
                    });
                }
            }
        }

        public class ColoredAsset
        {
            public string AssetId { get; set; }
            public int Divisibility { get; set; }
            public string Name { get; set; }
            public string IconImageUrl { get; set; }

            public IEnumerable<AggregatedInOut<In>> AggregatedIns { get; set; }
            public IEnumerable<AggregatedInOut<Out>> AggregatedOuts { get; set; }

            #region Classes

            public class In:AssetInOut
            {
                public string PreviousTransactionId { get; set; }

                public string ShortName { get; set; }
                public bool IsUnrecoginzedAddress => string.IsNullOrEmpty(Address);

                public static In Create(NinjaTransaction.InOut sourceIn, int divisibility,  IEnumerable<NinjaTransaction.InOut> outs, string shortName)
                {
                    var l = outs.Select(itm => itm.Quantity - sourceIn.Quantity);
                    var def = l.FirstOrDefault();

                    return new In
                    {
                        Address = sourceIn.Address,
                        PreviousTransactionId = sourceIn.TransactionId,
                        ShortName = shortName,
                        Value = BitcoinUtils.CalculateColoredAssetQuantity((def != 0 ? def : sourceIn.Quantity), divisibility)
                    };
                }
            }

            public class Out:AssetInOut
            {
                public string ShortName { get; set; }
                public bool IsUnrecoginzedAddress => string.IsNullOrEmpty(Address);

                public static Out Create(NinjaTransaction.InOut sourceOut, int divisibility, string shortName)
                {
                    return new Out
                    {
                        Address = sourceOut.Address,
                        Value = BitcoinUtils.CalculateColoredAssetQuantity(sourceOut.Quantity, divisibility),
                        ShortName = shortName
                    };
                }
            }
            
            #endregion

            public static ColoredAsset Create(NinjaTransaction.InOutsByAsset inOutsByAsset, AssetDictionary assetDictionary)
            {
                var divisibility = assetDictionary.GetAssetProp(inOutsByAsset.AssetId, p => p.Divisibility, 0);
                var assetShortName = assetDictionary.GetAssetProp(inOutsByAsset.AssetId, p => p.NameShort, null);

                var ins = inOutsByAsset.TransactionIn.Select(p => In.Create(p, divisibility, inOutsByAsset.TransactionsOut, assetShortName));
                var outs = inOutsByAsset.TransactionsOut.Select(p => Out.Create(p, divisibility, assetShortName));
                
                var result = new ColoredAsset
                {
                    AssetId = inOutsByAsset.AssetId,
                    Divisibility = divisibility,
                    Name = assetDictionary.GetAssetProp(inOutsByAsset.AssetId, p => p.Name, null) ?? inOutsByAsset.AssetId,
                    IconImageUrl = assetDictionary.GetAssetProp(inOutsByAsset.AssetId, p => p.IconUrl, null),
                    AggregatedIns = AssetHelper.GroupByAddress(ins),
                    AggregatedOuts = AssetHelper.GroupByAddress(outs)
                };

                return result;
            }
        }

        #endregion

        public static TransactionViewModel Create(NinjaTransaction ninjaTransaction, IDictionary<string, AssetDefinition> assetDictionary)
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
                Bitcoin = BitcoinAsset.Create(ninjaTransaction.Fees, ninjaTransaction.IsCoinBase, bc.TransactionIn, bc.TransactionsOut ),
                ColoredAssets = colored.Select(p => ColoredAsset.Create(p, assetDic))
            };
            
            return result;
        }
    }

    public abstract class AssetInOut
    {
        public string Address { get; set; }
        public double Value { get; set; }

        public virtual T Clone<T>() where T:AssetInOut
        {
            return (T)this.MemberwiseClone();
        }
    }

    public class AggregatedInOut<T> where T: AssetInOut
    {
        public T TitleItem { get; set; }

        public IEnumerable<T> AggregatedTransactions { get; set; }

        public bool ShowAggregatedTransactions => AggregatedTransactions?.Count() > 1;

        public int Count => AggregatedTransactions?.Count() ?? 0;

    }

    public static class AssetHelper
    {

        public static IEnumerable<AggregatedInOut<T>> GroupByAddress<T>(IEnumerable<T> source) where T: AssetInOut
        {
            return source
                .GroupBy(p => p.Address)
                .Select(p =>
                {
                    var titleItem = p.First().Clone<T>();
                    titleItem.Value = p.Sum(ti => ti.Value);

                    return new AggregatedInOut<T>
                    {
                        TitleItem = titleItem,
                        AggregatedTransactions = p.ToList()
                    };
                });
        } 
    }
}