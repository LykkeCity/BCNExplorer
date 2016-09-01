using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BCNExplorer.Web.Models;
using Providers.Providers.Ninja;

namespace BCNExplorer.Web.Controllers
{
    public class TransactionController:Controller
    {
        private readonly NinjaTransactionProvider _ninjaTransactionProvider;

        public TransactionController(NinjaTransactionProvider ninjaTransactionProvider)
        {
            _ninjaTransactionProvider = ninjaTransactionProvider;
        }

        [Route("transaction/{id}")]
        public async Task<ActionResult> Index(string id)
        {
            var ninjaTransaction = await _ninjaTransactionProvider.GetAsync(id);

            if (ninjaTransaction != null)
            {
                var result = TransactionViewModel.Create(ninjaTransaction);

                return View(result);
            }

            return View("NotFound");
        }

        [Route("transation/list")]
        public async Task<ActionResult> List(IList<string> ids)
        {
            var result = new ConcurrentStack<TransactionViewModel>();

            var loadTransactionTasks = ids.Select(id => _ninjaTransactionProvider.GetAsync(id).ContinueWith(task =>
            {
                if (task.Result != null)
                {
                    result.Push(TransactionViewModel.Create(task.Result));
                }
            }));

            await Task.WhenAll(loadTransactionTasks);

            return View(result.OrderBy(p=> ids.IndexOf(p.TransactionId)));
        }
    }
}