using System.Threading.Tasks;
using System.Web.Mvc;
using BCNExplorer.Web.Models;
using Core.AddressService;
using Core.Asset;
using Core.Block;

namespace BCNExplorer.Web.Controllers
{
    public class AddressController:Controller
    {
        private readonly IAddressService _addressProvider;
        private readonly IAssetService _assetService;
        private readonly IBlockService _blockService;

        public AddressController(IAddressService addressProvider, IAssetService assetService, IBlockService blockService)
        {
            _addressProvider = addressProvider;
            _assetService = assetService;
            _blockService = blockService;
        }

        [Route("address/{id}")]
        public async Task<ActionResult> Index(string id, int? at)
        {
            var mainInfoTask = _addressProvider.GetMainInfoAsync(id);

            await Task.WhenAll(mainInfoTask);

            if (mainInfoTask.Result != null)
            {
                return View(AddressMainInfoViewModel.Create(mainInfoTask.Result));
            }

            return View("NotFound");
        }

        [Route("address/balance/{id}")]
        public async Task<ActionResult> Balance(string id, int? at)
        {
            var balanceTask = _addressProvider.GetBalanceAsync(id);
            var assetDefinitionDictionaryTask = _assetService.GetAssetDefinitionDictionaryAsync();
            var lastBlockTask = _blockService.GetLastBlockHeaderAsync();
            Task<IBlockHeader> atBlockTask;

            if (at != null)
            {
                atBlockTask = _blockService.GetBlockHeaderAsync(at.ToString());
            }
            else
            {
                atBlockTask = Task.FromResult<IBlockHeader>(null);
            }

            await Task.WhenAll(balanceTask, assetDefinitionDictionaryTask, lastBlockTask, atBlockTask);

            if (balanceTask.Result != null)
            {
                return View(AddressBalanceViewModel.Create(balanceTask.Result, 
                    assetDefinitionDictionaryTask.Result,
                    lastBlockTask.Result,
                    atBlockTask.Result));
            }

            return View("NotFound");
        }

        [Route("address/transactions/{id}")]
        public async Task<ActionResult> Transactions(string id)
        {
            return View(AddressTransactionsViewModel.Create(await _addressProvider.GetTransactions(id)));
        } 
    }
}