using System;
using System.Runtime.CompilerServices;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Providers;
using BioEngine.Core.Tests.Fixtures;
using Xunit.Abstractions;

namespace BioEngine.Core.Tests
{
    public class CoreTestFixture : BaseTestFixture
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
    }

    public class CoreTest : BaseTest<CoreTestFixture>
    {
        public CoreTest(CoreTestFixture testFixture, ITestOutputHelper testOutputHelper) : base(testFixture,
            testOutputHelper)
        {
        }

        protected BioContext CreateDbContext([CallerMemberName] string databaseName = "", bool inMemory = true,
            bool init = true)
        {
            var typesProvider = new TypesProvider();
            typesProvider.AddContentType(typeof(TestContent));
            typesProvider.AddSectionType(typeof(TestSection));
            BioContext.TypesProvider = typesProvider;
            return GetDbContext(databaseName, inMemory, init);
        }

        protected SettingsProvider GetSettingsProvider(BioContext dbContext)
        {
            var provider = new SettingsProvider(dbContext);
            //init settings
            return provider;
        }
    }
}