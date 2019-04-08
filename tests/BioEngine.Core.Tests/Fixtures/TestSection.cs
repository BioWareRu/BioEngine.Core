using BioEngine.Core.DB;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Tests.Fixtures
{
    [TypedEntity("testsection")]
    public class TestSection : Section<TestSectionData>
    {
        public override string TypeTitle { get; set; } = "Раздел";
    }

    public class TestSectionData : TypedData
    {
    }
}
