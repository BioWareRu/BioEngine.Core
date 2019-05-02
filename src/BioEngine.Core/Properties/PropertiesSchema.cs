using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BioEngine.Core.Properties
{
    public class PropertiesSchema
    {
        public string Key { get; }
        public string Name { get; }
        public bool IsEditable { get; }
        public PropertiesQuantity Mode { get; }
        public List<PropertiesElementSchema> Properties { get; }
        public Type Type { get; }

        private readonly List<PropertiesRegistration> _registrations = new List<PropertiesRegistration>();

        private PropertiesSchema(string key, Type propertiesType, PropertiesSetAttribute setAttribute,
            List<PropertiesElementSchema> properties)
        {
            Key = key;
            Name = setAttribute.Name;
            IsEditable = setAttribute.IsEditable;
            Mode = setAttribute.Quantity;
            Type = propertiesType;
            Properties = properties;
        }

        public static PropertiesSchema Create<T>(string key) where T : PropertiesSet
        {
            var propertiesType = typeof(T);
            var classAttr = propertiesType.GetCustomAttribute<PropertiesSetAttribute>();
            if (classAttr == null)
            {
                throw new Exception("Properties class must have PropertiesSetAttribute");
            }

            var properties = new List<PropertiesElementSchema>();
            foreach (var propertyInfo in propertiesType.GetProperties())
            {
                var attr = propertyInfo.GetCustomAttribute<PropertiesElementAttribute>();
                if (attr != null)
                {
                    properties.Add(new PropertiesElementSchema(propertyInfo.Name, attr.Name, attr.Type,
                        attr.IsRequired));
                }
            }

            return new PropertiesSchema(key, propertiesType, classAttr, properties);
        }


        public void AddRegistration(string key, PropertiesRegistrationType type, Type entityType = null)
        {
            _registrations.Add(new PropertiesRegistration(key, type, entityType));
        }

        public bool IsRegisteredFor(Type entityType)
        {
            return _registrations.Any(r =>
                r.EntityType == entityType && r.RegistrationType == PropertiesRegistrationType.Entity);
        }

        public bool IsRegisteredForSections()
        {
            return _registrations.Any(r => r.RegistrationType == PropertiesRegistrationType.Section);
        }

        public bool IsRegisteredForContent()
        {
            return _registrations.Any(r => r.RegistrationType == PropertiesRegistrationType.Content);
        }
    }
}
