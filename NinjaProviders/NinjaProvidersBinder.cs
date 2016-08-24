using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.IocContainer;

namespace NinjaProviders
{
    public static class NinjaProvidersBinder
    {
        public static void BindNinjaProviders(this IoC ioc)
        {
            ioc.RegisterPerCall<NinjaBlockProvider>();
        }
    }
}
