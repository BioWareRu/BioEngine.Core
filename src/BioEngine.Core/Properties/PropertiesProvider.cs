using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private bool _batchMode;
        private bool _checkIfExists = true;

        private static readonly Dictionary<Type, PropertiesSchema> Schema = new Dictionary<Type, PropertiesSchema>();

        public PropertiesProvider(BioContext dbContext)
        {
            _dbContext = dbContext;
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

        public static void RegisterBioEngineProperties<TProperties, TEntity>() where TProperties : PropertiesSet, new()
            where TEntity : IEntity, new()
        {
            RegisterBioEngineProperties<TProperties>(typeof(TEntity));
        }

        public static void RegisterBioEngineSectionProperties<TProperties>()
            where TProperties : PropertiesSet, new()
        {
            RegisterBioEngineProperties<TProperties>(typeof(Section), PropertiesRegistrationType.Section);
        }

        public static void RegisterBioEngineContentProperties<TProperties>()
            where TProperties : PropertiesSet, new()
        {
            RegisterBioEngineProperties<TProperties>(typeof(Post), PropertiesRegistrationType.Content);
        }

        private static void RegisterBioEngineProperties<TProperties>(Type entityType = null,
            PropertiesRegistrationType registrationType = PropertiesRegistrationType.Entity)
            where TProperties : PropertiesSet, new()
        {
            var schema = Schema.ContainsKey(typeof(TProperties)) ? Schema[typeof(TProperties)] : null;
            if (schema == null)
            {
                schema = PropertiesSchema.Create<TProperties>();

                Schema.Add(typeof(TProperties), schema);
            }

            schema.AddRegistration(registrationType, entityType);
        }

        public static PropertiesSet GetInstance(string className)
        {
            var type = Schema.Keys.FirstOrDefault(k => k.FullName == className);
            if (type != null)
            {
                return (PropertiesSet)Activator.CreateInstance(type);
            }

            throw new ArgumentException($"Class {className} is not registered in properties provider");
        }

        public static PropertiesSchema GetSchema(Type propertiesType)
        {
            if (!Schema.ContainsKey(propertiesType))
            {
                throw new ArgumentException($"Type {propertiesType} is not registered in properties provider!");
            }

            return Schema[propertiesType];
        }

        [PublicAPI]
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

        [PublicAPI]
        public async Task<TProperties> GetAsync<TProperties>(IEntity entity, Guid? siteId = null)
            where TProperties : PropertiesSet, new()
        {
            if (!(entity.Properties.FirstOrDefault(x => x.Key == typeof(TProperties).FullName)?.Properties
                .FirstOrDefault(x => x.SiteId == siteId)?.Value is TProperties properties))
            {
                properties = new TProperties();
                var propertiesRecord = await LoadFromDatabaseAsync(properties, entity, siteId);
                if (propertiesRecord != null)
                {
                    properties = JsonConvert.DeserializeObject<TProperties>(propertiesRecord.Data);
                }
            }

            return properties;
        }

        [PublicAPI]
        public async Task<bool> SetAsync<TProperties>(TProperties properties)
            where TProperties : PropertiesSet, new()
        {
            var record = await LoadFromDatabaseAsync<TProperties>() ?? new PropertiesRecord
            {
                Key = typeof(TProperties).FullName
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

        [PublicAPI]
        public async Task<bool> SetAsync<TProperties>(TProperties properties, IEntity entity, Guid? siteId = null)
            where TProperties : PropertiesSet, new()
        {
            var record = await LoadFromDatabaseAsync(properties, entity) ?? new PropertiesRecord
            {
                Key = properties.GetType().FullName,
                EntityType = entity.GetType().FullName,
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

        private Task<PropertiesRecord> LoadFromDatabaseAsync<TProperties>()
            where TProperties : PropertiesSet, new()
        {
            return _checkIfExists
                ? Task.FromResult((PropertiesRecord)null)
                : _dbContext.Properties.FirstOrDefaultAsync(s =>
                    s.Key == typeof(TProperties).FullName
                    && s.EntityType == null && s.EntityId == null);
        }

        private Task<PropertiesRecord> LoadFromDatabaseAsync<TProperties>(TProperties properties, IEntity entity,
            Guid? siteId = null)
            where TProperties : PropertiesSet, new()
        {
            return _checkIfExists
                ? Task.FromResult((PropertiesRecord)null)
                : _dbContext.Properties.FirstOrDefaultAsync(s =>
                    s.Key == properties.GetType().FullName
                    && s.EntityType == entity.GetType().FullName && s.EntityId == entity.Id &&
                    (siteId == null || s.SiteId == siteId));
        }

        public async Task LoadPropertiesAsync<T>(IEnumerable<T> entities) where T : class, IEntity
        {
            var entitiesArray = entities as T[] ?? entities.ToArray();
            if (entitiesArray.Any())
            {
                var sites = await _dbContext.Sites.ToListAsync();
                var groups = entitiesArray.GroupBy(e => e.GetType().FullName);
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

                    if (groupEntity is Post)
                    {
                        schemas.AddRange(Schema.Where(schema =>
                            schema.Value.IsRegisteredForContent()));
                    }

                    var propertiesRecords = await _dbContext.Properties.Where(s =>
                        s.EntityType == entityType && ids.Contains(s.EntityId)).ToListAsync();

                    foreach (var entity in group)
                    {
                        entity.Properties = new List<PropertiesEntry>();
                        foreach (var schema in schemas.Select(s => s.Value))
                        {
                            var entry = new PropertiesEntry(schema.Key, schema);

                            var records = propertiesRecords
                                .Where(s => s.EntityId == entity.Id && s.Key == schema.Key)
                                .ToList();

                            switch (entry.Schema.Mode)
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
                                            GetInstance(schema.Type.FullName)));
                                    }

                                    break;
                                case PropertiesQuantity.OnePerSite:
                                    foreach (var site in sites)
                                    {
                                        var record = records.FirstOrDefault(r => r.SiteId == site.Id);
                                        var propertiesSet = record == null
                                            ? GetInstance(schema.Type.FullName)
                                            : DeserializeProperties(records.First(), schema);

                                        entry.Properties.Add(new PropertiesValue(site.Id, propertiesSet));
                                    }

                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            entity.Properties.Add(entry);
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
