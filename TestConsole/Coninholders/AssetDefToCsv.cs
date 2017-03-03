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
            csv.AppendLine("AssetIds|Name|NameShort|Divisibility|Verified?");
            foreach (var assetDefinition in assetDefs)
            {
                var str =
                    $"{string.Join(",", assetDefinition.AssetIds)}|{assetDefinition.Name}|{assetDefinition.NameShort}|{assetDefinition.Divisibility}|{assetDefinition.IsVerified()}";
                Console.WriteLine(str);
                csv.AppendLine(str);
            }
            File.Delete(filePath);
            File.WriteAllText(filePath, csv.ToString());
            Console.WriteLine("Done");
            Console.ReadLine();

        }
    }
}
