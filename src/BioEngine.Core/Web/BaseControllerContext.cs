using BioEngine.Core.Interfaces;
using BioEngine.Core.Properties;
using Microsoft.Extensions.Logging;

namespace BioEngine.Core.Web
{
    public class BaseControllerContext
    {
        public readonly IStorage Storage;
        public readonly PropertiesProvider PropertiesProvider;
        public ILogger Logger { get; }

        public BaseControllerContext(ILoggerFactory loggerFactory, IStorage storage, PropertiesProvider propertiesProvider)
        {
            Storage = storage;
            PropertiesProvider = propertiesProvider;
            Logger = loggerFactory.CreateLogger(GetType());
        }
    }

    public class BaseControllerContext<TEntity, TEntityPk> : BaseControllerContext
        where TEntity : class, IEntity<TEntityPk>
    {
        public IBioRepository<TEntity, TEntityPk> Repository { get; }

        public BaseControllerContext(ILoggerFactory loggerFactory, IStorage storage,
            PropertiesProvider propertiesProvider,
            IBioRepository<TEntity, TEntityPk> repository) : base(loggerFactory, storage, propertiesProvider)
        {
            Repository = repository;
        }
    }
}