using System;
using System.Collections.Generic;
using System.Reflection;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Entities;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BioEngine.Core.DB
{
    public sealed class BioContext : DbContext
    {
        // ReSharper disable once SuggestBaseTypeForParameter
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
        public BioContext(DbContextOptions<BioContext> options) : base(options)
        {
        }

        [UsedImplicitly] public DbSet<Site> Sites { get; set; }
        [UsedImplicitly] public DbSet<Tag> Tags { get; set; }
        [UsedImplicitly] public DbSet<Menu> Menus { get; set; }

        [UsedImplicitly] public DbSet<ContentItem> ContentItems { get; set; }
        [UsedImplicitly] public DbSet<Section> Sections { get; set; }
        [UsedImplicitly] public DbSet<ContentBlock> Blocks { get; set; }
        [UsedImplicitly] public DbSet<StorageItem> StorageItems { get; set; }
        public DbSet<ContentVersion> PostVersions { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public DbSet<PropertiesRecord> Properties { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.RegisterJsonConversion<Menu, List<MenuItem>>(s => s.Items);
            modelBuilder.RegisterJsonConversion<StorageItem, StorageItemPictureInfo>(s => s.PictureInfo);
            modelBuilder.Entity<StorageItem>().Property(i => i.PublicUri)
                .HasConversion(u => u.ToString(), s => new Uri(s));
            if (Database.IsInMemory())
            {
                modelBuilder.RegisterSiteEntityConversions<Section>();
                modelBuilder.RegisterSiteEntityConversions<ContentItem>();
                modelBuilder.RegisterSiteEntityConversions<Menu>();
            }
            else
            {
                modelBuilder.ForNpgsqlUseIdentityByDefaultColumns();
            }

            modelBuilder.Entity<Section>().HasIndex(s => s.SiteIds);
            modelBuilder.Entity<Section>().HasIndex(s => s.IsPublished);
            modelBuilder.Entity<Section>().HasIndex(s => s.Type);
            modelBuilder.Entity<Section>().HasIndex(s => s.Url);
            modelBuilder.Entity<Section>().HasMany(section => section.Blocks).WithOne().HasForeignKey(c => c.ContentId);

            modelBuilder.Entity<ContentItem>().HasIndex(i => i.SiteIds);
            modelBuilder.Entity<ContentItem>().HasIndex(i => i.TagIds);
            modelBuilder.Entity<ContentItem>().HasIndex(i => i.SectionIds);
            modelBuilder.Entity<ContentItem>().HasIndex(i => i.IsPublished);
            modelBuilder.Entity<ContentItem>().HasIndex(i => i.Url).IsUnique();
            modelBuilder.Entity<ContentItem>().HasMany(contentItem => contentItem.Blocks).WithOne()
                .HasForeignKey(c => c.ContentId);

            var dataConversionRegistrationMethod = typeof(ModelBuilderContextExtensions).GetMethod(
                nameof(ModelBuilderContextExtensions.RegisterDataConversion),
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            var siteConversionsRegistrationMethod = typeof(ModelBuilderContextExtensions).GetMethod(
                nameof(ModelBuilderContextExtensions.RegisterSiteEntityConversions),
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            var sectionEntityConversionsRegistrationMethod = typeof(ModelBuilderContextExtensions).GetMethod(
                nameof(ModelBuilderContextExtensions.RegisterSectionEntityConversions),
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            var entitiesManager = this.GetInfrastructure().GetRequiredService<BioEntitiesManager>();
            var logger = this.GetInfrastructure().GetRequiredService<ILogger<BioContext>>();
            foreach (var entityMetadata in entitiesManager.GetBlocksMetadata())
            {
                logger.LogInformation(
                    "Register content block type {type} - {entityType} ({dataType})", entityMetadata.Type,
                    entityMetadata.ObjectType,
                    entityMetadata.DataType);
                modelBuilder.RegisterDiscriminator<ContentBlock>(entityMetadata.ObjectType,
                    entityMetadata.Type);
                dataConversionRegistrationMethod
                    ?.MakeGenericMethod(entityMetadata.ObjectType, entityMetadata.DataType)
                    .Invoke(modelBuilder, new object[] {modelBuilder});
            }

            foreach (var entityMetadata in entitiesManager.GetSectionsMetadata())
            {
                logger.LogInformation("Register section type {type} - {entityType} ({dataType})", entityMetadata.Type,
                    entityMetadata.ObjectType,
                    entityMetadata.DataType);
                modelBuilder.RegisterDiscriminator<Section>(entityMetadata.ObjectType,
                    entityMetadata.Type);
                if (Database.IsInMemory())
                {
                    siteConversionsRegistrationMethod?.MakeGenericMethod(entityMetadata.ObjectType)
                        .Invoke(modelBuilder, new object[] {modelBuilder});
                }

                dataConversionRegistrationMethod
                    ?.MakeGenericMethod(entityMetadata.ObjectType, entityMetadata.DataType)
                    .Invoke(modelBuilder, new object[] {modelBuilder});
            }

            foreach (var entityMetadata in entitiesManager.GetContentItemsMetadata())
            {
                logger.LogInformation("Register content item type {type} - {entityType} ({dataType})",
                    entityMetadata.Type,
                    entityMetadata.ObjectType,
                    entityMetadata.DataType);
                modelBuilder.RegisterDiscriminator<ContentItem>(entityMetadata.ObjectType,
                    entityMetadata.Type);
                if (Database.IsInMemory())
                {
                    siteConversionsRegistrationMethod?.MakeGenericMethod(entityMetadata.ObjectType)
                        .Invoke(modelBuilder, new object[] {modelBuilder});

                    sectionEntityConversionsRegistrationMethod?.MakeGenericMethod(entityMetadata.ObjectType)
                        .Invoke(modelBuilder, new object[] {modelBuilder});
                }

                dataConversionRegistrationMethod
                    ?.MakeGenericMethod(entityMetadata.ObjectType, entityMetadata.DataType)
                    .Invoke(modelBuilder, new object[] {modelBuilder});
            }


            var entitiesTypes = entitiesManager.GetTypes();
            foreach (var registration in entitiesTypes)
            {
                modelBuilder.Entity(registration.Type);
                if (typeof(ISiteEntity).IsAssignableFrom(registration.Type) && Database.IsInMemory())
                {
                    siteConversionsRegistrationMethod?.MakeGenericMethod(registration.Type)
                        .Invoke(modelBuilder, new object[] {modelBuilder});
                }
            }

            foreach (var configureAction in entitiesManager.GetConfigureActions())
            {
                configureAction.Invoke(modelBuilder);
            }

            logger.LogInformation("Done registering");
        }
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
}
