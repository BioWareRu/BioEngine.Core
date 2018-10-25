using System;

namespace BioEngine.Core.Properties
{
    internal struct PropertiesRegistration
    {
        public PropertiesRegistration(PropertiesRegistrationType registrationType, Type entityType = null)
        {
            EntityType = entityType;
            RegistrationType = registrationType;
        }

        public Type EntityType { get; }
        public PropertiesRegistrationType RegistrationType { get; }
    }
}