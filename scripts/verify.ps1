$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
dotnet restore "$projectRoot\backend\RauchERP.slnx"
dotnet build "$projectRoot\backend\RauchERP.slnx" --no-restore
dotnet test "$projectRoot\backend\RauchERP.slnx" --no-build --no-restore
Push-Location "$projectRoot\frontend"
try {
    npm ci
    npm run lint
    npm test -- --watch=false
    npm run build
    npx playwright test --list
}
finally { Pop-Location }
