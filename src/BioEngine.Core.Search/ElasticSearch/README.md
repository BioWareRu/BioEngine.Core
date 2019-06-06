# ElasticSearchModule

Реализация поискового модуля для `ElasticSearch`

# Установка

Необходим `ElasticSearch` версии 6.0 и выше с установленными Hunspell-словарями для русского и английского языков.

# Использование

```csharp
bioEngine.AddModule<ElasticSearchModule, ElasticSearchModuleConfig>((configuration, env) =>
    new ElasticSearchModuleConfig("http://localhost:9200", "admin", "admin"));
```
