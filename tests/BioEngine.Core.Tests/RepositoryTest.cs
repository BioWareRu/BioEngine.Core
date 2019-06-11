using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Entities;
using BioEngine.Core.Repository;
using BioEngine.Core.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace BioEngine.Core.Tests
{
    [SuppressMessage("ReSharper", "VSTHRD200")]
    public class RepositoryTest : CoreTest
    {
        public RepositoryTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task Create()
        {
            var scope = GetScope();
            var repository = scope.Get<SitesRepository>();
            var count = await repository.CountAsync();
            Assert.Equal(1, count);
            var site = new Site {Title = "New site", Url = "https://site.ru"};
            var result = await repository.AddAsync(site);
            Assert.True(result.IsSuccess);
            Assert.True(result.Entity.Id != Guid.Empty);
            count = await repository.CountAsync();
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task GetById()
        {
            var scope = GetScope();
            var repository = scope.Get<SitesRepository>();
            var site = await repository.GetByIdAsync(CoreTestScope.SiteId);
            Assert.NotNull(site);
            Assert.Equal(CoreTestScope.SiteId, site.Id);
        }

        [Fact]
        public async Task Update()
        {
            var scope = GetScope();
            var context = scope.GetDbContext();
            var repository = scope.Get<SitesRepository>();
            var site = await repository.GetByIdAsync(CoreTestScope.SiteId);
            const string newTitle = "Test new";
            var oldDate = site.DateUpdated;
            site.Title = newTitle;
            await repository.UpdateAsync(site);
            Assert.True(site.DateUpdated > oldDate);
            var siteFromDb = await context.Sites.FirstAsync(s => s.Id == site.Id);
            Assert.Equal(newTitle, siteFromDb.Title);
        }

        [Fact]
        public async Task Delete()
        {
            var scope = GetScope();
            var context = scope.GetDbContext();
            var repository = scope.Get<SitesRepository>();
            var count = await context.Sites.CountAsync();
            var site = await context.Sites.FirstAsync();
            await repository.DeleteAsync(site.Id);
            var newCount = await context.Sites.CountAsync();
            Assert.Equal(count - 1, newCount);
        }

        [Fact]
        public async Task SearchNoConditions()
        {
            var scope = GetScope();
            var context = scope.GetDbContext();
            var repository = scope.Get<SitesRepository>();
            var result = await repository.GetAllAsync();
            var count = await context.Sites.CountAsync();
            Assert.NotEmpty(result.items);
            Assert.Equal(count, result.itemsCount);
        }

        [Fact]
        public async Task SearchPublished()
        {
            var scope = GetScope();
            var context = scope.GetDbContext();
            var repository = scope.Get<TestSectionRepository>();
            var section = await context.Set<TestSection>().FirstAsync();
            await repository.UnPublishAsync(section);
            var count = await context.Set<TestSection>().CountAsync(s => s.IsPublished);

            var result = await repository.GetAllAsync(sections => sections.Where(s => s.IsPublished));
            Assert.Equal(count, result.itemsCount);

            result = await repository.GetAllAsync();
            Assert.Equal(count + 1, result.itemsCount);
        }

        [Fact]
        public async Task Changes()
        {
            var scope = GetScope();
            var context = scope.GetDbContext();
            var repository = scope.Get<SitesRepository>();
            var site = await context.Sites.FirstAsync();
            var oldSite = await context.Sites.AsNoTracking().FirstAsync();

            var originalTitle = site.Title;
            var newTitle = "Another title";
            site.Title = newTitle;

            var changes = repository.GetChanges(site, oldSite);

            Assert.NotEmpty(changes);

            var change = changes.FirstOrDefault(x => x.Name == nameof(site.Title));
            Assert.NotEqual(default(PropertyChange), change);
            Assert.Equal(originalTitle, change.OriginalValue);
            Assert.Equal(newTitle, change.CurrentValue);
        }
    }
}
