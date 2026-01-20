# Script para criar arquivo de credenciais do tunnel

Write-Host "=== Criando arquivo de credenciais ===" -ForegroundColor Cyan

$tunnelId = "0a37b840-baeb-4f7e-8b4b-57d98fe6b5c6"
$tunnelName = "talkfood-app"

# Criar diretório se não existir
New-Item -ItemType Directory -Force -Path ".cloudflare" | Out-Null

# Tentar obter informações do certificado para extrair AccountTag
$certPath = "$env:USERPROFILE\.cloudflared\cert.pem"
if (Test-Path $certPath) {
    Write-Host "Certificado encontrado, extraindo informações..." -ForegroundColor Yellow
    
    # O certificado contém um token JWT base64
    $certContent = Get-Content $certPath -Raw
    if ($certContent -match "-----BEGIN ARGO TUNNEL TOKEN-----") {
        Write-Host "Token encontrado no certificado" -ForegroundColor Green
        
        # Tentar executar o tunnel sem arquivo de credenciais primeiro
        # O cloudflared pode criar automaticamente
        Write-Host "`nTentando obter credenciais do tunnel..." -ForegroundColor Yellow
        Write-Host "Execute manualmente:" -ForegroundColor Cyan
        Write-Host "  cloudflared tunnel token $tunnelId" -ForegroundColor White
    }
}

# Criar arquivo de credenciais básico
# Nota: Você precisará preencher os valores reais
$credentials = @{
    AccountTag = ""
    TunnelSecret = ""
    TunnelID = $tunnelId
    TunnelName = $tunnelName
} | ConvertTo-Json

Write-Host "`nPara obter as credenciais reais, execute:" -ForegroundColor Yellow
Write-Host "  cloudflared tunnel token $tunnelId" -ForegroundColor White
Write-Host "`nOu verifique no Cloudflare Dashboard:" -ForegroundColor Yellow
Write-Host "  https://dash.cloudflare.com -> Zero Trust -> Networks -> Tunnels" -ForegroundColor White
