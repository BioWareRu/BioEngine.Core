using System;
using System.Runtime.CompilerServices;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Entities.Blocks;
using BioEngine.Core.Properties;
using BioEngine.Core.Repository;
using BioEngine.Core.Tests.Fixtures;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace BioEngine.Core.Tests
{
    [UsedImplicitly]
    public class CoreTestScope : BaseTestScope
    {
        protected override void InitDbContext(BioContext dbContext)
        {
            var site = new Site
            {
                Title = "Test site",
                Url = "https://test.ru",
                DatePublished = DateTimeOffset.Now,
                IsPublished = true
            };
            dbContext.Add(site);
            dbContext.SaveChanges();

            var section = new TestSection
            {
                Title = "Test section",
                Url = "test",
                DatePublished = DateTimeOffset.Now,
                IsPublished = true,
                SiteIds = new[] {site.Id}
            };
            dbContext.Add(section);
            dbContext.SaveChanges();

            var content = new Post
            {
                Title = "Test content",
                Url = "test",
                DatePublished = DateTimeOffset.Now,
                IsPublished = true,
                SiteIds = new[] {site.Id},
                SectionIds = new[] {section.Id}
            };
            dbContext.Add(content);

            var page = new Page
            {
                Title = "Test page",
                Url = "test",
                Text = "Bla-bla",
                DatePublished = DateTimeOffset.Now,
                IsPublished = true
            };
            dbContext.Add(page);
            dbContext.SaveChanges();
        }

        protected override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.RegisterEntityType(typeof(TestSection));
            services.RegisterEntityType(typeof(TextBlock));
            return base.ConfigureServices(services);
        }
    }

    public abstract class CoreTest : BaseTest<CoreTestScope>
    {
        protected CoreTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        protected BioContext CreateDbContext([CallerMemberName] string databaseName = "", bool init = true)
        {
            return GetDbContext(databaseName, init);
        }

        private PropertiesProvider GetPropertiesProvider(BioContext dbContext)
        {
            var provider = new PropertiesProvider(dbContext);
            //init properties
            return provider;
        }

        protected SitesRepository GetSitesRepository(BioContext context)
        {
            var propertiesProvider = GetPropertiesProvider(context);
            var repositoryContext = new BioRepositoryContext<Site, int>(context, propertiesProvider);
            var repository = new SitesRepository(repositoryContext);
            return repository;
        }

        protected SectionRepository GetSectionsRepository(BioContext context)
        {
            var propertiesProvider = GetPropertiesProvider(context);
            var repositoryContext = new BioRepositoryContext<TestSection, int>(context, propertiesProvider);
            var repository = new SectionRepository(repositoryContext);
            return repository;
        }

        protected ContentRepository GetContentRepository(BioContext context)
        {
            var propertiesProvider = GetPropertiesProvider(context);
            var repositoryContext = new BioRepositoryContext<Post, int>(context, propertiesProvider);
            var repository = new ContentRepository(repositoryContext,
                new SectionsRepository(new BioRepositoryContext<Section, int>(context, propertiesProvider)));
            return repository;
        }
    }
}