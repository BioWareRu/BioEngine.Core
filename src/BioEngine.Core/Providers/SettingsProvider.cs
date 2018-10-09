using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Interfaces;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BioEngine.Core.Providers
{
    [UsedImplicitly]
    public class SettingsProvider
    {
        private readonly BioContext _dbContext;
        private static List<SettingsType> _types = new List<SettingsType>();

        private static Dictionary<Type, (string name, bool isEditable,
                Dictionary<string, (string name, SettingType type)> properties)>
            _schema =
                new Dictionary<Type, (string name, bool isEditable,
                    Dictionary<string, (string name, SettingType type)>
                    properties)>();

        public SettingsProvider(BioContext dbContext)
        {
            _dbContext = dbContext;
        }

        public static void RegisterBioEngineSettings<TSettings, TEntity>() where TSettings : SettingsBase, new()
            where TEntity : IEntity, new()
        {
            RegisterBioEngineSettings(typeof(TSettings), typeof(TEntity));
        }

        public static void RegisterBioEngineSettings<TSettings>()
            where TSettings : SettingsBase, new()
        {
            RegisterBioEngineSettings(typeof(TSettings));
        }

        public static void RegisterBioEngineSectionSettings<TSettings>()
            where TSettings : SettingsBase, new()
        {
            RegisterBioEngineSettings(typeof(TSettings), typeof(Section), SettingsRegistrationType.Section);
        }

        public static void RegisterBioEngineContentSettings<TSettings>()
            where TSettings : SettingsBase, new()
        {
            RegisterBioEngineSettings(typeof(TSettings), typeof(ContentItem), SettingsRegistrationType.Content);
        }

        private static void RegisterBioEngineSettings(Type settingsType, Type entityType = null,
            SettingsRegistrationType registrationType = SettingsRegistrationType.Entity)
        {
            SettingsType type = _types.FirstOrDefault(t => t.SettingsClassType == settingsType);
            if (type == null)
            {
                type = new SettingsType(settingsType);
                var classAttr = settingsType.GetCustomAttribute<SettingsClassAttribute>();
                if (classAttr != null)
                {
                    var properties = new Dictionary<string, (string name, SettingType type)>();
                    foreach (var propertyInfo in settingsType.GetProperties())
                    {
                        var attr = propertyInfo.GetCustomAttribute<SettingsPropertyAttribute>();
                        if (attr != null)
                        {
                            properties.Add(propertyInfo.Name, (attr.Name, attr.Type));
                        }
                    }

                    _schema.Add(settingsType, (classAttr.Name, classAttr.IsEditable, properties));
                }

                _types.Add(type);
            }

            type.AddRegistration(registrationType, entityType);
        }

        public static (string name, bool isEditable, Dictionary<string, (string name, SettingType type)>
            propertions) GetSchema<TSettings>() where TSettings : SettingsBase, new()
        {
            if (!_schema.ContainsKey(typeof(TSettings)))
            {
                throw new ArgumentException($"Type {typeof(TSettings)} is not registered in settings provider!");
            }

            return _schema[typeof(TSettings)];
        }

        public static SettingsBase GetInstance(string className)
        {
            var type = _schema.Keys.FirstOrDefault(k => k.FullName == className);
            if (type != null)
            {
                return (SettingsBase) Activator.CreateInstance(type);
            }

            return null;
        }

        public static (string name, bool isEditable, Dictionary<string, (string name, SettingType type)>
            properties) GetSchema(Type settingsType)
        {
            if (!_schema.ContainsKey(settingsType))
            {
                throw new ArgumentException($"Type {settingsType} is not registered in settings provider!");
            }

            return _schema[settingsType];
        }

        public async Task<TSettings> Get<TSettings>() where TSettings : SettingsBase, new()
        {
            var settings = new TSettings();
            var settingsRecord =
                await LoadFromDatabase<TSettings>();
            if (settingsRecord != null)
            {
                settings = JsonConvert.DeserializeObject<TSettings>(settingsRecord.Data);
            }

            return settings;
        }

        public async Task<TSettings> Get<TSettings>(IEntity entity)
            where TSettings : SettingsBase, new()
        {
            var settings = new TSettings();
            var settingsRecord =
                await LoadFromDatabase(settings, entity);
            if (settingsRecord != null)
            {
                settings = JsonConvert.DeserializeObject<TSettings>(settingsRecord.Data);
            }

            return settings;
        }

        public async Task<bool> Set<TSettings>(TSettings settings)
            where TSettings : SettingsBase, new()
        {
            var record = await LoadFromDatabase<TSettings>();
            if (record == null)
            {
                record = new SettingsRecord
                {
                    Key = typeof(TSettings).FullName
                };

                _dbContext.Add(record);
            }

            record.DateUpdated = DateTimeOffset.UtcNow;
            record.Data = JsonConvert.SerializeObject(settings);
            _dbContext.Update(record);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Set<TSettings>(TSettings settings, IEntity entity)
            where TSettings : SettingsBase, new()
        {
            var record = await LoadFromDatabase(settings, entity);
            if (record == null)
            {
                record = new SettingsRecord
                {
                    Key = settings.GetType().FullName,
                    EntityType = entity.GetType().FullName,
                    EntityId = entity.GetId().ToString()
                };

                _dbContext.Add(record);
            }

            record.DateUpdated = DateTimeOffset.UtcNow;
            record.Data = JsonConvert.SerializeObject(settings);
            if (record.Id > 0)
            {
                _dbContext.Update(record);
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        private Task<SettingsRecord> LoadFromDatabase<TSettings>()
            where TSettings : SettingsBase, new()
        {
            return _dbContext.Settings.FirstOrDefaultAsync(s =>
                s.Key == typeof(TSettings).FullName
                && s.EntityType == null && s.EntityId == null);
        }

        private Task<SettingsRecord> LoadFromDatabase<TSettings>(TSettings settings, IEntity entity)
            where TSettings : SettingsBase, new()
        {
            return _dbContext.Settings.FirstOrDefaultAsync(s =>
                s.Key == settings.GetType().FullName
                && s.EntityType == entity.GetType().FullName && s.EntityId == entity.GetId().ToString());
        }

        public async Task<Dictionary<string, SettingsBase>> GetAllSettings(IEntity entity)
        {
            var types = _types.Where(registration => registration.IsRegisteredFor(entity.GetType())).ToList();
            if (entity is Section)
            {
                types.AddRange(_types.Where(registration =>
                    registration.IsRegisteredForSections()));
            }

            if (entity is ContentItem)
            {
                types.AddRange(_types.Where(registration =>
                    registration.IsRegisteredForContent()));
            }

            var keys = types.Select(t => t.SettingsClassType.FullName).ToArray();
            var settings = await _dbContext.Settings.Where(s =>
                s.EntityType == entity.GetType().FullName && s.EntityId == entity.GetId().ToString() &&
                keys.Contains(s.Key)).ToListAsync();
            var entitySettings = new Dictionary<string, SettingsBase>();
            foreach (var type in types)
            {
                var record = settings.FirstOrDefault(s => s.Key == type.SettingsClassType.FullName);
                if (record != null)
                {
                    entitySettings.Add(type.SettingsClassType.FullName,
                        (SettingsBase) JsonConvert.DeserializeObject(record.Data, type.SettingsClassType));
                }
                else
                {
                    entitySettings.Add(type.SettingsClassType.FullName, GetInstance(type.SettingsClassType.FullName));
                }
            }

            return entitySettings;
        }

        public async Task LoadSettings<T, TId>(IEnumerable<T> entities) where T : class, IEntity<TId>
        {
            if (entities != null && entities.Any())
            {
                var groups = entities.GroupBy(e => e.GetType().FullName);
                foreach (var @group in groups)
                {
                    var ids = @group.Where(e => e.GetId() != default).Select(e => e.GetId().ToString());
                    var entityType = @group.Key;
                    var groupEntity = group.First();
                    var types = _types.Where(registration => registration.IsRegisteredFor(groupEntity.GetType()))
                        .ToList();
                    if (groupEntity is Section)
                    {
                        types.AddRange(_types.Where(registration =>
                            registration.IsRegisteredForSections()));
                    }

                    if (groupEntity is ContentItem)
                    {
                        types.AddRange(_types.Where(registration =>
                            registration.IsRegisteredForContent()));
                    }

                    foreach (var entity in @group)
                    {
                        entity.Settings = new Dictionary<string, SettingsBase>();
                        foreach (var type in types)
                        {
                            entity.Settings.Add(type.SettingsClassType.FullName,
                                GetInstance(type.SettingsClassType.FullName));
                        }
                    }

                    var settings = await _dbContext.Settings.Where(s =>
                        s.EntityType == entityType && ids.Contains(s.EntityId)).ToListAsync();
                    foreach (var settingsRecord in settings)
                    {
                        var type = types.FirstOrDefault(r => r.SettingsClassType.FullName == settingsRecord.Key);
                        if (type != null)
                        {
                            var entity = group.First(e => e.GetId().ToString() == settingsRecord.EntityId);

                            entity.Settings[type.SettingsClassType.FullName] =
                                (SettingsBase) JsonConvert.DeserializeObject(settingsRecord.Data,
                                    type.SettingsClassType);
                        }
                    }
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class SettingsClassAttribute : Attribute
    {
        public string Name { get; set; }
        public bool IsEditable { get; set; } = false;
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SettingsPropertyAttribute : Attribute
    {
        public string Name { get; set; }
        public SettingType Type { get; set; }
    }

    public enum SettingType
    {
        String = 1,
        HtmlString = 2,
        PasswordString = 3,
        Number = 4,
        Date = 5,
        DateTime = 6,
        Dropdown = 7,
        LongString = 8
    }

    internal class SettingsType
    {
        public Type SettingsClassType { get; }
        private readonly HashSet<SettingRegistration> _registrations = new HashSet<SettingRegistration>();

        public SettingsType(Type type)
        {
            SettingsClassType = type;
        }

        public void AddRegistration(SettingsRegistrationType type, Type entityType = null)
        {
            _registrations.Add(new SettingRegistration(type, entityType));
        }

        public bool IsRegisteredFor(Type entityType)
        {
            return _registrations.Any(r => r.EntityType == entityType && r.Type == SettingsRegistrationType.Entity);
        }

        public bool IsRegisteredForSections()
        {
            return _registrations.Any(r => r.Type == SettingsRegistrationType.Section);
        }

        public bool IsRegisteredForContent()
        {
            return _registrations.Any(r => r.Type == SettingsRegistrationType.Content);
        }
    }

    internal struct SettingRegistration
    {
        public SettingRegistration(SettingsRegistrationType type, Type entityType = null)
        {
            EntityType = entityType;
            Type = type;
        }

        public Type EntityType { get; }
        public SettingsRegistrationType Type { get; }
    }


    internal enum SettingsRegistrationType
    {
        Entity,
        Section,
        Content
    }

    public class SettingsBase
    {
    }

    [SettingsClass(Name = "Seo", IsEditable = true)]
    public class SeoSettings : SettingsBase
    {
        [SettingsProperty(Name = "Описание", Type = SettingType.LongString)]
        public string Description { get; set; }

        [SettingsProperty(Name = "Ключевые слова", Type = SettingType.String)]
        public string Keywords { get; set; }
    }
}