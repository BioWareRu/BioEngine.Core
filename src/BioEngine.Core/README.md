# BioEngine.Core

Ядро движка, которое содержит основные функции и сущности, поверх которых строится приложение.

## BioEngine

Класс `BioEngine` является точкой входа в приложение. Через него происходит инициализация, подключение модулей и запуск.

```csharp

public static class Program
{
    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateWebHostBuilder(string[] args) =>
        new BioEngine(args)
            .AddModule<SeoModule>
            .GetHostBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
```

## Модули

Различный функционал подключается к движку как модули. Модуль это класс реализующий `IBioEngineModule`.
Для упрощения можно наследоваться от класса `BaseBioEngineModule`, в котором реализованы основные функции.

Модуль должен реализовать 3 метода.

### ConfigureServices

Позволяет настроить DI-контейнер

### RegisterValidation

Позволяет зарегистрировать валидаторы. В `BaseBioEngineModule` все валидаторы будут найдены и зарегистрированы автоматически.

### ConfigureEntities

Позволяет зарегистрировать сущности базы данных и их репозитории, а так же сконфигурировать `DbContext`. 

Все классы реализующие `IEntity` и `IBioRepository<TEntity>` или отнаследованные от `ContentBlock`, `ContentItem` или `Section` будут зарегистрированы автоматически. Для дополнительной настройки контекста модуль должен переопределить метод `ConfigureEntities`. 

```csharp
public override void ConfigureEntities(IServiceCollection serviceCollection, BioEntitiesManager entitiesManager)
{
    base.ConfigureEntities(serviceCollection, entitiesManager);
    entitiesManager.ConfigureDbContext(modelBuilder =>
    {
        modelBuilder.Entity<Ad>().HasMany(contentItem => contentItem.Blocks).WithOne()
            .HasForeignKey(c => c.ContentId);
    });
}
```

Если модулю необходима конфигурация, то он должен реализовать интерфейс `IBioEngineModule<TConfig>` и подключаться через метод `BioEngine.AddModule<TModule, TConfig>`.

## База данных, сущности и репозитории

В качестве абстракции базы данных используется `EntityFramework Core`.

Для работы с базой данных необходимо подключить модуль, реализующий подключение к конкретной СУБД. В ядре реализован `PostgresDatabaseModule` для работы с Postgresql. Для запуска тестов рекомендуется использовать `InMemoryDatabaseModule`.

Все сущности, хранящиеся в БД должны реализовать интерфейс `IEntity`. Всё операции происходят через `BioContext`, его нельзя наследоват или заменять, но можно конфигурировать при инициализации модуля.

Для упрощения базовых операций в ядре реализована система репозиториев. Репозиторий это класс реализующий `IBioRepository<TEntity>`. 
В нём присутствуют методы для поиска, подсчёта, создания, обновления, валидации и удаления сущностей.

### Выборка списка

Возвращает кортеж, в котором первым элементом идёт массив сущностей, а вторым - полное количество подходящих под условия сущностей.

```csharp
Task<(TEntity[] items, int itemsCount)> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>>? configureQuery = null);
```

Пример использования

```csharp
var (items, itemsCount) = await Repository.GetAllAsync(entities => entities.Where(e => e.IsPublished));
```

### Выборка списка по ID

Возвращает массив сущностей по их ID

```csharp
Task<TEntity[]> GetByIdsAsync(Guid[] ids, Func<IQueryable<TEntity>, IQueryable<TEntity>>? configureQuery = null);
```

Пример использования

```csharp
var entities = await Repository.GetByIdsAsync(new []{
    "a2721c9d-6770-42f1-accd-0f08a5ea4407",
    "ffd80adf-6dbf-4b86-b486-1546c37ed29a", 
    "88c40418-f6cb-4568-8f0e-e4b361ba8657",
    "07278a8b-98a1-45ef-8582-73bed3c9c728"
});
```

### Выбор одной сущности

Возвращает первую сущность, удовлетворяющую результатам запроса

```csharp
Task<TEntity> GetAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> configureQuery);
```

Пример использования:

```csharp
var entity = await Repository.GetAsync(entities => entities.Where(e => e.Url == url && e.IsPublished));
```

### Выбор одной сущности по ID

Возвращает сущность с указанным ID

