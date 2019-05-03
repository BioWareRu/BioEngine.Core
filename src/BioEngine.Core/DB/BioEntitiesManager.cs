using System;
using System.Collections.Generic;
using BioEngine.Core.Entities;

namespace BioEngine.Core.DB
{
    public class BioEntitiesManager
    {
        private List<Type> _entities = new List<Type>();

        public void Register<TEntity>() where TEntity : IEntity
        {
            if (!_entities.Contains(typeof(TEntity)))
            {
                _entities.Add(typeof(TEntity));
            }
        }

        public IEnumerable<Type> GetTypes()
        {
            return _entities.AsReadOnly();
        }
    }
}
