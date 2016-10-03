using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.IocContainer;
using Microsoft.Azure.WebJobs.Host;

namespace JobsCommon
{
    public class DResolver : IJobActivator
    {
        public readonly IoC IoC = new IoC();

        public T CreateInstance<T>()
        {
            return (T)IoC.GetObject(typeof(T));
        }
    }
}
