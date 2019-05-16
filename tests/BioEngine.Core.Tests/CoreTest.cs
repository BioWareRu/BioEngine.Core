using System;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Modules;
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
        public static Guid SiteId = Guid.NewGuid();

        protected override void InitDbContext(BioContext dbContext)
        {
            var site = new Site
            {
                Id = SiteId,
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
                DatePublished = DateTimeOffset.Now,
                IsPublished = true,
                SiteIds = new[] {site.Id}
            };
            dbContext.Add(page);
            dbContext.SaveChanges();
        }

        protected override BioEngine ConfigureBioEngine(BioEngine bioEngine)
        {
            base.ConfigureBioEngine(bioEngine);
            return bioEngine.AddModule<TestsModule>().ConfigureServices(collection =>
            {
                collection.AddSingleton<IMainSiteSelectionPolicy, TestMainSiteSelectionPolicy>();
            });
        }
    }

    public abstract class CoreTest : BaseTest<CoreTestScope>
    {
        protected CoreTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }

    public class TestsModule : BioEngineModule
    {
    }
}
