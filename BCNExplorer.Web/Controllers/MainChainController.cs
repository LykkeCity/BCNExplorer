using System.Threading.Tasks;
using System.Web.Mvc;
using Core.Settings;
using Services.MainChain;

namespace BCNExplorer.Web.Controllers
{
    public class MainChainController:Controller
    {
        private readonly CachedMainChainRepository _cachedMainChainRepository;
        private readonly BaseSettings _baseSettings;

        public MainChainController(CachedMainChainRepository cachedMainChainRepository, BaseSettings baseSettings)
        {
            _cachedMainChainRepository = cachedMainChainRepository;
            _baseSettings = baseSettings;
        }

        [Route("mainchain/update/{secret}")]
        public async Task<ActionResult> Update(string secret)
        {
            if (_baseSettings.Secret == secret)
            {
                await _cachedMainChainRepository.ReloadAsync();

                return new EmptyResult();
            }

            return new HttpUnauthorizedResult();
        } 
    }
}