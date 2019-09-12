using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BioEngine.Core.Properties
{
    [UsedImplicitly]
    public class PropertiesProvider
    {
        private readonly BioContext _dbContext;
        private readonly BioEntitiesManager _entitiesManager;
        private bool _batchMode;
        private bool _checkIfExists = true;

        private readonly Dictionary<Guid, List<PropertiesEntry>> _properties =
            new Dictionary<Guid, List<PropertiesEntry>>();

        private static readonly ConcurrentDictionary<string, PropertiesSchema> Schema =
            new ConcurrentDictionary<string, PropertiesSchema>();

        public PropertiesProvider(BioContext dbContext, BioEntitiesManager entitiesManager)
        {
            _dbContext = dbContext;
            _entitiesManager = entitiesManager;
        }

        public void BeginBatch()
        {
            _batchMode = true;
        }

        public Task FinishBatchAsync()
        {
            _batchMode = false;
            return WriteChangesAsync();
        }

        private async Task WriteChangesAsync()
        {
            if (!_batchMode)
            {
                await _dbContext.SaveChangesAsync();
            }
        }

        public void DisableChecks()
        {
            _checkIfExists = false;
        }

        public void EnableChecks()
        {
            _checkIfExists = true;
        }

        public static void RegisterBioEngineProperties<TProperties, TEntity>(string key)
            where TProperties : PropertiesSet, new()
            where TEntity : IEntity, new()
        {
            RegisterBioEngineProperties<TProperties>(key, typeof(TEntity));
        }

        public static void RegisterBioEngineSectionProperties<TProperties>(string key)
            where TProperties : PropertiesSet, new()
        {
            RegisterBioEngineProperties<TProperties>(key, typeof(Section), PropertiesRegistrationType.Section);
        }

        public static void RegisterBioEngineContentProperties<TProperties>(string key)
            where TProperties : PropertiesSet, new()
        {
            RegisterBioEngineProperties<TProperties>(key, typeof(ContentItem), PropertiesRegistrationType.Content);
        }

        private static void RegisterBioEngineProperties<TProperties>(string key, Type entityType,
            PropertiesRegistrationType registrationType = PropertiesRegistrationType.Entity)
            where TProperties : PropertiesSet, new()
        {
            var schema = Schema.ContainsKey(key) ? Schema[key] : null;
            if (schema == null)
            {
                schema = PropertiesSchema.Create<TProperties>(key);

                Schema.TryAdd(key, schema);
            }

            schema.AddRegistration(key, registrationType, entityType);
        }

        public static PropertiesSet GetInstance(string key)
        {
            var type = Schema.ContainsKey(key) ? Schema[key] : null;
            if (type != null)
            {
                if (Activator.CreateInstance(type.Type) is PropertiesSet set)
                {
                    return set;
                }
            }

            throw new ArgumentException($"Class {key} is not registered in properties provider");
        }

        public static PropertiesSchema GetSchema(string key)
        {
            if (!Schema.ContainsKey(key))
            {
                throw new ArgumentException($"Type {key} is not registered in properties provider!");
            }

            return Schema[key];
        }

        [
            PublicAPI]
        public async Task<List<PropertiesEntry>> GetAsync(IBioEntity entity)
        {
            if (!_properties.ContainsKey(entity.Id))
            {
                await LoadPropertiesAsync(new[] {entity});
            }

            return _properties[entity.Id];
        }

        [
            PublicAPI]
        public async Task<TProperties> GetAsync<TProperties>() where TProperties : PropertiesSet, new()
        {
            var properties = new TProperties();
            var propertiesRecord = await LoadFromDatabaseAsync<TProperties>();
            if (propertiesRecord != null)
            {
                properties = JsonConvert.DeserializeObject<TProperties>(propertiesRecord.Data);
            }

            return properties;
        }

        [
            PublicAPI]
        public async Task<TProperties> GetAsync<TProperties>(IBioEntity entity, Guid? siteId = null)
            where TProperties : PropertiesSet, new()
        {
            var schema = Schema.FirstOrDefault(s => s.Value.Type == typeof(TProperties));
            var entityProperties = _properties.ContainsKey(entity.Id)
                ? _properties[entity.Id]
                : new List<PropertiesEntry>();
            if (!(entityProperties.Find(x => x.Key == schema.Key)?.Properties
                .Find(x => x.SiteId == siteId)?.Value is TProperties properties))
            {
                properties = new TProperties();
                var propertiesRecord = await LoadFromDatabaseAsync(schema.Value, entity, siteId);
                if (propertiesRecord != null)
                {
                    properties = JsonConvert.DeserializeObject<TProperties>(propertiesRecord.Data);
                }
            }

            return properties;
        }

        [
            PublicAPI]
        public async Task<bool> SetAsync<TProperties>(TProperties properties)
            where TProperties : PropertiesSet, new()
        {
            var schema = Schema.Where(s => s.Value.Type == properties.GetType()).Select(s => s.Value)
                .FirstOrDefault();
            if (schema == null)
            {
                throw new ArgumentException($"Schema for type {typeof(TProperties)} is not registered");
            }

            var record = await LoadFromDatabaseAsync<TProperties>() ?? new PropertiesRecord {Key = schema.Key};


            record.DateUpdated = DateTimeOffset.UtcNow;
            record.Data = JsonConvert.SerializeObject(properties);
            if (record.Id != Guid.Empty)
            {
                _dbContext.Update(record);
            }
            else
            {
                _dbContext.Add(record);
            }

            await WriteChangesAsync();
            return true;
        }

        [
            PublicAPI]
        public async Task<bool> SetAsync<TProperties>(TProperties properties, IBioEntity entity,
            Guid? siteId = null)
            where TProperties : PropertiesSet, new()
        {
            var schema = Schema.Where(s => s.Value.Type == properties.GetType()).Select(s => s.Value)
                .FirstOrDefault();
            if (schema == null)
            {
                throw new ArgumentException($"Schema for type {typeof(TProperties)} is not registered");
            }

            var record = await LoadFromDatabaseAsync(schema, entity, siteId) ?? new PropertiesRecord
            {
                Key = schema.Key,
                EntityType = _entitiesManager.GetKey(entity),
                EntityId = entity.Id,
                SiteId = siteId
            };


            record.DateUpdated = DateTimeOffset.UtcNow;
            record.Data = JsonConvert.SerializeObject(properties);
            if (record.Id != Guid.Empty)
            {
                _dbContext.Update(record);
            }
            else
            {
                _dbContext.Add(record);
            }

            await WriteChangesAsync();
            return true;
        }

        private async Task<PropertiesRecord?> LoadFromDatabaseAsync<TProperties>()
            where TProperties : PropertiesSet, new()
        {
            var schema = Schema.Where(s => s.Value.Type == typeof(TProperties)).Select(s => s.Value)
                .FirstOrDefault();
            return schema == null || !_checkIfExists
                ? null
                : await _dbContext.Properties.FirstOrDefaultAsync(s =>
                    s.Key == schema.Key
                    && s.EntityType == null && s.EntityId == null);
        }

        private async Task<PropertiesRecord?> LoadFromDatabaseAsync(PropertiesSchema schema, IBioEntity entity,
            Guid? siteId = null)
        {
            return schema == null || !_checkIfExists
                ? null
                : await _dbContext.Properties.FirstOrDefaultAsync(s =>
                    s.Key == schema.Key
                    && s.EntityType == _entitiesManager.GetKey(entity) && s.EntityId == entity.Id &&
                    (siteId == null || s.SiteId == siteId.Value));
        }

        public async Task LoadPropertiesAsync<T>(IEnumerable<T> entities) where T : class, IBioEntity
        {
            var entitiesArray = entities as T[] ?? entities.ToArray();
            if (entitiesArray.Any())
            {
                var sites = await _dbContext.Sites.ToListAsync();
                var groups = entitiesArray.GroupBy(e => _entitiesManager.GetKey(e));
                foreach (var group in groups)
                {
                    var ids = group.Where(e => e.Id != default).Select(e => e.Id);
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

                    var propertiesRecords = await _dbContext.Properties.Where(s =>
                        s.EntityType == entityType && ids.Contains(s.EntityId)).ToListAsync();

                    foreach (var entity in group)
                    {
                        if (!_properties.ContainsKey(entity.Id))
                        {
                            _properties.Add(entity.Id, new List<PropertiesEntry>());
                        }

                        foreach (var schema in schemas.Select(s => s.Value).OrderBy(s => s.Key))
                        {
                            var entry = new PropertiesEntry(schema.Key);

                            var records = propertiesRecords
                                .Where(s => s.EntityId == entity.Id && s.Key == schema.Key)
                                .ToList();

                            switch (schema.Mode)
                            {
                                case PropertiesQuantity.OnePerEntity:
                                    if (records.Any())
                                    {
                                        entry.Properties.Add(new PropertiesValue(null,
                                            DeserializeProperties(records.First(), schema)));
                                    }
                                    else
                                    {
                                        entry.Properties.Add(new PropertiesValue(null,
                                            GetInstance(schema.Key)));
                                    }

                                    break;
                                case PropertiesQuantity.OnePerSite:
                                    foreach (var site in sites)
                                    {
                                        var record = records.FirstOrDefault(r => r.SiteId == site.Id);
                                        var propertiesSet = record == null
                                            ? GetInstance(schema.Key)
                                            : DeserializeProperties(records.First(), schema);

                                        entry.Properties.Add(new PropertiesValue(site.Id, propertiesSet));
                                    }

                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            _properties[entity.Id].Add(entry);
                        }
                    }
                }
            }
        }

        private PropertiesSet DeserializeProperties(PropertiesRecord record, PropertiesSchema schema)
        {
            return (PropertiesSet)JsonConvert.DeserializeObject(record.Data, schema.Type);
        }
    }
}
