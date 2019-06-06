# Модуль поиска для BioEngine

Модуль реализует функции поиска по контенту сайта на BioEngine. Функции модуля разделены между двумя интерфейсами - `ISearcher` и `ISearchProvider` и классом `SearchModel`.

## SearchModel

Общий класс, к которому приводятся все сущности перед добавление в поисковой индекс. Используется для унификации структуру поискового индекса. 

## ISearcher

`ISearcher` - интерфейс для работы с конкретной поисковой системой. Содержит методы для добавления объектов типа `SearchModel` в индекс, поиска по индексу и так далее.
В модуле поставляется реализация для работы с ElasticSearch.

### Добавление и обновление сущностей в индекс

Добавляет сущности searchModels в индекс с именем `indexName` 

```csharp
Task<bool> AddOrUpdateAsync(string indexName, IEnumerable<SearchModel> searchModels);
```

### Удаление сущностей из индекса

Удаляет сущности searchModels из индекса с именем `indexName`

```csharp
Task<bool> DeleteAsync(string indexName, IEnumerable<SearchModel> searchModels);
```

### Удаление индекса

Удаляет индекс с именем `indexName`

```csharp
Task<bool> DeleteAsync(string indexName);
```

### Подсчёт сущностей в индексе по условию

Посчитывает количество сущностей в индексе с именем `indexName`, которые удовлетворяют условию `term`

```csharp
Task<long> CountAsync(string indexName, string term, Site site);
```

### Поиск сущностей в индексе по условию

Делает выборку сущностей из индексе с именем `indexName`, которые удовлетворяют условию `term`

```csharp
Task<SearchModel[]> SearchAsync(string indexName, string term, int limit, Site site);
```

### Инициализация индекса

Инициализирует индекс с именем `indexName`

```csharp
Task InitAsync(string indexName);
``` 

## ISearchProvider

В интерфейсе содержатся методы для индексации и поиска конкретных сущностей, конвертации их в `SearchModel` и обратно. 
Для каждого типа сущности должен быть реализован свой `SearchProvider`. 
Для уменьшения требуемого кода можно наследоваться от класса `BaseSearchProvider`.

### Проверка на совместимость провайдера с сущностью
```csharp
bool CanProcess(Type type);
```
### Инициализация провайдера
```csharp
Task InitAsync();
```
### Удаление индекса
```csharp
Task DeleteIndexAsync();
```
### Подсчёт сущностей в индексе по условию
```csharp
Task<long> CountAsync(string term, Site site);
```
### Поиск сущностей в индексе по условию
```csharp
Task<T[]> SearchAsync(string term, int limit, Site site);
```
### Добавление сущности в индекс
```csharp
Task AddOrUpdateEntityAsync(T entity);
```
### Добавление сущностей в индекс
```csharp
Task<bool> AddOrUpdateEntitiesAsync(T[] entities);
```
### Удаление сущности из индекса
```csharp
Task<bool> DeleteEntityAsync(T entity);
```
### Удаление сущностей из индекса
```csharp
Task<bool> DeleteEntitiesAsync(T[] entities);
```

