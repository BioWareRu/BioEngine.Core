using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation.Results;

namespace BioEngine.Core.Interfaces
{
    public interface IRepositoryFilter
    {
        bool CanProcess(Type type);

        Task<bool> BeforeValidate<T, TId>(T item, (bool isValid, IList<ValidationFailure> errors) validationResult)
            where T : class, IEntity<TId>;

        Task<bool> BeforeSave<T, TId>(T item, (bool isValid, IList<ValidationFailure> errors) validationResult)
            where T : class, IEntity<TId>;

        Task<bool> AfterSave<T, TId>(T item) where T : class, IEntity<TId>;
    }
}