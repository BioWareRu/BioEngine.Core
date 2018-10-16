using System;
using System.Runtime.CompilerServices;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Repository;
using BioEngine.Core.Settings;
using BioEngine.Core.Tests.Fixtures;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;
using ContentRepository = BioEngine.Core.Tests.Fixtures.ContentRepository;

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

            var content = new TestContent
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
            services.RegisterEntityType(typeof(TestContent));
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

        private SettingsProvider GetSettingsProvider(BioContext dbContext)
        {
            var provider = new SettingsProvider(dbContext);
            //init settings
            return provider;
        }

        protected SitesRepository GetSitesRepository(BioContext context)
        {
            var settingsProvider = GetSettingsProvider(context);
            var repositoryContext = new BioRepositoryContext<Site, int>(context, settingsProvider);
            var repository = new SitesRepository(repositoryContext);
            return repository;
        }

        protected SectionRepository GetSectionsRepository(BioContext context)
        {
            var settingsProvider = GetSettingsProvider(context);
            var repositoryContext = new BioRepositoryContext<TestSection, int>(context, settingsProvider);
            var repository = new SectionRepository(repositoryContext);
            return repository;
        }

        protected ContentRepository GetContentRepository(BioContext context)
        {
            var settingsProvider = GetSettingsProvider(context);
            var repositoryContext = new BioRepositoryContext<TestContent, int>(context, settingsProvider);
            var repository = new ContentRepository(repositoryContext,
                new SectionsRepository(new BioRepositoryContext<Section, int>(context, settingsProvider)));
            return repository;
        }
    }
}