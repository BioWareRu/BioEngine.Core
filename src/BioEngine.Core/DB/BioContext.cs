using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using BioEngine.Core.Entities;
using BioEngine.Core.Interfaces;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
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
        public DbSet<PostBlock> Blocks { get; set; }
        public DbSet<StorageItem> StorageItems { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public DbSet<PropertiesRecord> Properties { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ForNpgsqlUseIdentityByDefaultColumns();

            RegisterJsonConversion<Menu, List<MenuItem>>(modelBuilder, s => s.Items);
            RegisterJsonConversion<StorageItem, StorageItemPictureInfo>(modelBuilder, s => s.PictureInfo);
            modelBuilder.Entity<StorageItem>().Property(i => i.PublicUri)
                .HasConversion(u => u.ToString(), s => new Uri(s));
            if (Database.IsInMemory())
            {
                RegisterSiteEntityConversions<Page, int>(modelBuilder);
                RegisterSiteEntityConversions<Post, int>(modelBuilder);
                RegisterSectionEntityConversions<Post, int>(modelBuilder);
            }

            modelBuilder.Entity<Section>().HasIndex(s => s.SiteIds);
            modelBuilder.Entity<Section>().HasIndex(s => s.IsPublished);
            modelBuilder.Entity<Section>().HasIndex(s => s.Type);
            modelBuilder.Entity<Section>().HasIndex(s => s.Url);

            modelBuilder.Entity<Post>().HasIndex(i => i.SiteIds);
            modelBuilder.Entity<Post>().HasIndex(i => i.TagIds);
            modelBuilder.Entity<Post>().HasIndex(i => i.SectionIds);
            modelBuilder.Entity<Post>().HasIndex(i => i.IsPublished);
            modelBuilder.Entity<Post>().HasIndex(i => i.Url);

            var dataConversionRegistrationMethod = typeof(BioContext).GetMethod(nameof(RegisterDataConversion),
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            var siteConversionsRegistrationMethod = typeof(BioContext).GetMethod(nameof(RegisterSiteEntityConversions),
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            var metadataEntities = this.GetInfrastructure().GetServices<EntityMetadata>()?.ToArray();
            if (metadataEntities != null)
            {
                foreach (var entityMetadata in metadataEntities)
                {
                    if (typeof(Section).IsAssignableFrom(entityMetadata.EntityType))
                    {
                        Console.WriteLine($"Register section type {entityMetadata}");
                        RegisterDiscriminator<Section>(modelBuilder, entityMetadata.EntityType,
                            entityMetadata.EntityType.FullName);

                        if (Database.IsInMemory())
                        {
                            siteConversionsRegistrationMethod?.MakeGenericMethod(entityMetadata.EntityType,
                                    entityMetadata.EntityType.GetProperty("Id")?.PropertyType)
                                .Invoke(this, new object[] {modelBuilder});
                        }
                    }
                    else if (typeof(PostBlock).IsAssignableFrom(entityMetadata.EntityType))
                    {
                        Console.WriteLine(
                            $"Register content block type {entityMetadata.EntityType} ({entityMetadata.DataType})");
                        RegisterDiscriminator<PostBlock>(modelBuilder, entityMetadata.EntityType,
                            entityMetadata.EntityType.FullName);
                    }

                    dataConversionRegistrationMethod
                        ?.MakeGenericMethod(entityMetadata.EntityType, entityMetadata.DataType)
                        .Invoke(this, new object[] {modelBuilder});
                }
            }

            Console.WriteLine("Done registering");
        }

        private void RegisterDiscriminator<TBase>(ModelBuilder modelBuilder, Type sectionType, string discriminator)
            where TBase : class
        {
            var discriminatorBuilder =
                modelBuilder.Entity<TBase>().HasDiscriminator<string>(nameof(Section.Type));
            var method = discriminatorBuilder.GetType().GetMethods()
                .First(m => m.Name == nameof(DiscriminatorBuilder.HasValue) && m.IsGenericMethod);
            var typedMethod = method?.MakeGenericMethod(sectionType);
            typedMethod?.Invoke(discriminatorBuilder, new object[] {discriminator});
        }

        private void RegisterDataConversion<TEntity, TData>(ModelBuilder modelBuilder)
            where TEntity : class, ITypedEntity<TData> where TData : TypedData, new()
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


        private void RegisterSiteEntityConversions<TEntity, TPk>(ModelBuilder modelBuilder)
            where TEntity : class, ISiteEntity<TPk>
        {
            modelBuilder
                .Entity<TEntity>()
                .Property(s => s.SiteIds)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<int[]>(v));
        }

        private void RegisterSectionEntityConversions<TEntity, TPk>(ModelBuilder modelBuilder)
            where TEntity : class, ISectionEntity<TPk>
        {
            modelBuilder
                .Entity<TEntity>()
                .Property(s => s.SectionIds)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<int[]>(v));
            modelBuilder
                .Entity<TEntity>()
                .Property(s => s.TagIds)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<int[]>(v));
        }
    }
}