```csharp
Task<TEntity> GetByIdAsync(Guid id, Func<IQueryable<TEntity>, IQueryable<TEntity>>? configureQuery = null);
```

Пример использования:

```csharp
var entity = await Repository.GetByIdAsync("df5ca168-f3e8-4b8b-a561-fd79a978028d");
```

### Подсчёт результатов

Возвращается количество попадающих под условия запроса сущностей

```csharp
Task<int> CountAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>>? configureQuery = null);
```

Пример использования

```csharp
var count = await Repository.CountAsync(entities => entities.Where(e => e.IsPublished));
```

### Создание сущности

Создаёт новый экземпляр сущности и инициализирует её

```csharp
Task<TEntity> NewAsync();
```

Пример использования

```csharp
var entity = await Repository.NewAsync();
``` 

Валидирует и записывает сущность в базу данных. Возвращает объект `AddOrUpdateOperationResult<TEntity>` со статусом успешности операции, сущностью и ошибками валидации

```csharp
Task<AddOrUpdateOperationResult<TEntity>> AddAsync(TEntity item, IBioRepositoryOperationContext? operationContext = null);
```

Пример использования

```csharp
var result = await Repository.AddAsync(entity, new BioRepositoryOperationContext {User = CurrentUser});
if (result.IsSuccess)
{
    await AfterSaveAsync(result.Entity, result.Changes, item);
    return Created(await MapRestModelAsync(result.Entity));
}
```

### Обновление сущности

Валидирует и обновляет сущность в базе данных. Возвращает объект `AddOrUpdateOperationResult<TEntity>` со статусом успешности операции, сущностью, ошибками валидации и списком изменившихся свойств.

```csharp
Task<AddOrUpdateOperationResult<TEntity>> UpdateAsync(TEntity item, IBioRepositoryOperationContext? operationContext = null);
```

Пример использования

```csharp
var result = await Repository.UpdateAsync(entity, new BioRepositoryOperationContext {User = CurrentUser});
if (result.IsSuccess)
{
    await AfterSaveAsync(result.Entity, result.Changes, item);
    return Updated(await MapRestModelAsync(result.Entity));
}
```

### Удаление сущности

Удаляет сущность. Возвращает удалённую сущность.

```csharp
Task<TEntity> DeleteAsync(TEntity item, IBioRepositoryOperationContext? operationContext = null);
```

Пример использования

```csharp
var deletedEntity = await Repository.DeleteAsync(entity, new BioRepositoryOperationContext {User = CurrentUser});
```

### Удаление сущности по ID

Удаляет сущность. Выбрасывает ArgumentException если сущность не найдена. Возвращает удалённую сущность.

```csharp
Task<TEntity> DeleteAsync(Guid id, IBioRepositoryOperationContext? operationContext = null);
```

Пример использования

```csharp
var deletedEntity = await Repository.DeleteAsync(id, new BioRepositoryOperationContext {User = CurrentUser});
```


Валидация сущностей реализована с помощью библиотеки `FluentValidation`. Настройка валидации сущностей происходит путём добавления в DI-контейнер классов, реализующих `IValidator<TEntity>`. По умолчанию все валидаторы из сборки модуля будут зарегистрированы автоматически. Чтобы изменить это поведение модель должен переопределить метод `RegisterValidation`. 

## Сайты, разделы, контент и блоки

Движок реализует следующую структуру данных. Корневой сущностью является `Site`. Следующим уровнем организации контента являются разделы - `Section`. 
Каждый раздел может быть подключен к одному или нескольким сайтам.

Контентные сущности могут быть "привязаны" к разделам или напрямую к сайтам. 

Контентные блоки являются "умными единицами контента" и используются для сборки "типизированного" содержимого в контентных сущностях.

Разделы, блоки и конент являются типизируемыми, что позволяет расширять функционал движка. 
Ядро не содержит реализаций разделов и контента. Референсные реализации контента есть в модулях `Core.Pages` и `Core.Posts`.

### Типизация сущностей     

Класс типизируемая сущности должен реализовать интерфейс `ITypedEntity<TData>`, а так же содержать аттрибут `TypedEntity` с указанием уникального дескриминатора.
Дополнительные данные хранятся в свойстве Data и при сохранении в базу данных автоматически конвертируются в JSON. При загрузке из БД происходит автоматическая десериализация данных.
