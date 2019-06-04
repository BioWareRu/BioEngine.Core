using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB.Queries;
using BioEngine.Core.Entities;
using BioEngine.Core.Entities.Blocks;
using BioEngine.Core.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace BioEngine.Core.Tests
{
    [SuppressMessage("AsyncUsage.CSharp.Naming", "UseAsyncSuffix", Justification = "Reviewed.")]
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
                (await sectionRepository.GetAllAsync(new QueryContext<TestSection>())).items.First();

            Assert.NotEmpty(section.SiteIds);

            var content = new TestContent
            {
                Title = "Test Content 2",
                Url = "content2",
                SectionIds = new[] {section.Id},
                AuthorId = 1,
                Blocks = new List<ContentBlock> {new TextBlock {Data = new TextBlockData {Text = "Bla"}}}
            };

            Assert.Empty(content.SiteIds);
            var result = await repository.AddAsync(content);
            Assert.True(result.IsSuccess, $"Errors: {result.ErrorsString}");
            Assert.NotNull(content.SiteIds);
            Assert.NotEmpty(content.SiteIds);
            Assert.Equal(section.SiteIds, content.SiteIds);
        }
    }
}
