# Solução: Credenciais do Cloudflare Tunnel

## ✅ Status

Após fazer login com `cloudflared tunnel login`, o Cloudflare criou um **certificado** em:
```
C:\Users\Rafael Machado\.cloudflared\cert.pem
```

Este certificado contém o token de autenticação necessário para o tunnel funcionar.

## 🔧 Como Funciona

O Cloudflare Tunnel pode funcionar de duas formas:

1. **Com arquivo de credenciais JSON** (para tunnels específicos)
2. **Com certificado global** (cert.pem) - que é o que você tem agora

## ✅ Solução Atual

O arquivo `cloudflare-config.yml` está configurado para usar o tunnel pelo **nome** (`talkfood-app`), e o cloudflared usará automaticamente o certificado em `~/.cloudflared/cert.pem`.

**Você NÃO precisa criar um arquivo JSON de credenciais!** O certificado é suficiente.

## 🚀 Próximos Passos

### 1. Testar o Tunnel Manualmente

Execute:
```powershell
cloudflared tunnel --config cloudflare-config.yml run
```

Você deve ver mensagens indicando que o tunnel está conectado. Se funcionar, pressione `Ctrl+C` para parar.

### 2. Executar a Aplicação

Em outro terminal:
```powershell
cd "c:\Users\Rafael Machado\Downloads\Backend"
dotnet run --project Backend.Api
```

### 3. Verificar

Acesse:
- `https://talkfoodsoftwerk.net/api/health`

## 📝 Nota Importante

Se você quiser usar um arquivo de credenciais específico (opcional), você pode criar manualmente em `.cloudflare/talkfood-app.json` com o formato:

```json
{
  "AccountTag": "seu-account-tag",
  "TunnelSecret": "seu-tunnel-secret",
  "TunnelID": "0a37b840-baeb-4f7e-8b4b-57d98fe6b5c6",
  "TunnelName": "talkfood-app"
}
```

Mas isso **NÃO é necessário** - o certificado global funciona perfeitamente!

## 🔍 Verificar Configuração

Para verificar se tudo está correto:

```powershell
# Verificar se o certificado existe
Test-Path "$env:USERPROFILE\.cloudflared\cert.pem"

# Verificar configuração
Get-Content cloudflare-config.yml

# Listar tunnels
cloudflared tunnel list
```
