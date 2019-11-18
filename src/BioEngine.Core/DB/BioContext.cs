using System;
using System.Collections.Generic;
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
        [UsedImplicitly] public DbSet<Section> Sections { get; set; }
        [UsedImplicitly] public DbSet<ContentBlock> Blocks { get; set; }
        [UsedImplicitly] public DbSet<StorageItem> StorageItems { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public DbSet<PropertiesRecord> Properties { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            
            modelBuilder.Entity<StorageItem>().Property(i => i.PublicUri)
                .HasConversion(u => u.ToString(), s => new Uri(s));
            if (ModelBuilderExtensions.IsArrayConversionRequired())
            {
                modelBuilder.RegisterSiteEntityConversions<Section>();
                modelBuilder.RegisterSiteEntityConversions<Menu>();
                modelBuilder.RegisterJsonStringConversion<Menu, List<MenuItem>>(s => s.Items);
                modelBuilder.RegisterJsonStringConversion<StorageItem, StorageItemPictureInfo>(s => s.PictureInfo);
            }

            modelBuilder.Entity<Section>().HasIndex(s => s.SiteIds);
            modelBuilder.Entity<Section>().HasIndex(s => s.IsPublished);
            modelBuilder.Entity<Section>().HasIndex(s => s.Type);
            modelBuilder.Entity<Section>().HasIndex(s => s.Url);
            
            var serviceProvider = this.GetService<IServiceProvider>();
            var configurators = serviceProvider.GetServices<IBioContextModelConfigurator>();
            var logger = serviceProvider.GetService<ILogger<BioContext>>();
            foreach (IBioContextModelConfigurator configurator in configurators)
            {
                configurator.Configure(modelBuilder, logger);
            }

            logger.LogInformation("Done registering");
        }
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

    public interface IBioContextModelConfigurator
    {
        void Configure(ModelBuilder modelBuilder, ILogger<BioContext> logger);
    }
}
