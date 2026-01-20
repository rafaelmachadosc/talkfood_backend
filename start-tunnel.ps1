# Script para iniciar o Cloudflare Tunnel

Write-Host "=== Iniciando Cloudflare Tunnel ===" -ForegroundColor Cyan

# Verificar se o arquivo de configuração existe
if (-not (Test-Path "cloudflare-config.yml")) {
    Write-Host "ERRO: cloudflare-config.yml não encontrado!" -ForegroundColor Red
    Write-Host "Execute este script na raiz do projeto (onde está o cloudflare-config.yml)" -ForegroundColor Yellow
    exit 1
}

# Verificar se as credenciais existem
if (-not (Test-Path ".cloudflare\talkfood-app.json")) {
    Write-Host "ERRO: Credenciais não encontradas em .cloudflare\talkfood-app.json" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Configuração encontrada" -ForegroundColor Green
Write-Host "✓ Credenciais encontradas" -ForegroundColor Green

# Verificar se já existe um processo cloudflared rodando
$existing = Get-Process -Name "cloudflared" -ErrorAction SilentlyContinue
if ($existing) {
    Write-Host "`nAVISO: Já existe um processo cloudflared rodando (PID: $($existing.Id))" -ForegroundColor Yellow
    Write-Host "Deseja parar e reiniciar? (S/N)" -ForegroundColor Yellow
    $response = Read-Host
    if ($response -eq "S" -or $response -eq "s") {
        Stop-Process -Name "cloudflared" -Force
        Start-Sleep -Seconds 2
    } else {
        Write-Host "Mantendo processo existente..." -ForegroundColor Yellow
        exit 0
    }
}

Write-Host "`nIniciando Cloudflare Tunnel..." -ForegroundColor Yellow
Write-Host "Pressione Ctrl+C para parar`n" -ForegroundColor Gray

# Iniciar o tunnel
Start-Process -FilePath "cloudflared" -ArgumentList "tunnel","--config","cloudflare-config.yml","run" -NoNewWindow

Write-Host "Tunnel iniciado! Verifique os logs acima para confirmar a conexão." -ForegroundColor Green
Write-Host "`nA aplicação deve estar rodando na porta 8080 para o tunnel funcionar." -ForegroundColor Cyan
