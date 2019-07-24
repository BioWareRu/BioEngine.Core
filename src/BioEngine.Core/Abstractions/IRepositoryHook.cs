using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BioEngine.Core.Repository;
using FluentValidation.Results;

namespace BioEngine.Core.Abstractions
{
    public interface IRepositoryHook
    {
        bool CanProcess(Type type);

        Task<bool> BeforeValidateAsync<T>(T item, (bool isValid, IList<ValidationFailure> errors) validationResult,
            PropertyChange[]? changes = null, IBioRepositoryOperationContext? operationContext = null)
            where T : class, IBioEntity;

        Task<bool> BeforeSaveAsync<T>(T item, (bool isValid, IList<ValidationFailure> errors) validationResult,
            PropertyChange[]? changes = null, IBioRepositoryOperationContext? operationContext = null)
            where T : class, IBioEntity;

        Task<bool> AfterSaveAsync<T>(T item, PropertyChange[]? changes = null, IBioRepositoryOperationContext? operationContext = null) where T : class, IBioEntity;
    }
}
