using BioEngine.Core.Entities;

namespace BioEngine.Core.Repository
{
    public abstract class SectionRepository<TEntity> : ContentEntityRepository<TEntity> where TEntity : Section
    {
        protected SectionRepository(BioRepositoryContext<TEntity> repositoryContext) : base(repositoryContext)
        {
        }
    }
}
