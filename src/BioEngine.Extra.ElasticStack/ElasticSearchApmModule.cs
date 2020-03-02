using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BioEngine.Core.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BioEngine.Extra.ElasticStack
{
    public class ElasticSearchApmModule : BaseBioEngineModule<ElasticSearchApmConfig>
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            base.ConfigureServices(services, configuration, environment);
            configuration["ElasticApm:ServiceName"] = Config.ApmServiceName ?? environment.ApplicationName;
            configuration["ElasticApm:ServiceVersion"] = Config.ApmServiceVersion;
            configuration["ElasticApm:TransactionSampleRate"] =
                Config.ApmTransactionSampleRate.ToString(CultureInfo.InvariantCulture);
            configuration["ElasticApm:TransactionMaxSpans"] = Config.ApmTransactionMaxSpans.ToString();
            configuration["ElasticApm:CentralConfig"] = Config.ApmCentralConfig.ToString();
            configuration["ElasticApm:SanitizeFieldNames"] = Config.ApmCentralConfig.ToString();
            if (Config.ApmSanitizeFieldNames != null && Config.ApmSanitizeFieldNames.Any())
            {
                configuration["ElasticApm:SanitizeFieldNames"] = string.Join(", ", Config.ApmSanitizeFieldNames);
            }

            if (Config.ApmGlobalLabels.Any())
            {
                configuration["ElasticApm:GlobalLabels"] =
                    string.Join(",", Config.ApmGlobalLabels.Select(kv => $"{kv.Key}={kv.Value}"));
            }

            configuration["ElasticApm:ServerUrls"] = string.Join(",", Config.ApmServerUrls);
            configuration["ElasticApm:SecretToken"] = Config.ApmSecretToken;
            configuration["ElasticApm:VerifyServerCert"] = Config.ApmVerifyServerCert.ToString();
            configuration["ElasticApm:FlushInterval"] = $"{Config.ApmFlushInterval.TotalSeconds}s";
            configuration["ElasticApm:MaxBatchEventCount"] = Config.ApmMaxBatchEventCount.ToString();
            configuration["ElasticApm:MaxQueueEventCount"] = Config.ApmMaxQueueEventCount.ToString();
            configuration["ElasticApm:MetricsInterval"] = $"{Config.ApmMetricsInterval.TotalSeconds}s";
            if (Config.ApmDisableMetrics != null && Config.ApmDisableMetrics.Any())
            {
                configuration["ElasticApm:DisableMetrics"] = string.Join(",", Config.ApmDisableMetrics);
            }

            configuration["ElasticApm:CaptureBody"] = Config.ApmCaptureBody;
            if (Config.ApmCaptureBodyContentTypes != null && Config.ApmCaptureBodyContentTypes.Any())
            {
                configuration["ElasticApm:CaptureBodyContentTypes"] =
                    string.Join(",", Config.ApmCaptureBodyContentTypes);
            }

            configuration["ElasticApm:CaptureHeaders"] = Config.ApmCaptureHeaders.ToString();
            configuration["ElasticApm:UseElasticTraceparentHeader"] =
                Config.ApmUseElasticTraceparentHeader.ToString();
            configuration["ElasticApm:StackTraceLimit"] = Config.ApmStackTraceLimit.ToString();
            configuration["ElasticApm:SpanFramesMinDuration"] =
                $"{Config.ApmSpanFramesMinDuration.TotalMilliseconds}ms";
            configuration["ElasticApm:ApmLogLevel"] = Config.ApmLogLevel;
        }
    }

    public class ElasticSearchApmConfig
    {
        public string? ApmServiceName { get; set; }
        public string ApmServiceVersion { get; set; } = "dev";
        public double ApmTransactionSampleRate { get; set; } = 1.0;
        public int ApmTransactionMaxSpans { get; set; } = 500;
        public bool ApmCentralConfig { get; set; } = true;
        public List<string>? ApmSanitizeFieldNames { get; set; }
        public readonly Dictionary<string, string> ApmGlobalLabels = new Dictionary<string, string>();

        public ElasticSearchApmConfig(List<Uri> apmServerUrls)
        {
            ApmServerUrls = apmServerUrls;
        }

        public List<Uri> ApmServerUrls { get; }
        public string? ApmSecretToken { get; set; }
        public bool ApmVerifyServerCert { get; set; } = true;
        public TimeSpan ApmFlushInterval { get; set; } = TimeSpan.FromSeconds(10);
        public int ApmMaxBatchEventCount { get; set; } = 10;
        public int ApmMaxQueueEventCount { get; set; } = 1000;
        public TimeSpan ApmMetricsInterval { get; set; } = TimeSpan.FromSeconds(30);
        public List<string>? ApmDisableMetrics { get; set; }
        public string ApmCaptureBody { get; set; } = "off";
        public List<string>? ApmCaptureBodyContentTypes { get; set; }
        public bool ApmCaptureHeaders { get; set; } = true;
        public bool ApmUseElasticTraceparentHeader { get; set; } = true;
        public int ApmStackTraceLimit { get; set; } = 50;
        public TimeSpan ApmSpanFramesMinDuration { get; set; } = TimeSpan.FromSeconds(0.5);
        public string ApmLogLevel { get; set; } = "Error";
    }
}
