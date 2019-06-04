namespace BioEngine.Core.DB.Queries
{
    internal struct SortQuery
    {
        public readonly string Name;
        public readonly SortDirection SortDirection;

        public SortQuery(string name, SortDirection sortDirection)
        {
            Name = name;
            SortDirection = sortDirection;
        }
    }
}
