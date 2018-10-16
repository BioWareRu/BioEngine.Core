using System;

namespace BioEngine.Core.Settings
{
    internal struct SettingsRegistration
    {
        public SettingsRegistration(SettingsRegistrationType registrationType, Type entityType = null)
        {
            EntityType = entityType;
            RegistrationType = registrationType;
        }

        public Type EntityType { get; }
        public SettingsRegistrationType RegistrationType { get; }
    }
}