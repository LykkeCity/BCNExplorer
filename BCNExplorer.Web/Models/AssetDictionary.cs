using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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

        //public AssetDictionary Create(IDictionary<Asset> )
    }
}