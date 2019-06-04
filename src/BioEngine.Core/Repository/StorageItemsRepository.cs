using BioEngine.Core.DB;
using BioEngine.Core.Entities;

namespace BioEngine.Core.Repository
{
    public class StorageItemsRepository : BioRepository<StorageItem, QueryContext<StorageItem>>
    {
        public StorageItemsRepository(BioRepositoryContext<StorageItem> repositoryContext) : base(
            repositoryContext)
        {
        }
    }
}
