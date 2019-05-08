using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;

namespace BioEngine.Core.Search.ElasticSearch
{
    [UsedImplicitly]
    public class ElasticSearcher : ISearcher
    {
        private readonly ILogger<ElasticSearcher> _logger;
        private readonly ElasticSearcherOptions _options;
        private ElasticClient _client;

        public ElasticSearcher(IOptions<ElasticSearcherOptions> options, ILogger<ElasticSearcher> logger)
        {
            _logger = logger;
            _options = options.Value;
        }

        private ElasticClient GetClient()
        {
            if (_client == null)
            {
                _logger.LogDebug("Create elastic client");
                var settings = new ConnectionSettings(new Uri(_options.Url)).DisableDirectStreaming()
                    .OnRequestCompleted(details =>
                    {
                        _logger.LogDebug("### ES REQEUST ###");
                        if (details.RequestBodyInBytes != null)
                            _logger.LogDebug(Encoding.UTF8.GetString(details.RequestBodyInBytes));
                        _logger.LogDebug("### ES RESPONSE ###");
                        if (details.ResponseBodyInBytes != null)
                            _logger.LogDebug(Encoding.UTF8.GetString(details.ResponseBodyInBytes));
                    })
                    .PrettyJson();
                if (!string.IsNullOrEmpty(_options.Login))
                {
                    settings.BasicAuthentication(_options.Login, _options.Password);
                }

                settings.ServerCertificateValidationCallback((o, certificate, arg3, arg4) =>
                {
                    return true;
                });
                _client = new ElasticClient(settings);
            }

            return _client;
        }

        private SearchDescriptor<SearchModel> GetSearchRequest(SearchDescriptor<SearchModel> descriptor,
            string indexName, string term,
            int limit = 0)
        {
            var names = GetSearchText(term);

            return descriptor.Query(q => q.QueryString(qs => qs.Query(names)))
                .Sort(s => s.Descending("_score").Descending("date")).Size(limit > 0 ? limit : 20)
                .Index(indexName.ToLowerInvariant());
        }

        private static string GetSearchText(string term)
        {
            var names = "";
            if (term != null)
            {
                names = term.Replace("+", " OR *");
            }

            names = names + "*";
            return names;
        }

        public async Task<bool> AddOrUpdateAsync(string indexName, IEnumerable<SearchModel> searchModels)
        {
            var result = await GetClient().IndexManyAsync(searchModels, indexName.ToLowerInvariant());
            return !result.Errors;
        }

        public async Task<bool> DeleteAsync(string indexName, IEnumerable<SearchModel> searchModels)
        {
            var result = await GetClient().DeleteManyAsync(searchModels, indexName.ToLowerInvariant());
            return !result.Errors;
        }

        public async Task<bool> DeleteAsync(string indexName)
        {
            var result = await GetClient()
                .DeleteIndexAsync(Indices.All, descriptor => descriptor.Index(indexName.ToLowerInvariant()));
            return result.Acknowledged;
        }

        public async Task<long> CountAsync(string indexName, string term)
        {
            var names = GetSearchText(term);
            var resultsCount = await GetClient().CountAsync<SearchModel>(x =>
                x.Query(q => q.QueryString(qs => qs.Query(names))).Index(indexName.ToLowerInvariant()));
            return resultsCount.Count;
        }

        public async Task<IEnumerable<SearchModel>> SearchAsync(string indexName, string term, int limit)
        {
            var results = await GetClient().SearchAsync<SearchModel>(x => GetSearchRequest(x, indexName, term, limit));

            return results.Documents;
        }

        private AnalysisDescriptor BuildIndexDescriptor(AnalysisDescriptor a)
        {
            return
                a
                    .Analyzers(aa => aa
                        .Custom("default",
                            descriptor =>
                                descriptor.Tokenizer("standard")
                                    .CharFilters("html_strip")
                                    .Filters("lowercase", "ru_RU", "en_US"))
                    ).TokenFilters(descriptor =>
                        descriptor.Hunspell("ru_RU", hh => hh.Dedup().Locale("ru_RU"))
                            .Hunspell("en_US", hh => hh.Dedup().Locale("en_US")))
                ;
        }

        public async Task InitAsync(string indexName)
        {
            var indexExists = await GetClient().IndexExistsAsync(indexName);
            if (indexExists.Exists)
            {
                await GetClient().CloseIndexAsync(indexName);
                var result = await GetClient().UpdateIndexSettingsAsync(indexName, c => c.IndexSettings(s =>
                    s.Analysis(BuildIndexDescriptor)));
                if (!result.IsValid)
                {
                    throw result.OriginalException;
                }

                await GetClient().OpenIndexAsync(indexName);
            }
            else
            {
                var result = await GetClient()
                    .CreateIndexAsync(indexName, c => c.Settings(s => s.Analysis(BuildIndexDescriptor)));
                if (!result.IsValid)
                {
                    throw result.OriginalException;
                }
            }
        }
    }
}
