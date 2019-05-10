using System;

namespace BioEngine.Core.Properties
{
    internal struct PropertiesRegistration
    {
        public PropertiesRegistration(string key, PropertiesRegistrationType registrationType, Type entityType)
        {
            Key = key;
            EntityType = entityType;
            RegistrationType = registrationType;
        }

        public string Key { get; }
        public Type EntityType { get; }
        public PropertiesRegistrationType RegistrationType { get; }
    }
}
