using System.Reflection;
using System.Threading.Tasks;
using BioEngine.Core.Entities;
using BioEngine.Core.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace BioEngine.Core.Tests
{
    public class TypedEntityTest : CoreTest
    {
        public TypedEntityTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task DiscriminatorFill()
        {
            var context = CreateDbContext();
            var repository = GetSectionsRepository(context);
            var section = new TestSection
            {
                Title = "Test type",
                Url = "testurl",
                SiteIds = new[] {1}
            };
            var attr = section.GetType().GetCustomAttribute<TypedEntityAttribute>();

            Assert.True(attr.Type > 0);

            Assert.True(section.Type == 0);

            var result = await repository.Add(section);
            Assert.True(result.IsSuccess, $"Errors: {result.ErrorsString}");
            Assert.Equal(attr.Type, section.Type);
        }
    }
}