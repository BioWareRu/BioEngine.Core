using BioEngine.Core.Entities;

namespace BioEngine.Core.Tests.Fixtures
{
    public class TestSection : Section<TestSectionData>
    {
        public override string TypeTitle { get; set; } = "Раздел";
    }

    public class TestSectionData : TypedData
    {
    }
}