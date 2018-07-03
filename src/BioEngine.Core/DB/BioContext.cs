﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using BioEngine.Core.Entities;
using BioEngine.Core.Interfaces;
using BioEngine.Core.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace BioEngine.Core.DB
{
    public sealed class BioContext : DbContext
    {
        public static TypesProvider TypesProvider;

        public BioContext(DbContextOptions<BioContext> options) : base(options)
        {
        }

        public DbSet<Site> Sites { get; set; }
        public DbSet<Tag> Tags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ForNpgsqlUseIdentityByDefaultColumns();

            RegisterJsonConversion<Section, StorageItem>(modelBuilder, s => s.Logo);
            RegisterJsonConversion<Section, StorageItem>(modelBuilder, s => s.LogoSmall);

            modelBuilder.Entity<Section>().HasIndex(s => s.SiteIds);
            modelBuilder.Entity<Section>().HasIndex(s => s.IsPublished);
            modelBuilder.Entity<Section>().HasIndex(s => s.Url);

            modelBuilder.Entity<ContentItem>().HasIndex(i => i.SiteIds);
            modelBuilder.Entity<ContentItem>().HasIndex(i => i.TagIds);
            modelBuilder.Entity<ContentItem>().HasIndex(i => i.SectionIds);
            modelBuilder.Entity<ContentItem>().HasIndex(i => i.IsPublished);
            modelBuilder.Entity<ContentItem>().HasIndex(i => i.Type);
            modelBuilder.Entity<ContentItem>().HasIndex(i => i.Url);

            var dataConversionRegistrationMethod = GetType().GetMethod(nameof(RegisterDataConversion),
                BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var sectionType in TypesProvider.GetSectionTypes())
            {
                Console.WriteLine($"Register section type {sectionType}");
                RegisterDiscriminator<Section, int>(modelBuilder, sectionType.type, sectionType.discriminator);
                dataConversionRegistrationMethod?.MakeGenericMethod(sectionType.type, sectionType.dataType)
                    .Invoke(this, new object[] {modelBuilder});
            }

            foreach (var contentType in TypesProvider.GetContentTypes())
            {
                Console.WriteLine($"Register content type {contentType}");
                RegisterDiscriminator<ContentItem, int>(modelBuilder, contentType.type,
                    contentType.discriminator);
                dataConversionRegistrationMethod?.MakeGenericMethod(contentType.type, contentType.dataType)
                    .Invoke(this, new object[] {modelBuilder});
            }
        }

        private void RegisterDiscriminator<TBase, TDescriminator>(ModelBuilder modelBuilder, Type sectionType,
            TDescriminator discriminator) where TBase : class
        {
            var discriminatorBuilder =
                modelBuilder.Entity<TBase>().HasDiscriminator<TDescriminator>(nameof(Section.Type));
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
    }
}