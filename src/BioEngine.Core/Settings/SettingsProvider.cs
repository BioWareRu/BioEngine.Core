using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Interfaces;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BioEngine.Core.Settings
{
    [UsedImplicitly]
    public class SettingsProvider
    {
        private readonly BioContext _dbContext;

        private static readonly Dictionary<Type, SettingsSchema> Schema = new Dictionary<Type, SettingsSchema>();

        public SettingsProvider(BioContext dbContext)
        {
            _dbContext = dbContext;
        }

        public static void RegisterBioEngineSettings<TSettings, TEntity>() where TSettings : SettingsBase, new()
            where TEntity : IEntity, new()
        {
            RegisterBioEngineSettings<TSettings>(typeof(TEntity));
        }

        public static void RegisterBioEngineSectionSettings<TSettings>()
            where TSettings : SettingsBase, new()
        {
            RegisterBioEngineSettings<TSettings>(typeof(Section), SettingsRegistrationType.Section);
        }

        public static void RegisterBioEngineContentSettings<TSettings>()
            where TSettings : SettingsBase, new()
        {
            RegisterBioEngineSettings<TSettings>(typeof(ContentItem), SettingsRegistrationType.Content);
        }

        private static void RegisterBioEngineSettings<TSettings>(Type entityType = null,
            SettingsRegistrationType registrationType = SettingsRegistrationType.Entity)
            where TSettings : SettingsBase, new()
        {
            var schema = Schema.ContainsKey(typeof(TSettings)) ? Schema[typeof(TSettings)] : null;
            if (schema == null)
            {
                schema = SettingsSchema.Create<TSettings>();

                Schema.Add(typeof(TSettings), schema);
            }

            schema.AddRegistration(registrationType, entityType);
        }

        public static SettingsBase GetInstance(string className)
        {
            var type = Schema.Keys.FirstOrDefault(k => k.FullName == className);
            if (type != null)
            {
                return (SettingsBase) Activator.CreateInstance(type);
            }

            throw new ArgumentException($"Class {className} is not registered in settings provider");
        }

        public static SettingsSchema GetSchema(Type settingsType)
        {
            if (!Schema.ContainsKey(settingsType))
            {
                throw new ArgumentException($"Type {settingsType} is not registered in settings provider!");
            }

            return Schema[settingsType];
        }

        [PublicAPI]
        public async Task<TSettings> GetAsync<TSettings>() where TSettings : SettingsBase, new()
        {
            var settings = new TSettings();
            var settingsRecord =
                await LoadFromDatabaseAsync<TSettings>();
            if (settingsRecord != null)
            {
                settings = JsonConvert.DeserializeObject<TSettings>(settingsRecord.Data);
            }

            return settings;
        }

        [PublicAPI]
        public async Task<TSettings> GetAsync<TSettings>(IEntity entity, int? siteId = null)
            where TSettings : SettingsBase, new()
        {
            var settings = entity.Settings.FirstOrDefault(x => x.Key == typeof(TSettings).FullName)?.Settings
                .FirstOrDefault(x => x.SiteId == siteId)?.Value as TSettings;
            if (settings == null)
            {
                settings = new TSettings();
                var settingsRecord =
                    await LoadFromDatabaseAsync(settings, entity, siteId);
                if (settingsRecord != null)
                {
                    settings = JsonConvert.DeserializeObject<TSettings>(settingsRecord.Data);
                }
            }

            return settings;
        }

        [PublicAPI]
        public async Task<bool> SetAsync<TSettings>(TSettings settings)
            where TSettings : SettingsBase, new()
        {
            var record = await LoadFromDatabaseAsync<TSettings>();
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

        [PublicAPI]
        public async Task<bool> SetAsync<TSettings>(TSettings settings, IEntity entity, int? siteId = null)
            where TSettings : SettingsBase, new()
        {
            var record = await LoadFromDatabaseAsync(settings, entity);
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

        private Task<SettingsRecord> LoadFromDatabaseAsync<TSettings>()
            where TSettings : SettingsBase, new()
        {
            return _dbContext.Settings.FirstOrDefaultAsync(s =>
                s.Key == typeof(TSettings).FullName
                && s.EntityType == null && s.EntityId == null);
        }

        private Task<SettingsRecord> LoadFromDatabaseAsync<TSettings>(TSettings settings, IEntity entity, int? siteId = null)
            where TSettings : SettingsBase, new()
        {
            return _dbContext.Settings.FirstOrDefaultAsync(s =>
                s.Key == settings.GetType().FullName
                && s.EntityType == entity.GetType().FullName && s.EntityId == entity.GetId().ToString() &&
                (siteId == null || s.SiteId == siteId));
        }

        public async Task LoadSettingsAsync<T, TId>(IEnumerable<T> entities) where T : class, IEntity<TId>
        {
            var entitiesArray = entities as T[] ?? entities.ToArray();
            if (entitiesArray.Any())
            {
                var sites = await _dbContext.Sites.ToListAsync();
                var groups = entitiesArray.GroupBy(e => e.GetType().FullName);
                foreach (var group in groups)
                {
                    var ids = group.Where(e => e.GetId() != default).Select(e => e.GetId().ToString());
                    var entityType = group.Key;
                    var groupEntity = group.First();
                    var schemas = Schema.Where(schema => schema.Value.IsRegisteredFor(groupEntity.GetType()))
                        .ToList();
                    if (groupEntity is Section)
                    {
                        schemas.AddRange(Schema.Where(schema =>
                            schema.Value.IsRegisteredForSections()));
                    }

                    if (groupEntity is ContentItem)
                    {
                        schemas.AddRange(Schema.Where(schema =>
                            schema.Value.IsRegisteredForContent()));
                    }

                    var settings = await _dbContext.Settings.Where(s =>
                        s.EntityType == entityType && ids.Contains(s.EntityId)).ToListAsync();

                    foreach (var entity in group)
                    {
                        entity.Settings = new List<SettingsEntry>();
                        foreach (var schema in schemas.Select(s => s.Value))
                        {
                            var entry = new SettingsEntry(schema.Key, schema);

                            var records = settings.Where(s => s.EntityId == entity.Id.ToString() && s.Key == schema.Key)
                                .ToList();

                            switch (entry.Schema.Mode)
                            {
                                case SettingsMode.OnePerEntity:
                                    if (records.Any())
                                    {
                                        entry.Settings.Add(new SettingsValue(null,
                                            DeserializeSettings(records.First(), schema)));
                                    }
                                    else
                                    {
                                        entry.Settings.Add(new SettingsValue(null,
                                            GetInstance(schema.Type.FullName)));
                                    }

                                    break;
                                case SettingsMode.OnePerSite:
                                    foreach (var site in sites)
                                    {
                                        var record = records.FirstOrDefault(r => r.SiteId == site.Id);
                                        var recordSettings = record == null
                                            ? GetInstance(schema.Type.FullName)
                                            : DeserializeSettings(records.First(), schema);

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

        private SettingsBase DeserializeSettings(SettingsRecord record, SettingsSchema schema)
        {
            return (SettingsBase) JsonConvert.DeserializeObject(record.Data, schema.Type);
        }
    }
}