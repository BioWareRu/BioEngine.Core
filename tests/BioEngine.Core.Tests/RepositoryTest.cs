using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Repository;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace BioEngine.Core.Tests
{
    public class RepositoryTest : CoreTest
    {
        public RepositoryTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task Create()
        {
            var context = CreateDbContext(init: false);
            var repository = GetSitesRepository(context);
            var count = await repository.Count();
            Assert.Equal(0, count);
            var site = new Site
            {
                Title = "New site",
                Url = "https://site.ru"
            };
            var result = await repository.Add(site);
            Assert.True(result.IsSuccess);
            Assert.True(result.Entity.Id > 0);
            count = await repository.Count();
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task GetById()
        {
            var context = CreateDbContext();
            var repository = GetSitesRepository(context);
            var site = await repository.GetById(1);
            Assert.NotNull(site);
            Assert.Equal(1, site.Id);
        }

        [Fact]
        public async Task Update()
        {
            var context = CreateDbContext();
            var repository = GetSitesRepository(context);
            var site = await repository.GetById(1);
            const string newTitle = "Test new";
            var oldDate = site.DateUpdated;
            site.Title = newTitle;
            await repository.Update(site);
            Assert.True(site.DateUpdated > oldDate);
            var siteFromDb = await context.Sites.FirstAsync(s => s.Id == site.Id);
            Assert.Equal(newTitle, siteFromDb.Title);
        }

        [Fact]
        public async Task Delete()
        {
            var context = CreateDbContext();
            var repository = GetSitesRepository(context);
            var count = await context.Sites.CountAsync();
            var site = await context.Sites.FirstAsync();
            await repository.Delete(site.Id);
            var newCount = await context.Sites.CountAsync();
            Assert.Equal(count - 1, newCount);
        }

        [Fact]
        public async Task SearchNoConditions()
        {
            var context = CreateDbContext();
            var repository = GetSitesRepository(context);
            var queryContext = new QueryContext<Site, int>();
            var result = await repository.GetAll(queryContext);
            var count = await context.Sites.CountAsync();
            Assert.NotEmpty(result.items);
            Assert.Equal(count, result.itemsCount);
        }

        [Fact]
        public async Task SearchPublished()
        {
            var context = CreateDbContext();
            var repository = GetSitesRepository(context);
            var site = await context.Sites.FirstAsync();
            await repository.UnPublish(site);
            var count = await context.Sites.CountAsync(s => s.IsPublished);

            var queryContext = new QueryContext<Site, int>();
            var result = await repository.GetAll(queryContext);
            Assert.Equal(count, result.itemsCount);

            queryContext.IncludeUnpublished = true;
            result = await repository.GetAll(queryContext);
            Assert.Equal(count + 1, result.itemsCount);
        }

        [Fact]
        public async Task Changes()
        {
            var context = CreateDbContext();

            var repository = GetSitesRepository(context);
            var site = await context.Sites.FirstAsync();

            var originalTitle = site.Title;
            var newTitle = "Another title";
            site.Title = newTitle;

            var changes = repository.GetChanges(site);

            Assert.NotEmpty(changes);

            var change = changes.FirstOrDefault(x => x.Name == nameof(site.Title));
            Assert.NotEqual(default(PropertyChange), change);
            Assert.Equal(originalTitle, change.OriginalValue);
            Assert.Equal(newTitle, change.CurrentValue);
        }
    }
}