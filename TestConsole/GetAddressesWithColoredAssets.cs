using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.IocContainer;
using Common.Log;
using Core.Asset;
using Core.Settings;
using NBitcoin;
using Providers.Helpers;
using SQLRepositories.Binding;
using SQLRepositories.DbModels;

namespace TestConsole
{
    public static class GetAddressesWithColoredAssets
    {
        public static void Run(IoC container)
        {
            var baseSettings = container.GetObject<BaseSettings>();
            var log = container.GetObject<ILog>();

            var assetDefRepo = container.GetObject<IAssetDefinitionRepository>();

            var allAssets = assetDefRepo.GetAllAsync().Result.SelectMany(p=>p.AssetIds);
            var failAssets = new List<string>();

            var addressResults = new List<AddressResult>();
            var tasks = new List<Task>();
            var counter = allAssets.Count();
            foreach (var asset in allAssets)
            {
                var task = GetAddresses(asset).ContinueWith(p =>
                {
                    Console.WriteLine(counter);
                    counter--;
                    if (p.Result.Any(x => x == null))
                    {
                        throw new Exception();
                    }
                    addressResults.AddRange(p.Result);
                });
                tasks.Add(task);

            }

            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception e)
            {
                
            }

            Console.WriteLine("Done");
            Console.ReadLine();
            
            using (var db = SqlRepoFactories.GetBcnExplolerDataContext(baseSettings, log))
            {
                var addresses =
                    addressResults.Distinct(AddressResult.LegacyAddressColoredAddressComparer).Select(p => new Address
                    {
                        ColoredAddress = p.ColoredAddress,
                        LegacyAddress = p.LegacyAddress
                    });

                db.Addresses.AddRange(addresses);
                db.SaveChanges();
            }
        }

        public static async Task<IEnumerable<AddressResult>> GetAddresses(string asset)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(string.Format("https://api.coinprism.com/v1/assets/{0}/owners", asset));
            webRequest.Method = "GET";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            var webResponse = await webRequest.GetResponseAsync();
            using (var receiveStream = webResponse.GetResponseStream())
            {
                using (var sr = new StreamReader(receiveStream))
                {
                    var t =
                        Newtonsoft.Json.JsonConvert.DeserializeObject<CoinprismAssetContract>(await sr.ReadToEndAsync());
                    return t.owners.Select(p=>new AddressResult
                    {
                        LegacyAddress   = p.Address,
                        ColoredAddress = GetColoredAddress(p.Address).ToString()
                    });
                }
            }
        }

        public class AddressResult
        {
            private sealed class LegacyAddressColoredAddressEqualityComparer : IEqualityComparer<AddressResult>
            {
                public bool Equals(AddressResult x, AddressResult y)
                {
                    if (ReferenceEquals(x, y)) return true;
                    if (ReferenceEquals(x, null)) return false;
                    if (ReferenceEquals(y, null)) return false;
                    if (x.GetType() != y.GetType()) return false;
                    return string.Equals(x.LegacyAddress, y.LegacyAddress) && string.Equals(x.ColoredAddress, y.ColoredAddress);
                }

                public int GetHashCode(AddressResult obj)
                {
                    unchecked
                    {
                        return ((obj.LegacyAddress != null ? obj.LegacyAddress.GetHashCode() : 0)*397) ^ (obj.ColoredAddress != null ? obj.ColoredAddress.GetHashCode() : 0);
                    }
                }
            }

            private static readonly IEqualityComparer<AddressResult> LegacyAddressColoredAddressComparerInstance = new LegacyAddressColoredAddressEqualityComparer();

            public static IEqualityComparer<AddressResult> LegacyAddressColoredAddressComparer
            {
                get { return LegacyAddressColoredAddressComparerInstance; }
            }

            public string LegacyAddress { get; set; }
            public string ColoredAddress { get; set; }

        }

        public static BitcoinColoredAddress GetColoredAddress(string legacyAddress)
        {
            try
            {
                return new BitcoinColoredAddress(new BitcoinPubKeyAddress(legacyAddress));
            }
            catch (Exception)
            {
                
                return new BitcoinColoredAddress(new BitcoinScriptAddress(legacyAddress, Network.Main));
            }

        }
    }





    public class CoinprismAssetContract
    {
        public IEnumerable<Owner> owners { get; set; } 

        public class Owner
        {
             public string Address { get; set; }
             public string Script { get; set; }
        }
    }
}
