using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.IocContainer;
using Common.Log;
using Core.Asset;
using Core.AssetBlockChanges;
using Core.Settings;
using NBitcoin;

namespace TestConsole
{
    public static class GetAddressesWithColoredAssetsFromCoinPrism
    {
        public static void Run(IoC container)
        {
            var baseSettings = container.GetObject<BaseSettings>();
            var log = container.GetObject<ILog>();

            var assetDefRepo = container.GetObject<IAssetDefinitionRepository>();

            var allAssets = assetDefRepo.GetAllAsync().Result.SelectMany(p=>p.AssetIds);
            var failAssets = new List<string>();

            //var addressResults = new List<AddressResult>();
            //var tasks = new List<Task>();
            //var counter = allAssets.Count();
            //foreach (var asset in allAssets)
            //{
            //    var task = GetAddresses(asset).ContinueWith(p =>
            //    {
            //        Console.WriteLine(counter);
            //        counter--;
            //        if (p.Result.Any(x => x == null))
            //        {
            //            throw new Exception();
            //        }
            //        addressResults.AddRange(p.Result);
            //    });
            //    tasks.Add(task);

            //}

            //try
            //{
            //    Task.WaitAll(tasks.ToArray());
            //}
            //catch (Exception e)
            //{
                
            //}

            //Console.WriteLine("Done");
            ////Console.ReadLine();

            //var addressRepo = container.GetObject<IAddressRepository>();

            //addressRepo.AddAsync(addressResults.ToArray()).Wait();
            //Console.WriteLine("save Done");
        }

        //public static async Task<IEnumerable<AddressResult>> GetAddresses(string asset)
        //{
        //    var webRequest = (HttpWebRequest)WebRequest.Create(string.Format("https://api.coinprism.com/v1/assets/{0}/owners", asset));
        //    webRequest.Method = "GET";
        //    webRequest.ContentType = "application/x-www-form-urlencoded";
        //    var webResponse = await webRequest.GetResponseAsync();
        //    using (var receiveStream = webResponse.GetResponseStream())
        //    {
        //        using (var sr = new StreamReader(receiveStream))
        //        {
        //            var t =
        //                Newtonsoft.Json.JsonConvert.DeserializeObject<CoinprismAssetContract>(await sr.ReadToEndAsync());
        //            return t.owners.Select(p=>new AddressResult
        //            {
        //                ColoredAddress = GetColoredAddress(p.Address).ToString()
        //            });
        //        }
        //    }
        //}

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
