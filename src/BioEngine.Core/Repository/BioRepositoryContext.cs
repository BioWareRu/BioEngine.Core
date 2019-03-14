using System.Collections.Generic;
using System.Linq;
using BioEngine.Core.DB;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Properties;
using FluentValidation;

namespace BioEngine.Core.Repository
{
    public class BioRepositoryContext<T> where T : class, IEntity
    {
        internal BioContext DbContext { get; }
        public List<IRepositoryFilter> Filters { get; }
        public List<IValidator<T>> Validators { get; }
        public PropertiesProvider PropertiesProvider { get; }

        public BioRepositoryContext(BioContext dbContext, PropertiesProvider propertiesProvider,
            IEnumerable<IValidator<T>> validators = default,
            IEnumerable<IRepositoryFilter> filters = default)
        {
            DbContext = dbContext;
            PropertiesProvider = propertiesProvider;
            Filters = filters?.ToList();
            Validators = validators?.ToList();
        }
    }
}
