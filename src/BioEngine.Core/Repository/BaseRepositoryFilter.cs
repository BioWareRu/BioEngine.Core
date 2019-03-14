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

        public virtual Task<bool> BeforeValidateAsync<T>(T item,
            (bool isValid, IList<ValidationFailure> errors) validationResult,
            PropertyChange[] changes = null) where T : class, IEntity
        {
            return Task.FromResult(true);
        }

        public virtual Task<bool> BeforeSaveAsync<T>(T item,
            (bool isValid, IList<ValidationFailure> errors) validationResult,
            PropertyChange[] changes = null) where T : class, IEntity
        {
            return Task.FromResult(true);
        }

        public virtual Task<bool> AfterSaveAsync<T>(T item, PropertyChange[] changes = null)
            where T : class, IEntity
        {
            return Task.FromResult(true);
        }
    }
}
