using BioEngine.Core.Entities;

namespace BioEngine.Core.Repository
{
    public class StorageItemsRepository : BioRepository<StorageItem>
    {
        public StorageItemsRepository(BioRepositoryContext<StorageItem> repositoryContext) : base(
            repositoryContext)
        {
        }
    }
}
