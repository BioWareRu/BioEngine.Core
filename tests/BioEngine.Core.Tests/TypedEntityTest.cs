using System.Linq;
using System.Threading.Tasks;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;
using BioEngine.Core.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace BioEngine.Core.Tests
{
    public class TypedEntityTest : CoreTest
    {
        public TypedEntityTest(CoreTestFixture testFixture, ITestOutputHelper testOutputHelper) : base(testFixture,
            testOutputHelper)
        {
        }

        [Fact]
        public async Task DiscriminatorFill()
        {
            using (var context = CreateDbContext())
            {
                var repository = GetSectionsRepository(context);
                var section = new TestSection
                {
                    Title = "Test type",
                    Url = "testurl",
                    SiteIds = new[] {1}
                };
                var discriminator = BioContext.TypesProvider.GetSectionTypes().Where(t => t.type == section.GetType())
                    .Select(t => t.discriminator).First();
                Assert.True(discriminator > 0);

                Assert.True(section.Type == 0);

                var result = await repository.Add(section);
                Assert.True(result.IsSuccess, $"Errors: {result.ErrorsString}");
                Assert.Equal(discriminator, section.Type);
            }
        }
    }
}