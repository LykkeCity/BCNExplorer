using Common.IocContainer;

namespace TaskRouter.Functions
{
    public static class RouterFunctionsBinder
    {
        public static void BindRouterFunctions(this IoC ioc)
        {
            ioc.RegisterPerCall<ScanBlocksFunctions>();
        }
    }
}
