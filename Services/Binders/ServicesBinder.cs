using Common.IocContainer;
using Common.Log;
using Core.Block;
using Core.Settings;
using JobsCommon;
using Services.BlockChain;

namespace Services.Binders
{
    public static class ServicesBinder
    {
        public static void BindServices(this IoC ioc, BaseSettings baseSettings, ILog log)
        {
            ioc.RegisterSingleTone<MainChainRepository>();
            ioc.RegisterSingleTone<BalanceChangesService>();
            ioc.RegisterPerCall<IBlockService, BlockService>();
        }
    }
}
