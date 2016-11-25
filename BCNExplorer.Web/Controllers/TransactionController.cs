using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BCNExplorer.Web.Models;
using Providers.Providers.Asset;
using Providers.Providers.Ninja;

namespace BCNExplorer.Web.Controllers
{
    public class TransactionController:Controller
    {
        private readonly TransactionProvider _transactionProvider;
        private readonly AssetProvider _assetProvider;

        public TransactionController(TransactionProvider transactionProvider, AssetProvider assetProvider)
        {
            _transactionProvider = transactionProvider;
            _assetProvider = assetProvider;
        }

        [Route("transaction/{id}")]
        public async Task<ActionResult> Index(string id, bool change = true)
        {
            var ninjaTransaction = await _transactionProvider.GetAsync(id, change);

            if (ninjaTransaction != null)
            {
                var result = TransactionViewModel.Create(ninjaTransaction, await _assetProvider.GetAssetDictionaryAsync());

                return View(result);
            }

            return View("NotFound");
        }

        [OutputCache(Duration = 2 * 60)]
        [Route("transation/list")]
        public async Task<ActionResult> List(IList<string> ids)
        {
            var result = new ConcurrentStack<TransactionViewModel>();

            var assetDictionary = await _assetProvider.GetAssetDictionaryAsync();

            var loadTransactionTasks = ids.Select(id => _transactionProvider.GetAsync(id).ContinueWith(task =>
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