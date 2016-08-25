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
    }
}