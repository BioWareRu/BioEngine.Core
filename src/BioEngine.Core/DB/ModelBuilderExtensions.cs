using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BioEngine.Core.DB
{
    public static class ModelBuilderExtensions
    {
        private static readonly ConcurrentDictionary<string, Type>
            BlockTypes = new ConcurrentDictionary<string, Type>();

        private static readonly ConcurrentDictionary<string, Type> Entities = new ConcurrentDictionary<string, Type>();

        private static bool _requireArrayConversion;

        public static ContentBlock CreateBlock(string key)
        {
            if (BlockTypes.ContainsKey(key))
            {
                return Activator.CreateInstance(BlockTypes[key]) as ContentBlock;
            }

            return null;
        }

        public static void RequireArrayConversion()
        {
            _requireArrayConversion = true;
        }

        public static bool IsArrayConversionRequired()
        {
            return _requireArrayConversion;
        }

        public static void RegisterContentBlock<TBlock, TBlockData>(this ModelBuilder modelBuilder, ILogger logger)
            where TBlock : ContentBlock<TBlockData> where TBlockData : ContentBlockData, new()
        {
            var key = EntityExtensions.GetKey<TBlock>();
            logger.LogInformation(
                "Register content block type {type} - {entityType} ({dataType})", key,
                typeof(TBlock),
                typeof(TBlockData));
            modelBuilder.RegisterDiscriminator<ContentBlock, TBlock>(key);
            modelBuilder.RegisterDataConversion<TBlock, TBlockData>();
            if (!BlockTypes.ContainsKey(key))
            {
                BlockTypes.TryAdd(key, typeof(TBlock));
            }
        }

        public static void RegisterSection<TSection, TSectionData>(this ModelBuilder modelBuilder,
            ILogger<BioContext> logger)
            where TSection : Section<TSectionData> where TSectionData : ITypedData, new()
        {
            var key = EntityExtensions.GetKey<TSection>();
            logger.LogInformation("Register section type {type} - {entityType} ({dataType})", key,
                typeof(TSection),
                typeof(TSectionData));
            modelBuilder.RegisterEntity<TSection>();
            modelBuilder.RegisterDiscriminator<Section, TSection>(key);
            modelBuilder.RegisterDataConversion<TSection, TSectionData>();
            if (_requireArrayConversion)
            {
                modelBuilder.RegisterSiteEntityConversions<TSection>();
            }
        }

        public static void RegisterContentItem<TContentItem>(this ModelBuilder modelBuilder, ILogger<BioContext> logger)
            where TContentItem : class, IContentItem
        {
            logger.LogInformation("Register content item type {type} - {entityType}",
                EntityExtensions.GetKey<TContentItem>(),
                typeof(TContentItem));
            if (_requireArrayConversion)
            {
                modelBuilder.RegisterSiteEntityConversions<TContentItem>();
                modelBuilder.RegisterSectionEntityConversions<TContentItem>();
            }

            modelBuilder.RegisterEntity<TContentItem>();
            modelBuilder.Entity<TContentItem>().Property(i => i.Title).IsRequired();
            modelBuilder.Entity<TContentItem>().Property(i => i.Url).IsRequired();
            modelBuilder.Entity<TContentItem>().Ignore(i => i.Blocks);
            modelBuilder.Entity<TContentItem>().Ignore(i => i.Sections);
            modelBuilder.Entity<TContentItem>().Ignore(i => i.Tags);
            modelBuilder.Entity<TContentItem>().Ignore(i => i.PublicRouteName);
            modelBuilder.Entity<TContentItem>().HasIndex(i => i.SiteIds);
            modelBuilder.Entity<TContentItem>().HasIndex(i => i.TagIds);
            modelBuilder.Entity<TContentItem>().HasIndex(i => i.SectionIds);
            modelBuilder.Entity<TContentItem>().HasIndex(i => i.IsPublished);
            modelBuilder.Entity<TContentItem>().HasIndex(i => i.Url).IsUnique();
        }

        public static void RegisterEntity<TEntity>(this ModelBuilder modelBuilder) where TEntity : class, IEntity
        {
            modelBuilder.Entity<TEntity>();
            var key = EntityExtensions.GetKey<TEntity>();
            if (!Entities.ContainsKey(key))
            {
                Entities.TryAdd(key, typeof(TEntity));
            }
        }

        public static void RegisterSiteEntity<TSiteEntity>(this ModelBuilder modelBuilder)
            where TSiteEntity : class, ISiteEntity
        {
            modelBuilder.RegisterEntity<TSiteEntity>();
            modelBuilder.Entity<TSiteEntity>().HasIndex(e => e.SiteIds);
            if (_requireArrayConversion)
            {
                modelBuilder.RegisterSiteEntityConversions<TSiteEntity>();
            }
        }

        private static void RegisterSectionEntityConversions<TEntity>(this ModelBuilder modelBuilder)
            where TEntity : class, ISectionEntity
        {
            modelBuilder
                .Entity<TEntity>()
                .Property(s => s.SectionIds)
                .HasColumnType("jsonb");
            modelBuilder
                .Entity<TEntity>()
                .Property(s => s.TagIds)
                .HasColumnType("jsonb");
            
            if (_requireArrayConversion)
            {
                modelBuilder.RegisterJsonStringConversion<TEntity, Guid[]>(e => e.SectionIds);
                modelBuilder.RegisterJsonStringConversion<TEntity, Guid[]>(e => e.TagIds);
            }
        }

        public static void RegisterSiteEntityConversions<TEntity>(this ModelBuilder modelBuilder)
            where TEntity : class, ISiteEntity
        {
            modelBuilder
                .Entity<TEntity>()
                .Property(s => s.SiteIds)
                .HasColumnType("jsonb");
            if (_requireArrayConversion)
            {
                modelBuilder.RegisterJsonStringConversion<TEntity, Guid[]>(e => e.SiteIds);
            }
        }

        public static void RegisterJsonStringConversion<TEntity, TProperty>(this ModelBuilder modelBuilder,
            Expression<Func<TEntity, TProperty>> propertySelector)
            where TEntity : class
        {
            modelBuilder
                .Entity<TEntity>()
                .Property(propertySelector)
                .HasConversion(data => JsonConvert.SerializeObject(data),
                    json => JsonConvert.DeserializeObject<TProperty>(json));
        }

        private static void RegisterDataConversion<TEntity, TData>(this ModelBuilder modelBuilder)
            where TEntity : class, ITypedEntity<TData> where TData : ITypedData, new()
        {
            modelBuilder
                .Entity<TEntity>()
                .Property(e => e.Data)
                .HasColumnType("jsonb")
                .HasColumnName(nameof(ITypedEntity<TData>.Data));
            if (_requireArrayConversion)
            {
                modelBuilder.RegisterJsonStringConversion<TEntity, TData>(e => e.Data);
            }
        }

        private static void RegisterDiscriminator<TBase, TObject>(this ModelBuilder modelBuilder,
            string discriminator)
            where TBase : class
        {
            var discriminatorBuilder =
                modelBuilder.Entity<TBase>().HasDiscriminator<string>(nameof(Section.Type));
            discriminatorBuilder.HasValue<TObject>(discriminator);
        }
    }
}
