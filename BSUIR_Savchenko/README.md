# BSUIR_Savchenko — КР № 1, КР № 2, ИПР № 1

ASP.NET Core MVC / Razor Pages / REST API / Entity Framework Core / SQLite / Keycloak / Serilog.

## Требования

- .NET SDK 10
- Docker Desktop (запущенный Docker Engine)
- Windows PowerShell 5.1 или новее

## Первоначальная настройка Keycloak

В PowerShell из папки `BSUIR_Savchenko`:

```powershell
.\scripts\setup-keycloak.ps1
```

Скрипт создаёт локальные секреты, сохраняет OIDC client secret через `dotnet user-secrets`, запускает Keycloak на `http://localhost:8180` и печатает случайный пароль пользователя `poweruser` с ролью `POWER-USER`. Секреты, аватары и журналы игнорируются Git. Не загружайте `keycloak/runtime` в репозиторий.

При первом запуске контейнер может загружать образ. Realm импортируется только если он ещё не существует. Для сохранения пользователей не удаляйте том Docker `keycloak-db`. При повторном запуске используется прежний client secret.

В проекте намеренно используется локальный Keycloak `start-dev` с HTTP и встроенной БД. Для публичного размещения нужны HTTPS, безопасное хранение секретов и постоянная СУБД. PostgreSQL в методичке рекомендован, но не обязателен.

## Запуск приложения

Открыть три окна PowerShell в папке `BSUIR_Savchenko`:

```powershell
docker compose up -d
```

```powershell
dotnet restore
dotnet build
dotnet run --project .\src\BSUIR_Savchenko.API
```

```powershell
dotnet run --project .\src\BSUIR_Savchenko.UI
```

Сайты:

- Keycloak: http://localhost:8180
- API: http://localhost:5101/api/books
- UI: http://localhost:5000

Если Keycloak перестал отвечать, проверьте `docker compose logs --tail=100 keycloak`.

## Вход и роли

- **Register** ведёт на официальную страницу регистрации Keycloak (`prompt=create`). После регистрации пользователь попадает в профиль, где можно загрузить аватар. Эта операция завершает пользовательский сценарий регистрации.
- **Login** запускает стандартный OIDC Authorization Code + PKCE.
- **Logout** завершает cookie-сессию приложения и сессию Keycloak.
- Регистрация создаёт обычного пользователя без роли `POWER-USER`.
- Для проверки администрирования войдите как `poweruser` с паролем, напечатанным скриптом настройки.
- CRUD API защищён `[Authorize(Roles = "POWER-USER")]` на операциях POST/PUT/DELETE. GET остаются публичными.
- Административные Razor Pages также требуют роль `POWER-USER`.
- MVC UI отправляет access JWT в заголовке `Authorization: Bearer ...` только при изменении данных.

## Проверки

1. Открыть каталог без входа — карточки, категории и страницы должны работать.
2. Без токена POST на API должен вернуть 401.
3. Войти как обычный пользователь — раздел администрирования запрещён.
4. Войти как `poweruser` — создать книгу, отредактировать и удалить.
5. Зарегистрироваться и сохранить PNG, JPEG или WebP аватар до 2 МБ в профиле. Аватар показывается возле имени.
6. Открыть несуществующий URL UI `http://localhost:5000/not-found-test`. Проверить `src/BSUIR_Savchenko.UI/logs/non-success-YYYYMMDD.log` — должна появиться запись с кодом 404. В журнал не записываются тела запросов и токены.

Пример запроса к API без авторизации:

```powershell
try {
    Invoke-WebRequest -Uri 'http://localhost:5101/api/books' -Method Post -UseBasicParsing
} catch {
    Write-Host $_.Exception.Response.StatusCode
}
```

По истечении срока действия токена необходимо повторно войти. Автоматическое обновление access token не реализовано.

## Обновление прежнего репозитория

Архив включает полную текущую версию проекта. Распаковать содержимое в каталог `BSUIR_Savchenko_Minimal`, заменяя файлы внутри `BSUIR_Savchenko`, но не удаляя соседнюю папку `.git`. После проверки `dotnet build` и сценариев авторизации выполнить из `BSUIR_Savchenko_Minimal`:

```powershell
git add -A -- BSUIR_Savchenko
git diff --cached --stat
git commit -m "Add IPR1 Keycloak authentication and Serilog middleware"
git push origin main
```
