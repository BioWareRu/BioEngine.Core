using BioEngine.Core.Interfaces;
using BioEngine.Core.Providers;
using Microsoft.Extensions.Logging;

namespace BioEngine.Core.Web
{
    public abstract class BaseControllerContext
    {
        public readonly IStorage Storage;
        public readonly SettingsProvider SettingsProvider;
        public ILogger Logger { get; }

        protected BaseControllerContext(ILoggerFactory loggerFactory, IStorage storage, SettingsProvider settingsProvider)
        {
            Storage = storage;
            SettingsProvider = settingsProvider;
            Logger = loggerFactory.CreateLogger(GetType());
        }
    }

    public class BaseControllerContext<TEntity, TEntityPk> : BaseControllerContext
        where TEntity : class, IEntity<TEntityPk>
    {
        public IBioRepository<TEntity, TEntityPk> Repository { get; }

        public BaseControllerContext(ILoggerFactory loggerFactory, IStorage storage,
            SettingsProvider settingsProvider,
            IBioRepository<TEntity, TEntityPk> repository) : base(loggerFactory, storage, settingsProvider)
        {
            Repository = repository;
        }
    }
}