using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.Entities;
using BioEngine.Core.Entities.Blocks;
using BioEngine.Core.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace BioEngine.Core.Tests
{
    [SuppressMessage("ReSharper", "VSTHRD200")]
    public class SectionEntityTest : CoreTest
    {
        public SectionEntityTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task SaveWithoutSectionIdsFails()
        {
            var scope = GetScope();
            var repository = scope.Get<TestContentRepository>();

            var content = new TestContent {Title = "Test Content 2", Url = "content2"};

            var result = await repository.AddAsync(content);
            Assert.False(result.IsSuccess);
            var error = result.Errors.FirstOrDefault(e => e.PropertyName == nameof(content.SectionIds));
            Assert.NotNull(error);
        }

        [Fact]
        public async Task SiteIdsAutoFillFromSections()
        {
            var scope = GetScope();
            var repository = scope.Get<TestContentRepository>();
            var sectionRepository = scope.Get<TestSectionRepository>();
            var section =
                (await sectionRepository.GetAllAsync()).items.First();

            Assert.NotEmpty(section.SiteIds);

            var content = new TestContent
            {
                Title = "Test Content 2",
                Url = "content2",
                SectionIds = new[] {section.Id},
                Blocks = new List<ContentBlock> {new TextBlock {Data = new TextBlockData {Text = "Bla"}}}
            };

            Assert.Empty(content.SiteIds);
            var result = await repository.AddAsync(content);
            Assert.True(result.IsSuccess, $"Errors: {result.ErrorsString}");
            Assert.NotNull(content.SiteIds);
            Assert.NotEmpty(content.SiteIds);
            Assert.Equal(section.SiteIds, content.SiteIds);
        }

        [Fact]
        public async Task JsonSearch()
        {
            var scope = GetScope();
            var dbContext = scope.GetDbContext();
            var count = await dbContext.Sections.CountAsync();
            var section = new TestSection
            {
                Id = Guid.NewGuid(),
                Title = "Test section 4",
                Url = "test4",
                DatePublished = DateTimeOffset.Now,
                IsPublished = true,
                SiteIds = new[] {Guid.NewGuid()},
                Data = new TestSectionData {SomeNumber = 5}
            };
            await dbContext.AddAsync(section);
            await dbContext.SaveChangesAsync();

            var newCount = await dbContext.Sections.CountAsync();

            Assert.Equal(count + 1, newCount);

            var search = await dbContext.Set<TestSection>().Where(s => s.Data.SomeNumber == 5).FirstOrDefaultAsync();
            Assert.NotNull(search);
            Assert.Equal(section.Id, search.Id);
        }
    }
}
