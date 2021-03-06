using System;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Modules;
using BioEngine.Core.Tests.Fixtures;
using BioEngine.Core.Tests.Xunit;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace BioEngine.Core.Tests
{
    [UsedImplicitly]
    public class CoreTestScope : BaseTestScope<CoreTestScope>
    {
        public static Guid SiteId = Guid.NewGuid();

        protected override void InitDbContext(BioContext dbContext)
        {
            var site = new Site {Id = SiteId, Title = "Test site", Url = "https://test.ru"};
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
                Url = "test2",
                DatePublished = DateTimeOffset.Now,
                IsPublished = true,
                SiteIds = new[] {site.Id},
                SectionIds = new[] {section.Id}
            };
            dbContext.Add(content);

            var page = new TestContent
            {
                Title = "Test page",
                Url = "test3",
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
            return bioEngine.AddModule<TestsModule>();
        }
    }

    public abstract class CoreTest : BaseTest<CoreTestScope>
    {
        protected CoreTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }

    public class TestsModule : BaseBioEngineModule
    {
    }

    public class TestBioContextModelConfigurator : IBioContextModelConfigurator
    {
        public void Configure(ModelBuilder modelBuilder, ILogger<BioContext> logger)
        {
            modelBuilder.RegisterContentItem<TestContent>(logger);
            modelBuilder.RegisterSection<TestSection, TestSectionData>(logger);
        }
    }
}
