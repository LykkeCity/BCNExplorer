using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Email
{
    public interface ITemplateGenerator
    {
        Task<string> GenerateAsync<T>(string templateName, T templateVm);
    }
}
