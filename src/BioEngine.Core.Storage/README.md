# Модуль файлов

Модуль реализует работу с загруженными файлами. Все загруженные файлы регистрируются в БД и сохраняются в выбранный бэкенд. В комплекте идут бэкенды для файловой системы и для S3.

## Установка

Подключаем одну из реализаций

```csharp
bioEngine.AddModule<S3StorageModule, S3StorageModuleConfig>((configuration, env) =>
    {
        return new S3StorageModuleConfig("https://cdn.mysite.com", "localhost:9876", "mybucket",
            "myaccesskey", "mysecretkey");
    });
``` 

## Использование

Инжектим `IStorage` и используем его методы

### Загружаем файл

```csharp
var item = await Storage.SaveFileAsync(file, name, path, "storage");
``` 

### Получаем список файлов

```csharp
Storage.ListItemsAsync(path, "storage");
```
