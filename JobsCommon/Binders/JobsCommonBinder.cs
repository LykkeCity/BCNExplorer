using Common.IocContainer;
using Common.Log;
using Core.Settings;
using Services.Binders;

namespace JobsCommon.Binders
{
    public static class JobsCommonBinder
    {
        public static void BindJobsCommon(this IoC ioc, BaseSettings baseSettings, ILog log)
        {
            ioc.RegisterSingleTone<MainChainRepository>();
            ioc.RegisterSingleTone<BalanceChangesService>();
        }
    }
}
