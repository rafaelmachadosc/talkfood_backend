# Script para iniciar aplicação e tunnel juntos

Write-Host "=== Iniciando Aplicação e Cloudflare Tunnel ===" -ForegroundColor Cyan

# Verificar se estamos no diretório correto
if (-not (Test-Path "Backend.Api\Backend.Api.csproj")) {
    Write-Host "ERRO: Execute este script na raiz do projeto Backend" -ForegroundColor Red
    exit 1
}

# Verificar configuração do tunnel
if (-not (Test-Path "cloudflare-config.yml")) {
    Write-Host "ERRO: cloudflare-config.yml não encontrado!" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path ".cloudflare\talkfood-app.json")) {
    Write-Host "ERRO: Credenciais não encontradas!" -ForegroundColor Red
    exit 1
}

Write-Host "`n[1/2] Iniciando Cloudflare Tunnel em background..." -ForegroundColor Yellow
$tunnelJob = Start-Job -ScriptBlock {
    Set-Location $using:PWD
    cloudflared tunnel --config cloudflare-config.yml run
}

Start-Sleep -Seconds 3

# Verificar se o tunnel iniciou
$tunnelProcess = Get-Process -Name "cloudflared" -ErrorAction SilentlyContinue
if ($tunnelProcess) {
    Write-Host "✓ Tunnel iniciado (PID: $($tunnelProcess.Id))" -ForegroundColor Green
} else {
    Write-Host "⚠ Tunnel pode não ter iniciado corretamente" -ForegroundColor Yellow
}

Write-Host "`n[2/2] Iniciando aplicação..." -ForegroundColor Yellow
Write-Host "A aplicação será iniciada na porta 8080" -ForegroundColor Cyan
Write-Host "Pressione Ctrl+C para parar tudo`n" -ForegroundColor Gray

# Iniciar aplicação
dotnet run --project Backend.Api

# Quando a aplicação parar, parar o tunnel também
Write-Host "`nParando tunnel..." -ForegroundColor Yellow
Stop-Job $tunnelJob -ErrorAction SilentlyContinue
Remove-Job $tunnelJob -ErrorAction SilentlyContinue
Stop-Process -Name "cloudflared" -ErrorAction SilentlyContinue

Write-Host "Tudo parado." -ForegroundColor Green
