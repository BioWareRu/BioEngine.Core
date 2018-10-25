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

        private readonly HashSet<PropertiesRegistration> _registrations = new HashSet<PropertiesRegistration>();

        private PropertiesSchema(Type propertiesType, PropertiesSetAttribute setAttribute,
            List<PropertiesElementSchema> properties)
        {
            Key = propertiesType.FullName;
            Name = setAttribute.Name;
            IsEditable = setAttribute.IsEditable;
            Mode = setAttribute.Quantity;
            Type = propertiesType;
            Properties = properties;
        }

        public static PropertiesSchema Create<T>() where T : PropertiesSet
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

            return new PropertiesSchema(propertiesType, classAttr, properties);
        }


        public void AddRegistration(PropertiesRegistrationType type, Type entityType = null)
        {
            _registrations.Add(new PropertiesRegistration(type, entityType));
        }

        public bool IsRegisteredFor(Type entityType)
        {
            return _registrations.Any(r => r.EntityType == entityType && r.RegistrationType == PropertiesRegistrationType.Entity);
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