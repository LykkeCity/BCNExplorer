using Common.IocContainer;
using Common.Log;
using Core.Settings;

namespace JobsCommon.Binders
{
    public static class JobsCommonBinder
    {
        public static void BindJobsCommon(this IoC ioc, BaseSettings baseSettings, ILog log)
        {
            ioc.RegisterSingleTone<MainChainRepository>();
        }
    }
}
