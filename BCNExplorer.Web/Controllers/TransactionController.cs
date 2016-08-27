using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BCNExplorer.Web.Models;
using NinjaProviders.Providers;

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

            return HttpNotFound();
        }

        [Route("transation/list")]
        public async Task<ActionResult> List(IList<string> ids)
        {
            var result = new List<TransactionViewModel>();

            var loadTransactionTasks = new List<Task>();
            foreach (var id in ids)
            {
                var task = _ninjaTransactionProvider.GetAsync(id).ContinueWith(p =>
                {
                    result.Add(TransactionViewModel.Create(p.Result, id));
                });

                loadTransactionTasks.Add(task);
            }

            await Task.WhenAll(loadTransactionTasks);

            return View(result.OrderBy(p=> ids.IndexOf(p.TransactionId)));
        }
    }
}