using System;
using System.Reflection;
using BioEngine.Core.Abstractions;

namespace BioEngine.Core.DB
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EntityAttribute : Attribute
    {
        public EntityAttribute(string key)
        {
            Key = key;
        }

        public string Key { get; set; }
    }

    public static class EntityExtensions
    {
        public static string GetKey(this IEntity entity)
        {
            var attr = entity.GetType().GetCustomAttribute<EntityAttribute>();
            if (attr == null)
            {
                throw new ArgumentException($"Entity type without type attribute: {entity.GetType()}");
            }

            return attr.Key;
        }

        public static string GetKey<T>() where T : IEntity
        {
            var attr = typeof(T).GetCustomAttribute<EntityAttribute>();
            if (attr == null)
            {
                throw new ArgumentException($"Entity type without type attribute: {typeof(T)}");
            }

            return attr.Key;
        }
    }
}
