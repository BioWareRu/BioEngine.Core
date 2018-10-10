using System;
using System.Collections.Generic;
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
        private static readonly List<SettingsType> Types = new List<SettingsType>();

        private static readonly Dictionary<Type, SettingsSchema> Schema = new Dictionary<Type, SettingsSchema>();

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
            SettingsType type = Types.FirstOrDefault(t => t.SettingsClassType == settingsType);
            if (type == null)
            {
                type = new SettingsType(settingsType);
                var classAttr = settingsType.GetCustomAttribute<SettingsClassAttribute>();
                if (classAttr != null)
                {
                    var properties = new List<SettingsPropertySchema>();
                    foreach (var propertyInfo in settingsType.GetProperties())
                    {
                        var attr = propertyInfo.GetCustomAttribute<SettingsPropertyAttribute>();
                        if (attr != null)
                        {
                            properties.Add(new SettingsPropertySchema(propertyInfo.Name, attr.Name, attr.Type));
                        }
                    }

                    var schema = new SettingsSchema(settingsType.FullName, classAttr.Name, classAttr.IsEditable,
                        classAttr.Mode);
                    schema.Properties.AddRange(properties);

                    Schema.Add(settingsType, schema);
                }

                Types.Add(type);
            }

            type.AddRegistration(registrationType, entityType);
        }

        public static SettingsSchema GetSchema<TSettings>() where TSettings : SettingsBase, new()
        {
            if (!Schema.ContainsKey(typeof(TSettings)))
            {
                throw new ArgumentException($"Type {typeof(TSettings)} is not registered in settings provider!");
            }

            return Schema[typeof(TSettings)];
        }

        public static SettingsBase GetInstance(string className)
        {
            var type = Schema.Keys.FirstOrDefault(k => k.FullName == className);
            if (type != null)
            {
                return (SettingsBase) Activator.CreateInstance(type);
            }

            return null;
        }

        public static SettingsSchema GetSchema(Type settingsType)
        {
            if (!Schema.ContainsKey(settingsType))
            {
                throw new ArgumentException($"Type {settingsType} is not registered in settings provider!");
            }

            return Schema[settingsType];
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

        public async Task<bool> Set<TSettings>(TSettings settings, IEntity entity, int? siteId = null)
            where TSettings : SettingsBase, new()
        {
            var record = await LoadFromDatabase(settings, entity);
            if (record == null)
            {
                record = new SettingsRecord
                {
                    Key = settings.GetType().FullName,
                    EntityType = entity.GetType().FullName,
                    EntityId = entity.GetId().ToString(),
                    SiteId = siteId
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

        public async Task LoadSettings<T, TId>(IEnumerable<T> entities) where T : class, IEntity<TId>
        {
            var entitiesArray = entities as T[] ?? entities.ToArray();
            if (entitiesArray.Any())
            {
                var sites = await _dbContext.Sites.ToListAsync();
                var groups = entitiesArray.GroupBy(e => e.GetType().FullName);
                foreach (var @group in groups)
                {
                    var ids = @group.Where(e => e.GetId() != default).Select(e => e.GetId().ToString());
                    var entityType = @group.Key;
                    var groupEntity = group.First();
                    var types = Types.Where(registration => registration.IsRegisteredFor(groupEntity.GetType()))
                        .ToList();
                    if (groupEntity is Section)
                    {
                        types.AddRange(Types.Where(registration =>
                            registration.IsRegisteredForSections()));
                    }

                    if (groupEntity is ContentItem)
                    {
                        types.AddRange(Types.Where(registration =>
                            registration.IsRegisteredForContent()));
                    }

                    var settings = await _dbContext.Settings.Where(s =>
                        s.EntityType == entityType && ids.Contains(s.EntityId)).ToListAsync();

                    foreach (var entity in @group)
                    {
                        entity.Settings = new List<SettingsEntry>();
                        foreach (var type in types)
                        {
                            var entry = new SettingsEntry(type.SettingsClassType.FullName,
                                GetSchema(type.SettingsClassType));

                            var records = settings.Where(s =>
                                    s.EntityId == entity.Id.ToString() && s.Key == type.SettingsClassType.FullName)
                                .ToList();

                            switch (entry.Schema.Mode)
                            {
                                case SettingMode.OnePerEntity:
                                    if (records.Any())
                                    {
                                        entry.Settings.Add(new SettingsValue(null,
                                            DeserializeSettings(records.First(), type)));
                                    }
                                    else
                                    {
                                        entry.Settings.Add(new SettingsValue(null,
                                            GetInstance(type.SettingsClassType.FullName)));
                                    }

                                    break;
                                case SettingMode.OnePerSite:
                                    foreach (var site in sites)
                                    {
                                        var record = records.FirstOrDefault(r => r.SiteId == site.Id);
                                        SettingsBase recordSettings;
                                        if (record == null)
                                        {
                                            recordSettings = GetInstance(type.SettingsClassType.FullName);
                                        }
                                        else
                                        {
                                            recordSettings = DeserializeSettings(records.First(), type);
                                        }

                                        entry.Settings.Add(new SettingsValue(site.Id, recordSettings));
                                    }

                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            entity.Settings.Add(entry);
                        }
                    }
                }
            }
        }

        private SettingsBase DeserializeSettings(SettingsRecord record, SettingsType type)
        {
            return (SettingsBase) JsonConvert.DeserializeObject(record.Data, type.SettingsClassType);
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class SettingsClassAttribute : Attribute
    {
        public string Name { get; set; }
        public bool IsEditable { get; set; }
        public SettingMode Mode { get; set; } = SettingMode.OnePerEntity;
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SettingsPropertyAttribute : Attribute
    {
        public string Name { get; set; }
        public SettingType Type { get; set; } = SettingType.String;
    }

    public enum SettingMode
    {
        OnePerEntity = 1,
        OnePerSite = 2
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

    public class SettingsEntry
    {
        public SettingsEntry(string key, SettingsSchema schema)
        {
            Key = key;
            Schema = schema;
        }

        public string Key { get; }

        public SettingsSchema Schema { get; }

        public List<SettingsValue> Settings { get; } = new List<SettingsValue>();
    }

    public class SettingsValue
    {
        public SettingsValue(int? siteId, SettingsBase value)
        {
            SiteId = siteId;
            Value = value;
        }

        public int? SiteId { get; set; }
        public SettingsBase Value { get; set; }
    }

    public class SettingsSchema
    {
        public SettingsSchema(string key, string name, bool isEditable, SettingMode mode)
        {
            Key = key;
            Name = name;
            IsEditable = isEditable;
            Mode = mode;
        }

        public string Key { get; set; }
        public string Name { get; set; }
        public bool IsEditable { get; set; }
        public SettingMode Mode { get; set; }
        public List<SettingsPropertySchema> Properties { get; } = new List<SettingsPropertySchema>();
    }

    public class SettingsPropertySchema
    {
        public SettingsPropertySchema(string key, string name, SettingType type)
        {
            Key = key;
            Name = name;
            Type = type;
        }

        public string Key { get; set; }
        public string Name { get; set; }
        public SettingType Type { get; set; }
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