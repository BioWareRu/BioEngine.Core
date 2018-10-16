using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BioEngine.Core.Settings
{
    public class SettingsSchema
    {
        public string Key { get; }
        public string Name { get; }
        public bool IsEditable { get; }
        public SettingsMode Mode { get; }
        public List<SettingsPropertySchema> Properties { get; }
        public Type Type { get; }

        private readonly HashSet<SettingsRegistration> _registrations = new HashSet<SettingsRegistration>();

        protected SettingsSchema(Type settingsType, SettingsClassAttribute classAttribute,
            List<SettingsPropertySchema> properties)
        {
            Key = settingsType.FullName;
            Name = classAttribute.Name;
            IsEditable = classAttribute.IsEditable;
            Mode = classAttribute.Mode;
            Type = settingsType;
            Properties = properties;
        }

        public static SettingsSchema Create<T>() where T : SettingsBase
        {
            var settingsType = typeof(T);
            var classAttr = settingsType.GetCustomAttribute<SettingsClassAttribute>();
            if (classAttr == null)
            {
                throw new Exception("Settings class must have SettingsClassAttribute");
            }

            var properties = new List<SettingsPropertySchema>();
            foreach (var propertyInfo in settingsType.GetProperties())
            {
                var attr = propertyInfo.GetCustomAttribute<SettingsPropertyAttribute>();
                if (attr != null)
                {
                    properties.Add(new SettingsPropertySchema(propertyInfo.Name, attr.Name, attr.Type,
                        attr.IsRequired));
                }
            }

            return new SettingsSchema(settingsType, classAttr, properties);
        }


        public void AddRegistration(SettingsRegistrationType type, Type entityType = null)
        {
            _registrations.Add(new SettingsRegistration(type, entityType));
        }

        public bool IsRegisteredFor(Type entityType)
        {
            return _registrations.Any(r => r.EntityType == entityType && r.RegistrationType == SettingsRegistrationType.Entity);
        }

        public bool IsRegisteredForSections()
        {
            return _registrations.Any(r => r.RegistrationType == SettingsRegistrationType.Section);
        }

        public bool IsRegisteredForContent()
        {
            return _registrations.Any(r => r.RegistrationType == SettingsRegistrationType.Content);
        }
    }
}