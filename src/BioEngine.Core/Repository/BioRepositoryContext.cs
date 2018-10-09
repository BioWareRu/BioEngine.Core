using System.Collections.Generic;
using System.Linq;
using BioEngine.Core.DB;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Providers;
using FluentValidation;

namespace BioEngine.Core.Repository
{
    public class BioRepositoryContext<T, TId> where T : class, IEntity<TId>
    {
        internal BioContext DbContext { get; }
        public IRepositoryFilter[] Filters { get; }
        public IValidator<T>[] Validators { get; }
        public SettingsProvider SettingsProvider { get; }

        public BioRepositoryContext(BioContext dbContext, SettingsProvider settingsProvider,
            IEnumerable<IValidator<T>> validators = default,
            IEnumerable<IRepositoryFilter> filters = default)
        {
            DbContext = dbContext;
            SettingsProvider = settingsProvider;
            Filters = filters?.ToArray();
            Validators = validators?.ToArray();
        }
    }
}