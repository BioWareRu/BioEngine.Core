using System;
using System.Collections.Generic;
using BioEngine.Core.Logging;
using Elastic.Apm.SerilogEnricher;
using Elastic.CommonSchema.Serilog;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace BioEngine.Extra.ElasticStack
{
    public class ElasticSearchLoggingModule : LoggingModule<ElasticSearchLoggingConfig>
    {
        protected override LoggerConfiguration ConfigureProd(LoggerConfiguration loggerConfiguration, string appName)
        {
            return loggerConfiguration.Enrich.WithElasticApmCorrelationInfo()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(Config.ElasticSearchUrls)
                {
                    CustomFormatter = new EcsTextFormatter(),
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                    IndexFormat = Config.LoggingIndexFormat
                });
        }
    }

    public class ElasticSearchLoggingConfig : LoggingModuleConfig
    {
        public ElasticSearchLoggingConfig(List<Uri> elasticSearchUrls)
        {
            ElasticSearchUrls = elasticSearchUrls;
        }

        public List<Uri> ElasticSearchUrls { get; }
        public string LoggingIndexFormat { get; set; } = "apm-logs-{0:yyyy.MM.dd}";
    }
}
