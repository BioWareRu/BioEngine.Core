# Модуль SEO

Модуль реализует базовый набор meta-свойств для сущностей. В частности ключевые слова и описание.

## Установка

```csharp
bioengine.AddModule<SeoModule>() 
```

## Использование

Получаем набор свойств для конкретной сущности
```csharp
var seoPropertiesSet = await PropertiesProvider.GetAsync<SeoPropertiesSet>(Section);
```
