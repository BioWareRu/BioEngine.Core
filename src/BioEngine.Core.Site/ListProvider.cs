using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BioEngine.Core.Abstractions;
using BioEngine.Core.Repository;

namespace BioEngine.Core.Site
{
    public class ListProvider<TEntity, TRepository>
        where TEntity : class, IEntity, IRoutable, IContentEntity
        where TRepository : IBioRepository<TEntity>
    {
        private readonly TRepository _repository;
        private int _page;
        private int _itemsPerPage = 20;
        private string _orderByString;
        private Expression<Func<TEntity, object>> _orderBy;
        private bool _orderByDescending = false;
        private Core.Entities.Site _site;

        public ListProvider(TRepository repository)
        {
            _repository = repository;
        }

        public ListProvider<TEntity, TRepository> SetPage(int page)
        {
            _page = page;
            return this;
        }

        public ListProvider<TEntity, TRepository> SetPageSize(int itemsPerPage)
        {
            _itemsPerPage = itemsPerPage;
            return this;
        }

        public ListProvider<TEntity, TRepository> SetOrderByString(string orderByString)
        {
            _orderByString = orderByString;
            return this;
        }

        public ListProvider<TEntity, TRepository> SetOrderBy(Expression<Func<TEntity, object>> orderBy)
        {
            _orderBy = orderBy;
            return this;
        }
        
        public ListProvider<TEntity, TRepository> SetOrderByDescending (Expression<Func<TEntity, object>> orderBy)
        {
            _orderBy = orderBy;
            _orderByDescending = true;
            return this;
        }

        public ListProvider<TEntity, TRepository> SetSite(Core.Entities.Site site)
        {
            _site = site;
            return this;
        }


        public Task<(TEntity[] items, int itemsCount)> GetAllAsync(
            Func<BioRepositoryQuery<TEntity>, BioRepositoryQuery<TEntity>>? configureQuery)
        {
            return _repository.GetAllAsync(entities => ConfigureQuery(configureQuery(entities)));
        }

        private BioRepositoryQuery<TEntity> ConfigureQuery(BioRepositoryQuery<TEntity> query)
        {
            if (!string.IsNullOrEmpty(_orderByString)) 
            {
                query.OrderByString(_orderByString);
            }
            else
            {
                if (_orderBy != null)
                {
                    if (_orderByDescending)
                    {
                        query.OrderByDescending(_orderBy);
                    }
                    else
                    {
                        query.OrderBy(_orderBy);
                    }
                }
            }

            var offset = 0;
            if (_page > 0)
            {
                offset = (_page - 1) * _itemsPerPage;
            }

            if (_site != null)
            {
                query.ForSite(_site);
            }

            return query.Skip(offset).Take(_itemsPerPage);
        }
    }
}
