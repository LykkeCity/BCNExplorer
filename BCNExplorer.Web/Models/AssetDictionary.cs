using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Core.Asset;
using Providers.TransportTypes.Asset;

namespace BCNExplorer.Web.Models
{
    public class AssetDictionary
    {
        public IDictionary<string, AssetViewModel> Dic { get; set; }

        public AssetViewModel Get(string asset)
        {
            if (Dic.ContainsKey(asset))
            {
                return Dic[asset];
            }

            return null;
        }

        public T GetAssetProp<T>(string asset, Expression<Func<AssetViewModel, T>> selectPropExpression, T defaultValue)
        {
            var ent = Get(asset);
            if (ent != null)
            {
                return selectPropExpression.Compile()(ent);
            }
            return defaultValue;
        }

        public static AssetDictionary Create(IDictionary<string, IAssetDefinition> source)
        {
            return new AssetDictionary
            {
                Dic = source.ToDictionary(p => p.Key, p => AssetViewModel.Create(p.Value))
            };
        }
    }
}