# Script para encontrar e copiar credenciais do Cloudflare Tunnel

Write-Host "=== Procurando credenciais do Cloudflare Tunnel ===" -ForegroundColor Cyan

# Listar todos os arquivos em .cloudflared
Write-Host "`n[1/4] Verificando arquivos em .cloudflared..." -ForegroundColor Yellow
$cloudflaredPath = "$env:USERPROFILE\.cloudflared"
if (Test-Path $cloudflaredPath) {
    $files = Get-ChildItem $cloudflaredPath -ErrorAction SilentlyContinue
    if ($files) {
        Write-Host "Arquivos encontrados:" -ForegroundColor Green
        $files | ForEach-Object { Write-Host "  - $($_.Name)" -ForegroundColor White }
    } else {
        Write-Host "Nenhum arquivo encontrado" -ForegroundColor Yellow
    }
} else {
    Write-Host "Diretório .cloudflared não existe" -ForegroundColor Red
}

# Verificar se o certificado foi criado
Write-Host "`n[2/4] Verificando certificado..." -ForegroundColor Yellow
$certPath = "$cloudflaredPath\cert.pem"
if (Test-Path $certPath) {
    Write-Host "✓ Certificado encontrado: $certPath" -ForegroundColor Green
} else {
    Write-Host "✗ Certificado não encontrado" -ForegroundColor Yellow
}

# Tentar obter informações do tunnel
Write-Host "`n[3/4] Obtendo informações do tunnel..." -ForegroundColor Yellow
$tunnelId = "0a37b840-baeb-4f7e-8b4b-57d98fe6b5c6"
$tunnelName = "talkfood-app"

# Verificar locais possíveis para credenciais
$possiblePaths = @(
    "$cloudflaredPath\$tunnelId.json",
    "$cloudflaredPath\$tunnelName.json",
    "$cloudflaredPath\$tunnelId\config.json",
    "$env:USERPROFILE\.cloudflare\$tunnelName.json"
)

Write-Host "`n[4/4] Verificando locais possíveis..." -ForegroundColor Yellow
$found = $false
foreach ($path in $possiblePaths) {
    if (Test-Path $path) {
        Write-Host "✓ Credenciais encontradas em: $path" -ForegroundColor Green
        
        # Copiar para o projeto
        $destPath = ".cloudflare\talkfood-app.json"
        Copy-Item $path -Destination $destPath -Force
        Write-Host "✓ Credenciais copiadas para: $destPath" -ForegroundColor Green
        $found = $true
        break
    }
}

if (-not $found) {
    Write-Host "`n⚠️ Credenciais não encontradas nos locais padrão" -ForegroundColor Yellow
    Write-Host "`nO cloudflared pode usar apenas o certificado (cert.pem) para autenticação." -ForegroundColor Cyan
    Write-Host "Nesse caso, você pode precisar criar o arquivo de credenciais manualmente." -ForegroundColor Cyan
    Write-Host "`nTente executar:" -ForegroundColor White
    Write-Host "  cloudflared tunnel run talkfood-app --config cloudflare-config.yml" -ForegroundColor Yellow
    Write-Host "`nOu verifique a documentação do Cloudflare para obter as credenciais do tunnel." -ForegroundColor White
}

Write-Host "`n=== Concluído ===" -ForegroundColor Green
