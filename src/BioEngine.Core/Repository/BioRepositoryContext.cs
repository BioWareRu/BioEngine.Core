using System.Collections.Generic;
using System.Linq;
using BioEngine.Core.Abstractions;
using BioEngine.Core.DB;
using BioEngine.Core.Properties;
using FluentValidation;

namespace BioEngine.Core.Repository
{
    public class BioRepositoryContext<T> where T : class, IEntity
    {
        internal BioContext DbContext { get; }
        public List<IValidator<T>>? Validators { get; }
        public PropertiesProvider PropertiesProvider { get; }
        public BioRepositoryHooksManager HooksManager { get; }

        public BioRepositoryContext(
            BioContext dbContext,
            PropertiesProvider propertiesProvider,
            BioRepositoryHooksManager hooksManager,
            IEnumerable<IValidator<T>>? validators = default)
        {
            DbContext = dbContext;
            PropertiesProvider = propertiesProvider;
            HooksManager = hooksManager;
            Validators = validators?.ToList();
        }
    }
}
