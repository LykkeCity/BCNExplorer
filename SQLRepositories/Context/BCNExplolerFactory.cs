using Common.Log;
using Core.Settings;
using SQLRepositories.Binding;

namespace SQLRepositories.Context
{
    public class BcnExplolerFactory
    {
        private readonly BaseSettings _baseSettings;
        private readonly ILog _log;

        public BcnExplolerFactory(BaseSettings baseSettings, ILog log)
        {
            _baseSettings = baseSettings;
            _log = log;
        }

        public BcnExplolerDataContext GetContext()
        {
            return SqlRepoFactories.GetBcnExplolerDataContext(_baseSettings, _log);
        }
    }
}
