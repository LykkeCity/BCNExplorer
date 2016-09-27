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
            public IEnumerable<In> Ins { get; set; }
            public IEnumerable<Out> Outs { get; set; }
            public decimal Fees { get; set; }

            public static BitcoinAsset Create(double fees,
                bool isCoinBase,
                IEnumerable<NinjaTransaction.InOut> ins,
                IEnumerable<NinjaTransaction.InOut> outs)
            {
                return new BitcoinAsset
                {
                    Fees = BitcoinUtils.SatoshiToBtc(fees),
                    IsCoinBase = isCoinBase,
                    Ins = In.Create(ins),
                    Outs = Out.Create(outs)
                };
            }

            public class In
            {
                public decimal Value { get; set; }
                public string Address { get; set; }
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

            public class Out
            {
                public decimal Value { get; set; }
                public string Address { get; set; }
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
            public string ShortName { get; set; }
            public string Name { get; set; }
            public string IconImageUrl { get; set; }

            public IEnumerable<In> Ins { get; set; }
            public IEnumerable<Out> Outs { get; set; }

            #region Classes

            public class In
            {
                public string PreviousTransactionId { get; set; }
                public double Quantity { get; set; }
                public string Address { get; set; }
                public bool IsUnrecoginzedAddress => string.IsNullOrEmpty(Address);

                public static In Create(NinjaTransaction.InOut sourceIn, int divisibility, IEnumerable<NinjaTransaction.InOut> outs )
                {
                    var l = outs.Select(itm => itm.Quantity - sourceIn.Quantity);
                    var def = l.FirstOrDefault();

                    return new In
                    {
                        Address = sourceIn.Address,
                        PreviousTransactionId = sourceIn.TransactionId,
                        Quantity = BitcoinUtils.CalculateColoredAssetQuantity((def != 0 ? def : sourceIn.Quantity), divisibility)
                    };
                }
            }

            public class Out
            {
                public double Quantity { get; set; }
                public string Address { get; set; }
                public bool IsUnrecoginzedAddress => string.IsNullOrEmpty(Address);

                public static Out Create(NinjaTransaction.InOut sourceOut, int divisibility)
                {
                    return new Out
                    {
                        Address = sourceOut.Address,
                        Quantity = BitcoinUtils.CalculateColoredAssetQuantity(sourceOut.Quantity, divisibility)
                    };
                }
            }
            
            #endregion

            public static ColoredAsset Create(NinjaTransaction.InOutsByAsset inOutsByAsset, AssetDictionary assetDictionary)
            {
                var divisibility = assetDictionary.GetAssetProp(inOutsByAsset.AssetId, p => p.Divisibility, 0);

                var result = new ColoredAsset
                {
                    AssetId = inOutsByAsset.AssetId,
                    Divisibility = divisibility,
                    Name = assetDictionary.GetAssetProp(inOutsByAsset.AssetId, p => p.Name, null) ?? inOutsByAsset.AssetId,
                    IconImageUrl = assetDictionary.GetAssetProp(inOutsByAsset.AssetId, p => p.IconUrl, null),
                    ShortName = assetDictionary.GetAssetProp(inOutsByAsset.AssetId, p => p.NameShort, null),
                    Ins = inOutsByAsset.TransactionIn.Select(p=> In.Create(p, divisibility, inOutsByAsset.TransactionsOut)),
                    Outs = inOutsByAsset.TransactionsOut.Select(p => Out.Create(p, divisibility))
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
}