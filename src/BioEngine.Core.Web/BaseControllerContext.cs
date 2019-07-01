using BioEngine.Core.Abstractions;
using BioEngine.Core.Properties;
using BioEngine.Core.Storage;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace BioEngine.Core.Web
{
    public class BaseControllerContext
    {
        public readonly LinkGenerator LinkGenerator;
        public readonly IStorage Storage;
        public readonly PropertiesProvider PropertiesProvider;
        public readonly ICurrentUserProvider CurrentUserProvider;
        public ILogger Logger { get; }

        public BaseControllerContext(ILoggerFactory loggerFactory, IStorage storage,
            PropertiesProvider propertiesProvider, LinkGenerator linkGenerator,
            ICurrentUserProvider currentUserProvider)
        {
            LinkGenerator = linkGenerator;
            Storage = storage;
            PropertiesProvider = propertiesProvider;
            Logger = loggerFactory.CreateLogger(GetType());
            CurrentUserProvider = currentUserProvider;
        }
    }

    public class BaseControllerContext<TEntity, TRepository> : BaseControllerContext
        where TEntity : class, IEntity
        where TRepository : IBioRepository<TEntity>
    {
        public TRepository Repository { get; }

        public BaseControllerContext(ILoggerFactory loggerFactory, IStorage storage,
            PropertiesProvider propertiesProvider, LinkGenerator linkGenerator,
            ICurrentUserProvider currentUserProvider,
            TRepository repository) : base(loggerFactory, storage, propertiesProvider,
            linkGenerator, currentUserProvider)
        {
            Repository = repository;
        }
    }
}
