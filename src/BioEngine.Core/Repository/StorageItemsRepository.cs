using BioEngine.Core.Entities;

namespace BioEngine.Core.Repository
{
    public class StorageItemsRepository : BioRepository<StorageItem, int>
    {
        public StorageItemsRepository(BioRepositoryContext<StorageItem, int> repositoryContext) : base(
            repositoryContext)
        {
        }
    }
}