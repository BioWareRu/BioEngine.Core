using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using BioEngine.Core.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace BioEngine.Core.Tests
{
    [SuppressMessage("ReSharper", "VSTHRD200")]
    public class TypedEntityTest : CoreTest
    {
        public TypedEntityTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task DiscriminatorFill()
        {
            var scope = GetScope();
            var repository = scope.Get<TestSectionRepository>();
            var section = new TestSection
            {
                Title = "Test type", Url = "testurl", SiteIds = new[] {CoreTestScope.SiteId}
            };

            Assert.True(string.IsNullOrEmpty(section.Type));

            var result = await repository.AddAsync(section);
            Assert.True(result.IsSuccess, $"Errors: {result.ErrorsString}");
            Assert.Equal("testsection", section.Type);
        }
    }
}
