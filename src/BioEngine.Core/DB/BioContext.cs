using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using BioEngine.Core.Entities;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BioEngine.Core.DB
{
    public sealed class BioContext : DbContext
    {
        // ReSharper disable once SuggestBaseTypeForParameter
        public BioContext(DbContextOptions<BioContext> options) : base(options)
        {
        }

        [UsedImplicitly] public DbSet<Site> Sites { get; set; }
        [UsedImplicitly] public DbSet<Tag> Tags { get; set; }
        [UsedImplicitly] public DbSet<Page> Pages { get; set; }
        [UsedImplicitly] public DbSet<Menu> Menus { get; set; }
        public DbSet<ContentBlock> Blocks { get; set; }
        [UsedImplicitly] public DbSet<StorageItem> StorageItems { get; set; }
        public DbSet<PostVersion> PostVersions { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public DbSet<PropertiesRecord> Properties { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ForNpgsqlUseIdentityByDefaultColumns();

            RegisterJsonConversion<Menu, List<MenuItem>>(modelBuilder, s => s.Items);
            RegisterJsonConversion<Section, StorageItem>(modelBuilder, s => s.Logo);
            RegisterJsonConversion<Section, StorageItem>(modelBuilder, s => s.LogoSmall);
            RegisterJsonConversion<StorageItem, StorageItemPictureInfo>(modelBuilder, s => s.PictureInfo);
            modelBuilder.Entity<StorageItem>().Property(i => i.PublicUri)
                .HasConversion(u => u.ToString(), s => new Uri(s));
            if (Database.IsInMemory())
            {
                RegisterSiteEntityConversions<Page>(modelBuilder);
                RegisterSiteEntityConversions<Post>(modelBuilder);
                RegisterSiteEntityConversions<Section>(modelBuilder);
                RegisterSectionEntityConversions<Post>(modelBuilder);
            }

            modelBuilder.Entity<Section>().HasIndex(s => s.SiteIds);
            modelBuilder.Entity<Section>().HasIndex(s => s.IsPublished);
            modelBuilder.Entity<Section>().HasIndex(s => s.Type);
            modelBuilder.Entity<Section>().HasIndex(s => s.Url);

            modelBuilder.Entity<Post>().HasIndex(i => i.SiteIds);
            modelBuilder.Entity<Post>().HasIndex(i => i.TagIds);
            modelBuilder.Entity<Post>().HasIndex(i => i.SectionIds);
            modelBuilder.Entity<Post>().HasIndex(i => i.IsPublished);
            modelBuilder.Entity<Post>().HasIndex(i => i.Url).IsUnique();

            var dataConversionRegistrationMethod = typeof(BioContext).GetMethod(nameof(RegisterDataConversion),
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            var siteConversionsRegistrationMethod = typeof(BioContext).GetMethod(nameof(RegisterSiteEntityConversions),
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            var metadataManager = this.GetInfrastructure().GetService<BioEntityMetadataManager>();
            var logger = this.GetInfrastructure().GetRequiredService<ILogger<BioContext>>();
            foreach (var entityMetadata in metadataManager.GetBlocksMetadata())
            {
                logger.LogInformation(
                    "Register content block type {type} - {entityType} ({dataType})", entityMetadata.Type,
                    entityMetadata.ObjectType,
                    entityMetadata.DataType);
                RegisterDiscriminator<ContentBlock>(modelBuilder, entityMetadata.ObjectType,
                    entityMetadata.Type);
                dataConversionRegistrationMethod
                    ?.MakeGenericMethod(entityMetadata.ObjectType, entityMetadata.DataType)
                    .Invoke(this, new object[] {modelBuilder});
            }

            foreach (var entityMetadata in metadataManager.GetSectionsMetadata())
            {
                logger.LogInformation("Register section type {type} - {entityType} ({dataType})", entityMetadata.Type,
                    entityMetadata.ObjectType,
                    entityMetadata.DataType);
                RegisterDiscriminator<Section>(modelBuilder, entityMetadata.ObjectType,
                    entityMetadata.Type);
                if (Database.IsInMemory())
                {
                    siteConversionsRegistrationMethod?.MakeGenericMethod(entityMetadata.ObjectType)
                        .Invoke(this, new object[] {modelBuilder});
                }

                dataConversionRegistrationMethod
                    ?.MakeGenericMethod(entityMetadata.ObjectType, entityMetadata.DataType)
                    .Invoke(this, new object[] {modelBuilder});
            }

            logger.LogInformation("Done registering");
        }

        private void RegisterDiscriminator<TBase>(ModelBuilder modelBuilder, Type objectType, string discriminator)
            where TBase : class
        {
            var discriminatorBuilder =
                modelBuilder.Entity<TBase>().HasDiscriminator<string>(nameof(Section.Type));
            var method = discriminatorBuilder.GetType().GetMethods()
                .First(m => m.Name == nameof(DiscriminatorBuilder.HasValue) && m.IsGenericMethod);
            var typedMethod = method?.MakeGenericMethod(objectType);
            typedMethod?.Invoke(discriminatorBuilder, new object[] {discriminator});
        }

        private void RegisterDataConversion<TEntity, TData>(ModelBuilder modelBuilder)
            where TEntity : class, ITypedEntity<TData> where TData : ITypedData, new()
        {
            modelBuilder
                .Entity<TEntity>()
                .Property(e => e.Data)
                .HasColumnType("jsonb")
                .HasColumnName(nameof(ITypedEntity<TData>.Data))
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<TData>(v));
        }

        private void RegisterJsonConversion<TEntity, TProperty>(ModelBuilder modelBuilder,
            Expression<Func<TEntity, TProperty>> propertySelector)
            where TEntity : class
        {
            modelBuilder
                .Entity<TEntity>()
                .Property(propertySelector)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<TProperty>(v));
        }


        private void RegisterSiteEntityConversions<TEntity>(ModelBuilder modelBuilder)
            where TEntity : class, ISiteEntity
        {
            modelBuilder
                .Entity<TEntity>()
                .Property(s => s.SiteIds)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Guid[]>(v));
        }

        private void RegisterSectionEntityConversions<TEntity>(ModelBuilder modelBuilder)
            where TEntity : class, ISectionEntity
        {
            modelBuilder
                .Entity<TEntity>()
                .Property(s => s.SectionIds)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Guid[]>(v));
            modelBuilder
                .Entity<TEntity>()
                .Property(s => s.TagIds)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Guid[]>(v));
        }
    }
}
