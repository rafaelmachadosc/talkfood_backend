# Troubleshooting - Erro 404 no talkfoodsoftwerk.net

## ✅ Diagnóstico

Se você está vendo um erro 404 ao acessar `www.talkfoodsoftwerk.net`, significa que:
- ✅ O DNS está configurado corretamente (senão não chegaria na página)
- ✅ O Cloudflare está respondendo
- ❌ O tunnel não está roteando para a aplicação

## 🔍 Verificações

### 1. A aplicação está rodando?

Verifique se há um processo dotnet rodando:
```powershell
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
```

Se não estiver, inicie:
```powershell
dotnet run --project Backend.Api
```

A aplicação deve estar rodando na porta 8080.

### 2. O tunnel está rodando?

Verifique se há um processo cloudflared rodando:
```powershell
Get-Process -Name "cloudflared" -ErrorAction SilentlyContinue
```

Se não estiver, inicie:
```powershell
# Opção 1: Script automático
.\start-tunnel.ps1

# Opção 2: Manual
cloudflared tunnel --config cloudflare-config.yml run
```

### 3. A porta 8080 está acessível?

Teste se a porta está respondendo:
```powershell
Test-NetConnection -ComputerName localhost -Port 8080
```

Ou acesse diretamente: `http://localhost:8080/api/health`

### 4. Verificar configuração do tunnel

O arquivo `cloudflare-config.yml` deve ter:
```yaml
tunnel: talkfood-app
credentials-file: .cloudflare/talkfood-app.json

ingress:
  - hostname: talkfoodsoftwerk.net
    service: http://localhost:8080
  - hostname: www.talkfoodsoftwerk.net
    service: http://localhost:8080
  - service: http_status:404
```

### 5. Verificar credenciais

O arquivo `.cloudflare/talkfood-app.json` deve existir e conter:
```json
{
  "AccountTag": "2161d023ab3b3b529d75a17516613623",
  "TunnelSecret": "NGNlYjJjYTMtMWQ1Zi00NzhiLThjMmEtOGU3ZDZmMTM2Y2Zk",
  "TunnelID": "0a37b840-baeb-4f7e-8b4b-57d98fe6b5c6",
  "TunnelName": "talkfood-app"
}
```

## 🚀 Solução Rápida

### Opção 1: Iniciar tudo manualmente

**Terminal 1 - Tunnel:**
```powershell
cd "c:\Users\Rafael Machado\Downloads\Backend"
cloudflared tunnel --config cloudflare-config.yml run
```

**Terminal 2 - Aplicação:**
```powershell
cd "c:\Users\Rafael Machado\Downloads\Backend"
dotnet run --project Backend.Api
```

### Opção 2: Usar script automático

```powershell
cd "c:\Users\Rafael Machado\Downloads\Backend"
.\start-all.ps1
```

## 📋 Checklist

- [ ] Aplicação rodando na porta 8080
- [ ] Tunnel cloudflared rodando
- [ ] Arquivo `cloudflare-config.yml` existe e está correto
- [ ] Arquivo `.cloudflare/talkfood-app.json` existe
- [ ] DNS configurado no Cloudflare Dashboard
- [ ] Acessar `http://localhost:8080/api/health` funciona localmente

## 🔧 Comandos Úteis

```powershell
# Ver processos rodando
Get-Process -Name "dotnet","cloudflared" -ErrorAction SilentlyContinue

# Parar tudo
Stop-Process -Name "dotnet","cloudflared" -ErrorAction SilentlyContinue

# Ver logs do tunnel
cloudflared tunnel --config cloudflare-config.yml run --loglevel debug

# Testar conexão local
Invoke-WebRequest -Uri "http://localhost:8080/api/health"
```

## ⚠️ Problemas Comuns

### Tunnel não conecta
- Verifique se as credenciais estão corretas
- Verifique se o certificado existe: `Test-Path "$env:USERPROFILE\.cloudflared\cert.pem"`
- Tente fazer login novamente: `cloudflared tunnel login`

### Aplicação não inicia
- Verifique se o PostgreSQL está rodando
- Verifique a connection string no `appsettings.json`
- Execute `dotnet restore` e `dotnet build`

### DNS não resolve
- Aguarde alguns minutos para propagação
- Verifique no Cloudflare Dashboard se os CNAMEs estão corretos
- Certifique-se de que o proxy está ativado (ícone laranja)
