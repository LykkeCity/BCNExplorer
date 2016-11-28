using Common.IocContainer;
using Common.Log;
using Core.Block;
using Core.Settings;
using Core.Transaction;
using JobsCommon;
using Services.BlockChain;
using Services.Transaction;

namespace Services.Binders
{
    public static class ServicesBinder
    {
        public static void BindServices(this IoC ioc, BaseSettings baseSettings, ILog log)
        {
            ioc.RegisterSingleTone<MainChainRepository>();
            ioc.RegisterSingleTone<BalanceChangesService>();

            ioc.RegisterPerCall<IBlockService, BlockService>();
            ioc.RegisterPerCall<ITransactionService, TransactionService>();
        }
    }
}
