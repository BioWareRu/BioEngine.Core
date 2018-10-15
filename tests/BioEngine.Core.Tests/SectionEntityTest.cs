using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace BioEngine.Core.Tests
{
    public class SectionEntityTest : CoreTest
    {
        public SectionEntityTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task SaveWithoutSectionIdsFails()
        {
            var context = CreateDbContext();
            var repository = GetContentRepository(context);

            var content = new TestContent
            {
                Title = "Test Content 2",
                Url = "content2"
            };

            var result = await repository.Add(content);
            Assert.False(result.IsSuccess);
            Assert.True(result.Errors.Any(e => e.PropertyName == nameof(content.SectionIds)));
        }

        [Fact]
        public async Task SiteIdsAutoFillFromSections()
        {
            var context = CreateDbContext();
            var repository = GetContentRepository(context);
            var sectionRepository = GetSectionsRepository(context);
            var section = (await sectionRepository.GetAll(new QueryContext<TestSection, int>())).items.First();

            Assert.NotEmpty(section.SiteIds);

            var content = new TestContent
            {
                Title = "Test Content 2",
                Url = "content2",
                SectionIds = new[] {section.Id},
                AuthorId = 1
            };

            Assert.Empty(content.SiteIds);
            var result = await repository.Add(content);
            Assert.True(result.IsSuccess, $"Errors: {result.ErrorsString}");
            Assert.NotEmpty(content.SiteIds);
            Assert.Equal(section.SiteIds, content.SiteIds);
        }
    }
}