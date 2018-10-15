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

        Task<bool> BeforeValidate<T, TId>(T item, (bool isValid, IList<ValidationFailure> errors) validationResult,
            PropertyChange[] changes = null)
            where T : class, IEntity<TId>;

        Task<bool> BeforeSave<T, TId>(T item, (bool isValid, IList<ValidationFailure> errors) validationResult,
            PropertyChange[] changes = null)
            where T : class, IEntity<TId>;

        Task<bool> AfterSave<T, TId>(T item, PropertyChange[] changes = null) where T : class, IEntity<TId>;
    }
}