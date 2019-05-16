using System.Collections.Generic;
using System.Linq;
using BioEngine.Core.Entities;
using FluentValidation.Results;

namespace BioEngine.Core.Repository
{
    public class AddOrUpdateOperationResult<T> where T : IEntity
    {
        public bool IsSuccess { get; }
        public T Entity { get; }
        public PropertyChange[] Changes { get; }
        public ValidationFailure[] Errors { get; }

        public AddOrUpdateOperationResult(T entity, IEnumerable<ValidationFailure> errors, PropertyChange[] changes)
        {
            Entity = entity;
            Changes = changes;
            var validationFailures = errors as ValidationFailure[] ?? errors.ToArray();
            Errors = validationFailures.ToArray();
            IsSuccess = !validationFailures.Any();
        }

        public string ErrorsString => string.Join(" ", Errors.Select(e => e.ErrorMessage));
    }
}
