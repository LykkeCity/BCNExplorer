using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.SessionState;
using BCNExplorer.Web.Models;
using Core.Asset;
using Core.Transaction;

namespace BCNExplorer.Web.Controllers
{
    [SessionState(SessionStateBehavior.Disabled)]
    public class TransactionController:Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly IAssetService _assetService;
        private readonly ICachedTransactionService _cachedTransactionService;

        public TransactionController(ITransactionService transactionService, 
            IAssetService assetService, 
            ICachedTransactionService cachedTransactionService)
        {
            _transactionService = transactionService;
            _assetService = assetService;
            _cachedTransactionService = cachedTransactionService;
        }

        [Route("transaction/{id}")]
        public async Task<ActionResult> Index(string id)
        {
            var ninjaTransaction = await _transactionService.GetAsync(id);

            if (ninjaTransaction != null)
            {
                var result = TransactionViewModel.Create(ninjaTransaction, await _assetService.GetAssetDefinitionDictionaryAsync());

                return View(result);
            }

            return View("NotFound");
        }

        
        [Route("transation/list")]
        public async Task<ActionResult> List(IList<string> ids)
        {
            var assetDictionary = _assetService.GetAssetDefinitionDictionaryAsync();

            var loadTransactionTask = _cachedTransactionService.GetAsync(ids);

            await Task.WhenAll(loadTransactionTask, assetDictionary);

            return View(loadTransactionTask.Result.Select(p=> TransactionViewModel.Create(p, assetDictionary.Result)).OrderBy(p => ids.IndexOf(p.TransactionId)));
        }
    }
}