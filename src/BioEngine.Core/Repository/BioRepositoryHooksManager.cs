using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Entities;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;

namespace BioEngine.Core.Repository
{
    public class BioRepositoryHooksManager
    {
        private readonly IServiceProvider _serviceProvider;

        public BioRepositoryHooksManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }


        public async Task<bool> BeforeValidateAsync<T>(T item,
            (bool isValid, IList<ValidationFailure> errors) validationResult,
            PropertyChange[] changes = null, IBioRepositoryOperationContext operationContext = null)
            where T : class, IEntity
        {
            var result = true;
            var hooks = _serviceProvider.GetServices<IRepositoryHook>().ToArray();
            if (hooks.Any())
            {
                foreach (var hook in hooks)
                {
                    if (!hook.CanProcess(typeof(T))) continue;
                    result = await hook.BeforeValidateAsync(item, validationResult, changes, operationContext);
                }
            }

            return result;
        }

        public async Task<bool> BeforeSaveAsync<T>(T item,
            (bool isValid, IList<ValidationFailure> errors) validationResult,
            PropertyChange[] changes = null, IBioRepositoryOperationContext operationContext = null)
            where T : class, IEntity
        {
            var result = true;
            var hooks = _serviceProvider.GetServices<IRepositoryHook>().ToArray();
            if (hooks.Any())
            {
                foreach (var hook in hooks)
                {
                    if (!hook.CanProcess(typeof(T))) continue;
                    result = await hook.BeforeSaveAsync(item, validationResult, changes, operationContext);
                }
            }

            return result;
        }

        public async Task<bool> AfterSaveAsync<T>(T item, PropertyChange[] changes = null,
            IBioRepositoryOperationContext operationContext = null) where T : class, IEntity
        {
            var result = true;
            var hooks = _serviceProvider.GetServices<IRepositoryHook>().ToArray();
            if (hooks.Any())
            {
                foreach (var hook in hooks)
                {
                    if (!hook.CanProcess(typeof(T))) continue;
                    result = await hook.AfterSaveAsync(item, changes, operationContext);
                }
            }

            return result;
        }
    }
}
