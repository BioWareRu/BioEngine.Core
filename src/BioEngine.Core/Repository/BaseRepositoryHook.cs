using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using FluentValidation.Results;

namespace BioEngine.Core.Repository
{
    public abstract class BaseRepositoryHook : IRepositoryHook
    {
        public abstract bool CanProcess(Type type);

        public virtual Task<bool> BeforeValidateAsync<T>(T item,
            (bool isValid, IList<ValidationFailure> errors) validationResult,
            PropertyChange[]? changes = null, IBioRepositoryOperationContext? operationContext = null)
            where T : class, IBioEntity
        {
            return Task.FromResult(true);
        }

        public virtual Task<bool> BeforeSaveAsync<T>(T item,
            (bool isValid, IList<ValidationFailure> errors) validationResult,
            PropertyChange[]? changes = null, IBioRepositoryOperationContext? operationContext = null)
            where T : class, IBioEntity
        {
            return Task.FromResult(true);
        }

        public virtual Task<bool> AfterSaveAsync<T>(T item, PropertyChange[]? changes = null,
            IBioRepositoryOperationContext? operationContext = null)
            where T : class, IBioEntity
        {
            return Task.FromResult(true);
        }
    }
}
