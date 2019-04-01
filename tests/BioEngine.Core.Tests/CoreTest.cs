using System;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
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

        protected override IServiceCollection ConfigureServices(IServiceCollection services, string name)
        {
            RegisterCoreModule(services, name, new[] {typeof(CoreTest).Assembly});

            //services.RegisterEntityType(typeof(TestSection));
            //services.RegisterEntityType(typeof(TextBlock));
            return base.ConfigureServices(services, name);
        }
    }

    public abstract class CoreTest : BaseTest<CoreTestScope>
    {
        protected CoreTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

//        protected BioContext CreateDbContext([CallerMemberName] string databaseName = "", bool init = true)
//        {
//            return GetDbContext(databaseName, init);
//        }

//        private PropertiesProvider GetPropertiesProvider(BioContext dbContext)
//        {
//            var provider = new PropertiesProvider(dbContext);
//            //init properties
//            return provider;
//        }
//
//        protected SitesRepository GetSitesRepository(BioContext context)
//        {
//            var propertiesProvider = GetPropertiesProvider(context);
//            var repositoryContext = new BioRepositoryContext<Site>(context, propertiesProvider);
//            var repository = new SitesRepository(repositoryContext);
//            return repository;
//        }
//
//        protected SectionRepository GetSectionsRepository(BioContext context)
//        {
//            var propertiesProvider = GetPropertiesProvider(context);
//            var repositoryContext = new BioRepositoryContext<TestSection>(context, propertiesProvider);
//            var repository = new SectionRepository(repositoryContext);
//            return repository;
//        }
//
//        protected PostsRepository GetContentRepository(BioContext context)
//        {
//            var propertiesProvider = GetPropertiesProvider(context);
//            var repositoryContext = new BioRepositoryContext<Post>(context, propertiesProvider);
//            var repository = new PostsRepository(repositoryContext,
//                new SectionsRepository(new BioRepositoryContext<Section>(context, propertiesProvider)));
//            return repository;
//        }
    }
}
