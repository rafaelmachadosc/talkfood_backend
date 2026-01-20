# Setup Rápido - Cloudflare Tunnel para talkfoodsoftwerk.net

## ✅ Status Atual

- ✅ Tunnel `talkfood-app` já existe
- ✅ Tunnel ID: `0a37b840-baeb-4f7e-8b4b-57d98fe6b5c6`
- ✅ Arquivo `cloudflare-config.yml` configurado
- ⚠️ Credenciais precisam ser configuradas

## 📋 Passos para Finalizar

### 1. Obter as Credenciais do Tunnel

Execute:
```powershell
cloudflared tunnel show talkfood-app
```

Ou verifique se existe em:
- `C:\Users\Rafael Machado\.cloudflared\0a37b840-baeb-4f7e-8b4b-57d98fe6b5c6.json`
- `C:\Users\Rafael Machado\.cloudflared\talkfood-app.json`

Se não encontrar, você pode precisar fazer login novamente:
```powershell
# Se houver certificado antigo, remova primeiro (opcional)
Remove-Item "$env:USERPROFILE\.cloudflared\cert.pem" -ErrorAction SilentlyContinue

# Fazer login
cloudflared tunnel login
```

### 2. Copiar Credenciais para o Projeto

Depois de encontrar o arquivo de credenciais, copie para:
```powershell
# Criar diretório se não existir
New-Item -ItemType Directory -Force -Path ".cloudflare"

# Copiar credenciais (ajuste o caminho conforme necessário)
Copy-Item "$env:USERPROFILE\.cloudflared\0a37b840-baeb-4f7e-8b4b-57d98fe6b5c6.json" -Destination ".cloudflare\talkfood-app.json"
```

### 3. Configurar DNS no Cloudflare Dashboard

1. Acesse: https://dash.cloudflare.com
2. Selecione o domínio: **talkfoodsoftwerk.net**
3. Vá em **DNS** > **Records**
4. Adicione/Edite os seguintes registros CNAME:

   **Registro 1:**
   - **Type**: CNAME
   - **Name**: `@` (ou deixe em branco para domínio raiz)
   - **Target**: `0a37b840-baeb-4f7e-8b4b-57d98fe6b5c6.cfargotunnel.com`
   - **Proxy status**: ✅ Proxied (ícone laranja)
   - **TTL**: Auto

   **Registro 2:**
   - **Type**: CNAME
   - **Name**: `www`
   - **Target**: `0a37b840-baeb-4f7e-8b4b-57d98fe6b5c6.cfargotunnel.com`
   - **Proxy status**: ✅ Proxied (ícone laranja)
   - **TTL**: Auto

### 4. Testar o Tunnel Manualmente

Antes de rodar a aplicação, teste o tunnel:
```powershell
cloudflared tunnel --config cloudflare-config.yml run
```

Você deve ver mensagens indicando que está conectado. Pressione `Ctrl+C` para parar.

### 5. Executar a Aplicação

```powershell
dotnet run --project Backend.Api
```

A aplicação irá:
- Iniciar na porta 8080
- Conectar automaticamente ao Cloudflare Tunnel
- Expor a API em `https://talkfoodsoftwerk.net`

### 6. Verificar

Acesse:
- ✅ `https://talkfoodsoftwerk.net/api/health` - Health check
- ✅ `https://talkfoodsoftwerk.net/swagger` - Swagger UI (se em desenvolvimento)

## 🔍 Troubleshooting

### Credenciais não encontradas

Se não encontrar as credenciais, você pode precisar recriar o arquivo. O formato é:
```json
{
  "AccountTag": "seu-account-tag",
  "TunnelSecret": "seu-tunnel-secret",
  "TunnelID": "0a37b840-baeb-4f7e-8b4b-57d98fe6b5c6",
  "TunnelName": "talkfood-app"
}
```

Para obter essas informações:
```powershell
cloudflared tunnel show talkfood-app
```

### DNS não resolve

- Aguarde alguns minutos para propagação DNS
- Verifique se o proxy está ativado (ícone laranja)
- Verifique se os registros CNAME estão corretos

### Tunnel não conecta

1. Verifique se o arquivo de credenciais existe:
   ```powershell
   Test-Path ".cloudflare\talkfood-app.json"
   ```

2. Verifique se o arquivo de configuração está correto:
   ```powershell
   Get-Content cloudflare-config.yml
   ```

3. Teste manualmente:
   ```powershell
   cloudflared tunnel --config cloudflare-config.yml run
   ```

## 📝 Resumo das Configurações

- **Tunnel Name**: `talkfood-app`
- **Tunnel ID**: `0a37b840-baeb-4f7e-8b4b-57d98fe6b5c6`
- **Domain**: `talkfoodsoftwerk.net`
- **Local Port**: `8080`
- **Config File**: `cloudflare-config.yml`
- **Credentials**: `.cloudflare/talkfood-app.json`
