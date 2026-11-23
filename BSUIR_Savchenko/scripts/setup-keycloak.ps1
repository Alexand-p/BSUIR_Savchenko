$ErrorActionPreference = "Stop"
$root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$runtime = Join-Path $root "keycloak\runtime"
$settingsPath = Join-Path $runtime "local-settings.json"
$importPath = Join-Path $runtime "bsuir-realm.json"
$uiProject = Join-Path $root "src\BSUIR_Savchenko.UI"

if (!(Get-Command dotnet -ErrorAction SilentlyContinue)) { throw "Требуется .NET SDK 10." }
if (!(Get-Command docker -ErrorAction SilentlyContinue)) { throw "Требуется Docker Desktop." }

New-Item -ItemType Directory -Path $runtime -Force | Out-Null
function New-Secret {
    $bytes = New-Object byte[] 32
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try { $rng.GetBytes($bytes) } finally { $rng.Dispose() }
    return [Convert]::ToBase64String($bytes).TrimEnd('=').Replace('+','-').Replace('/','_')
}

if (Test-Path $settingsPath) {
    $settings = Get-Content -LiteralPath $settingsPath -Raw | ConvertFrom-Json
} else {
    $settings = [PSCustomObject]@{
        ClientSecret = New-Secret
        PowerUserPassword = (New-Secret) + "A1!"
    }
    $settings | ConvertTo-Json | Set-Content -LiteralPath $settingsPath -Encoding UTF8
}

$templatePath = Join-Path $root "keycloak\bsuir-realm.template.json"
$template = [System.IO.File]::ReadAllText($templatePath)
$template = $template.Replace("__CLIENT_SECRET__", $settings.ClientSecret)
$template = $template.Replace("__POWERUSER_PASSWORD__", $settings.PowerUserPassword)
[System.IO.File]::WriteAllText($importPath, $template, (New-Object System.Text.UTF8Encoding($false)))

dotnet user-secrets set "Keycloak:ClientSecret" $settings.ClientSecret --project $uiProject
if ($LASTEXITCODE -ne 0) { throw "Не удалось записать секрет OIDC." }

Push-Location $root
try {
    docker compose up -d
    if ($LASTEXITCODE -ne 0) { throw "Не удалось запустить Keycloak." }
} finally {
    Pop-Location
}

Write-Host "Keycloak: http://localhost:8080" -ForegroundColor Green
Write-Host "Realm: bsuir"
Write-Host "Учебный пользователь: poweruser"
Write-Host "Пароль: $($settings.PowerUserPassword)"
Write-Host "Пароль администратора Keycloak: admin-local-only"
Write-Host "Данные тестовые, только для локальной разработки."
