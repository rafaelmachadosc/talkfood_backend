# Script para iniciar a aplicação Backend.Api

Write-Host "=== Iniciando Aplicação Backend.Api ===" -ForegroundColor Cyan

# Verificar se estamos no diretório correto
if (-not (Test-Path "Backend.Api\Backend.Api.csproj")) {
    Write-Host "ERRO: Execute este script na raiz do projeto Backend" -ForegroundColor Red
    Write-Host "Diretório atual: $(Get-Location)" -ForegroundColor Yellow
    exit 1
}

# Verificar se já está rodando
$dotnetProcess = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
if ($dotnetProcess) {
    Write-Host "AVISO: Já existe um processo dotnet rodando (PID: $($dotnetProcess.Id))" -ForegroundColor Yellow
    Write-Host "Deseja parar e reiniciar? (S/N)" -ForegroundColor Yellow
    $response = Read-Host
    if ($response -eq "S" -or $response -eq "s") {
        Stop-Process -Name "dotnet" -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 2
        Write-Host "Processo anterior parado." -ForegroundColor Green
    } else {
        Write-Host "Mantendo processo existente..." -ForegroundColor Yellow
        exit 0
    }
}

# Verificar se a porta 8080 está livre
$portInUse = netstat -an | findstr ":8080"
if ($portInUse -and $portInUse -notmatch "LISTENING") {
    Write-Host "AVISO: Porta 8080 pode estar em uso" -ForegroundColor Yellow
}

Write-Host "`nIniciando aplicação na porta 8080..." -ForegroundColor Yellow
Write-Host "Pressione Ctrl+C para parar`n" -ForegroundColor Gray

# Iniciar aplicação
dotnet run --project Backend.Api

Write-Host "`nAplicação parada." -ForegroundColor Yellow
