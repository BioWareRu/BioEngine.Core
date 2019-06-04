using System;
using System.Linq;
using System.Linq.Expressions;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace BioEngine.Core.DB
{
    public static class ModelBuilderContextExtensions
    {
        public static void RegisterSectionEntityConversions<TEntity>(this ModelBuilder modelBuilder)
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

        public static void RegisterSiteEntityConversions<TEntity>(this ModelBuilder modelBuilder)
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

        public static void RegisterJsonConversion<TEntity, TProperty>(this ModelBuilder modelBuilder,
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

        public static void RegisterDataConversion<TEntity, TData>(this ModelBuilder modelBuilder)
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

        public static void RegisterDiscriminator<TBase>(this ModelBuilder modelBuilder, Type objectType,
            string discriminator)
            where TBase : class
        {
            var discriminatorBuilder =
                modelBuilder.Entity<TBase>().HasDiscriminator<string>(nameof(Section.Type));
            var method = discriminatorBuilder.GetType().GetMethods()
                .First(m => m.Name == nameof(DiscriminatorBuilder.HasValue) && m.IsGenericMethod);
            var typedMethod = method?.MakeGenericMethod(objectType);
            typedMethod?.Invoke(discriminatorBuilder, new object[] {discriminator});
        }
    }
}
