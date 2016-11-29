using System;
using System.Threading.Tasks;
using AzureRepositories.AssetCoinHolders;
using Common.IocContainer;
using Core.Settings;
using MongoDB.Bson;
using MongoDB.Driver;

namespace TestConsole.Coninholders
{
    public class CopyMongoData
    {
        public static async Task Run(IoC container)
        {
            var baseSettings = container.GetObject<BaseSettings>();

            var sourceMongoClient = new MongoClient(baseSettings.Db.AssetBalanceChanges.ConnectionString);
            var destClient = new MongoClient("mongodb://localhost:27017");

            var collectionName = AddressAssetBalanceChangeMongoEntity.CollectionName;

            var sourceCollection = sourceMongoClient.GetDatabase(baseSettings.Db.AssetBalanceChanges.DbName).GetCollection<BsonDocument>(collectionName);
            var destCollection = destClient.GetDatabase(baseSettings.Db.AssetBalanceChanges.DbName).GetCollection<BsonDocument>(collectionName);

            var counter = 0;
            using (var cursor = await sourceCollection.Find(p => true).ToCursorAsync())
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    await destCollection.InsertManyAsync(batch);
                    counter++;
                    Console.WriteLine("{0} done", counter);
                }
            }
            Console.WriteLine("All done");
            Console.ReadLine();
        }
    }
}
