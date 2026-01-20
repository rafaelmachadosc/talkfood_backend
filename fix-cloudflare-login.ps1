# Script para resolver problema de login do Cloudflare Tunnel

Write-Host "=== Resolvendo problema de login do Cloudflare Tunnel ===" -ForegroundColor Cyan

# Verificar se o certificado existe
$certPath = "$env:USERPROFILE\.cloudflared\cert.pem"
if (Test-Path $certPath) {
    Write-Host "`n[1/3] Certificado antigo encontrado: $certPath" -ForegroundColor Yellow
    
    # Fazer backup do certificado antigo
    $backupPath = "$env:USERPROFILE\.cloudflared\cert.pem.backup"
    Copy-Item $certPath -Destination $backupPath -Force
    Write-Host "✓ Backup criado em: $backupPath" -ForegroundColor Green
    
    # Remover certificado antigo
    Remove-Item $certPath -Force
    Write-Host "✓ Certificado antigo removido" -ForegroundColor Green
} else {
    Write-Host "`n[1/3] Nenhum certificado antigo encontrado" -ForegroundColor Green
}

# Verificar estrutura de diretórios
Write-Host "`n[2/3] Verificando estrutura de diretórios..." -ForegroundColor Yellow
if (-not (Test-Path "$env:USERPROFILE\.cloudflared")) {
    New-Item -ItemType Directory -Path "$env:USERPROFILE\.cloudflared" -Force | Out-Null
    Write-Host "✓ Diretório .cloudflared criado" -ForegroundColor Green
} else {
    Write-Host "✓ Diretório .cloudflared já existe" -ForegroundColor Green
}

# Instruções para login
Write-Host "`n[3/3] Pronto para fazer login!" -ForegroundColor Yellow
Write-Host "`nExecute o seguinte comando:" -ForegroundColor Cyan
Write-Host "  cloudflared tunnel login" -ForegroundColor White
Write-Host "`nIsso abrirá seu navegador para autenticação." -ForegroundColor White
Write-Host "Selecione o domínio: talkfoodsoftwerk.net" -ForegroundColor Yellow

Write-Host "`n=== Concluído! ===" -ForegroundColor Green
