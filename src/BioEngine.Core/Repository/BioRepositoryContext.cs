using System.Collections.Generic;
using System.Linq;
using BioEngine.Core.DB;
using BioEngine.Core.Interfaces;
using FluentValidation;

namespace BioEngine.Core.Repository
{
    public class BioRepositoryContext<T, TId> where T : IEntity<TId>
    {
        internal BioContext DbContext { get; }
        public IValidator<T>[] Validators { get; }

        public BioRepositoryContext(BioContext dbContext, IEnumerable<IValidator<T>> validators = default)
        {
            DbContext = dbContext;
            Validators = validators?.ToArray();
        }
    }
}