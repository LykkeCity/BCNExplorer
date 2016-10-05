using Common.IocContainer;

namespace AssetScanner.Functions
{
    public static class AssetsFunctionsBinder
    {
        public static void BindAssetsFunctions(this IoC ioc)
        {
            ioc.RegisterPerCall<AssetCreatorFunctions>();
            ioc.RegisterPerCall<AssetDataFunctions>();
        }
    }
}
