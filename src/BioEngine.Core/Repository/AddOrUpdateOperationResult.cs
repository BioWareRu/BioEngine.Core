using System.Collections.Generic;
using System.Linq;
using BioEngine.Core.Interfaces;
using FluentValidation.Results;

namespace BioEngine.Core.Repository
{
    public class AddOrUpdateOperationResult<T, TId> where T : IEntity<TId>
    {
        public bool IsSuccess { get; }
        public T Entity { get; }
        public ValidationFailure[] Errors { get; }

        public AddOrUpdateOperationResult(T entity, IEnumerable<ValidationFailure> errors)
        {
            Entity = entity;
            Errors = errors.ToArray();
            IsSuccess = !errors.Any();
        }
    }
}