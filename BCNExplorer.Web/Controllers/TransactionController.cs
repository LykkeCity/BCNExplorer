using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BCNExplorer.Web.Models;
using Core.Asset;
using Core.Transaction;

namespace BCNExplorer.Web.Controllers
{
    public class TransactionController:Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly IAssetService _assetService;

        public TransactionController(ITransactionService transactionService, IAssetService assetService)
        {
            _transactionService = transactionService;
            _assetService = assetService;
        }

        [Route("transaction/{id}")]
        public async Task<ActionResult> Index(string id, bool change = true)
        {
            var ninjaTransaction = await _transactionService.GetAsync(id, change);

            if (ninjaTransaction != null)
            {
                var result = TransactionViewModel.Create(ninjaTransaction, await _assetService.GetAssetDefinitionDictionaryAsync());

                return View(result);
            }

            return View("NotFound");
        }

        [OutputCache(Duration = 2 * 60)]
        [Route("transation/list")]
        public async Task<ActionResult> List(IList<string> ids)
        {
            var result = new ConcurrentStack<TransactionViewModel>();

            var assetDictionary = await _assetService.GetAssetDefinitionDictionaryAsync();

            var loadTransactionTasks = ids.Select(id => _transactionService.GetAsync(id).ContinueWith(task =>
            {
                if (task.Result != null)
                {
                    result.Push(TransactionViewModel.Create(task.Result, assetDictionary));
                }
            }));

            await Task.WhenAll(loadTransactionTasks);

            return View(result.OrderBy(p=> ids.IndexOf(p.TransactionId)));
        }
    }
}