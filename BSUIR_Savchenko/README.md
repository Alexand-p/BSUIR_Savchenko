# BSUIR_Savchenko — КР № 1 и № 2

ASP.NET Core MVC / Razor Pages / REST API / SQLite / Entity Framework Core 10.

## Запуск (Windows PowerShell)

Открыть папку `BSUIR_Savchenko`, в двух терминалах выполнить:

Терминал 1 — API (порт 5101):

```powershell
dotnet run --project .\src\BSUIR_Savchenko.API
```

Терминал 2 — UI (порт 5000):

```powershell
dotnet run --project .\src\BSUIR_Savchenko.UI
```

Браузер: http://localhost:5000

- `КР1` — форма из первой контрольной.
- `Каталог` — книги, категории, фильтр и пагинация по 3 элемента.
- `Администрирование` — таблица, добавление, изменение и удаление книг.

API доступен на http://localhost:5101/api/books и http://localhost:5101/api/categories.
SQLite создаётся автоматически при первом запуске в `src/BSUIR_Savchenko.API/App_Data/catalog.db`.
Изображения находятся в `src/BSUIR_Savchenko.API/wwwroot/images`.

## Проверка

```powershell
dotnet build
Invoke-RestMethod http://localhost:5101/api/categories
Invoke-RestMethod 'http://localhost:5101/api/books?pageNo=1'
Invoke-RestMethod 'http://localhost:5101/api/books?pageNo=2&categoryNormalizedName=programming-books'
```

При добавлении/редактировании изображения принимаются PNG, JPEG и WebP до 4 МБ.
Учебный проект: авторизация администратора не реализована, развёртывание должно быть локальным.
