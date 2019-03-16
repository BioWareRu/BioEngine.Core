using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BioEngine.Core.Repository;
using FluentValidation.Results;

namespace BioEngine.Core.Interfaces
{
    public interface IRepositoryFilter
    {
        bool CanProcess(Type type);

        Task<bool> BeforeValidateAsync<T>(T item, (bool isValid, IList<ValidationFailure> errors) validationResult,
            PropertyChange[] changes = null, IBioRepositoryOperationContext operationContext = null)
            where T : class, IEntity;

        Task<bool> BeforeSaveAsync<T>(T item, (bool isValid, IList<ValidationFailure> errors) validationResult,
            PropertyChange[] changes = null, IBioRepositoryOperationContext operationContext = null)
            where T : class, IEntity;

        Task<bool> AfterSaveAsync<T>(T item, PropertyChange[] changes = null, IBioRepositoryOperationContext operationContext = null) where T : class, IEntity;
    }
}
