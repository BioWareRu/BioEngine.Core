using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BioEngine.Core.Interfaces;
using FluentValidation.Results;

namespace BioEngine.Core.Repository
{
    public abstract class BaseRepositoryFilter : IRepositoryFilter
    {
        public abstract bool CanProcess(Type type);

        public virtual Task<bool> BeforeValidate<T, TId>(T item,
            (bool isValid, IList<ValidationFailure> errors) validationResult) where T : class, IEntity<TId>
        {
            return Task.FromResult(true);
        }

        public virtual Task<bool> BeforeSave<T, TId>(T item,
            (bool isValid, IList<ValidationFailure> errors) validationResult) where T : class, IEntity<TId>
        {
            return Task.FromResult(true);
        }

        public virtual Task<bool> AfterSave<T, TId>(T item) where T : class, IEntity<TId>
        {
            return Task.FromResult(true);
        }
    }
}