using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.IocContainer;
using Core.Asset;

namespace TestConsole.Coninholders
{
    public static class AssetDefToCsv
    {
        public static async Task Run(IoC container)
        {
            Console.WriteLine("AssetDefToCsv");
            var filePath = "./AssetDefinitions.csv";


            var repo = container.GetObject<IAssetDefinitionRepository>();

            var assetDefs = await repo.GetAllAsync();

            var csv = new StringBuilder();
            csv.AppendLine("AssetId,NameShort,Divisibility");
            foreach (var assetDefinition in assetDefs)
            {
                csv.AppendLine($"{assetDefinition.AssetIds.FirstOrDefault()},{assetDefinition.NameShort}, {assetDefinition.Divisibility}");
            }
            
            File.WriteAllText(filePath, csv.ToString());
            Console.WriteLine("Done");
            Console.ReadLine();

        }
    }
}
