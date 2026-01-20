# Script de configuração do Cloudflare Tunnel para talkfoodsoftwerk.net

Write-Host "=== Configuração do Cloudflare Tunnel ===" -ForegroundColor Cyan

# Verificar se cloudflared está instalado
Write-Host "`n[1/6] Verificando cloudflared..." -ForegroundColor Yellow
$cloudflared = Get-Command cloudflared -ErrorAction SilentlyContinue
if (-not $cloudflared) {
    Write-Host "ERRO: cloudflared não encontrado. Instale com: winget install --id Cloudflare.cloudflared" -ForegroundColor Red
    exit 1
}
Write-Host "✓ cloudflared encontrado: $($cloudflared.Source)" -ForegroundColor Green

# Listar tunnels existentes
Write-Host "`n[2/6] Verificando tunnels existentes..." -ForegroundColor Yellow
$tunnelList = cloudflared tunnel list 2>&1 | Out-String
Write-Host $tunnelList

# Verificar se o tunnel talkfood-app existe
$tunnelId = $null
if ($tunnelList -match "talkfood-app") {
    Write-Host "✓ Tunnel 'talkfood-app' encontrado" -ForegroundColor Green
    
    # Extrair o ID do tunnel (primeira coluna da linha que contém talkfood-app)
    $tunnelLine = ($tunnelList -split "`n" | Where-Object { $_ -match "talkfood-app" }) | Select-Object -First 1
    if ($tunnelLine) {
        $tunnelId = ($tunnelLine -split '\s+')[0]
        Write-Host "  ID do Tunnel: $tunnelId" -ForegroundColor Cyan
    }
} else {
    Write-Host "ERRO: Tunnel 'talkfood-app' não encontrado" -ForegroundColor Red
    Write-Host "Execute: cloudflared tunnel create talkfood-app" -ForegroundColor Yellow
    exit 1
}

# Criar diretório .cloudflare se não existir
Write-Host "`n[3/6] Preparando estrutura de diretórios..." -ForegroundColor Yellow
if (-not (Test-Path ".cloudflare")) {
    New-Item -ItemType Directory -Path ".cloudflare" | Out-Null
    Write-Host "✓ Diretório .cloudflare criado" -ForegroundColor Green
} else {
    Write-Host "✓ Diretório .cloudflare já existe" -ForegroundColor Green
}

# Verificar e copiar credenciais
Write-Host "`n[4/6] Verificando credenciais..." -ForegroundColor Yellow
$credentialPaths = @(
    "$env:USERPROFILE\.cloudflared\$tunnelId.json",
    "$env:USERPROFILE\.cloudflared\talkfood-app.json",
    "$env:USERPROFILE\.cloudflare\talkfood-app.json"
)

$credentialsFound = $false
foreach ($path in $credentialPaths) {
    if (Test-Path $path) {
        Write-Host "✓ Credenciais encontradas em: $path" -ForegroundColor Green
        Copy-Item $path -Destination ".cloudflare\talkfood-app.json" -Force
        Write-Host "✓ Credenciais copiadas para .cloudflare\talkfood-app.json" -ForegroundColor Green
        $credentialsFound = $true
        break
    }
}

if (-not $credentialsFound) {
    Write-Host "AVISO: Credenciais não encontradas nos locais padrão" -ForegroundColor Yellow
    Write-Host "Você pode precisar executar: cloudflared tunnel login" -ForegroundColor Yellow
}

# Verificar arquivo de configuração
Write-Host "`n[5/6] Verificando arquivo de configuração..." -ForegroundColor Yellow
if (Test-Path "cloudflare-config.yml") {
    Write-Host "✓ cloudflare-config.yml encontrado" -ForegroundColor Green
    
    # Verificar se o arquivo está correto
    $configContent = Get-Content "cloudflare-config.yml" -Raw
    if ($configContent -match "talkfood-app" -and $configContent -match "talkfoodsoftwerk.net") {
        Write-Host "✓ Configuração parece correta" -ForegroundColor Green
    } else {
        Write-Host "AVISO: Verifique o conteúdo do cloudflare-config.yml" -ForegroundColor Yellow
    }
} else {
    Write-Host "ERRO: cloudflare-config.yml não encontrado" -ForegroundColor Red
    exit 1
}

# Configurar DNS (se necessário)
Write-Host "`n[6/6] Configuração DNS..." -ForegroundColor Yellow
Write-Host "IMPORTANTE: Configure manualmente no Cloudflare Dashboard:" -ForegroundColor Cyan
Write-Host "  1. Acesse: https://dash.cloudflare.com" -ForegroundColor White
Write-Host "  2. Selecione o domínio: talkfoodsoftwerk.net" -ForegroundColor White
Write-Host "  3. Vá em DNS > Records" -ForegroundColor White
Write-Host "  4. Adicione CNAME:" -ForegroundColor White
Write-Host "     - Name: @ (ou deixe em branco)" -ForegroundColor White
Write-Host "     - Target: $tunnelId.cfargotunnel.com" -ForegroundColor Yellow
Write-Host "     - Proxy: ON (ícone laranja)" -ForegroundColor White
Write-Host "  5. Adicione outro CNAME:" -ForegroundColor White
Write-Host "     - Name: www" -ForegroundColor White
Write-Host "     - Target: $tunnelId.cfargotunnel.com" -ForegroundColor Yellow
Write-Host "     - Proxy: ON (ícone laranja)" -ForegroundColor White

Write-Host "`n=== Configuração concluída! ===" -ForegroundColor Green
Write-Host "`nPróximos passos:" -ForegroundColor Cyan
Write-Host "1. Configure o DNS no Cloudflare Dashboard (veja acima)" -ForegroundColor White
Write-Host "2. Execute: dotnet run --project Backend.Api" -ForegroundColor White
Write-Host "3. Acesse: https://talkfoodsoftwerk.net/api/health" -ForegroundColor White
