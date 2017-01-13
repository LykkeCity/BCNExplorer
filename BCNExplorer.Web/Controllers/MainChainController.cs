using System.Threading.Tasks;
using System.Web.Mvc;
using Core.Settings;
using Services.MainChain;

namespace BCNExplorer.Web.Controllers
{
    public class MainChainController:Controller
    {
        private readonly CachedMainChainService _cachedMainChainService;
        private readonly BaseSettings _baseSettings;

        public MainChainController(CachedMainChainService cachedMainChainService, BaseSettings baseSettings)
        {
            _cachedMainChainService = cachedMainChainService;
            _baseSettings = baseSettings;
        }

        [Route("mainchain/update/{secret}")]
        public async Task<ActionResult> Update(string secret)
        {
            if (_baseSettings.Secret == secret)
            {
                await _cachedMainChainService.ReloadAsync();

                return new EmptyResult();
            }

            return new HttpUnauthorizedResult();
        } 
    }
}