using System.ComponentModel.DataAnnotations.Schema;
using BioEngine.Core.DB;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Tests.Fixtures
{
    [TypedEntity("testsection")]
    public class TestSection : Section<TestSectionData>
    {
        public override string TypeTitle { get; set; } = "Раздел";
        [NotMapped] public override string PublicUrl => $"/test/{Url}/about.html";
        [NotMapped] public override string PostsUrl => $"/test/{Url}/posts.html";
    }

    public class TestSectionData : ITypedData
    {
    }
}
