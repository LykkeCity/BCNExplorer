using Common.IocContainer;
using Common.Log;
using Core.Asset;
using Core.Settings;

namespace AzureRepositories.Binders
{
    public static class AzureRepositoriesBinder
    {
        public static void BindAzureRepositories(this IoC ioc, BaseSettings baseSettings, ILog log)
        {
            ioc.Register<IAssetDefinitionRepository>(AzureRepoFactories.CreateAssetDefinitionsRepository(baseSettings, log));
            ioc.Register<IAssetParsedBlockRepository>(AzureRepoFactories.CreateAssetParsedBlockRepository(baseSettings, log));

            ioc.Register(AzureRepoFactories.CreateUpdateAssetDataCommandProducer(baseSettings, log));
            ioc.Register(AzureRepoFactories.CreateParseBlockCommandProducer(baseSettings, log));
        }
    }
}
