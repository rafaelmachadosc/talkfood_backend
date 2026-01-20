# ✅ Configuração Final - Cloudflare Tunnel

## 📊 Status do Tunnel

- **Tunnel Name**: `talkfood-app`
- **Tunnel ID**: `0a37b840-baeb-4f7e-8b4b-57d98fe6b5c6`
- **Domain**: `talkfoodsoftwerk.net`
- **Porta Local**: `8080`

## 🚀 Próximos Passos

### 1. Localizar/Criar Credenciais

O arquivo de credenciais deve estar em um destes locais:
- `C:\Users\Rafael Machado\.cloudflared\0a37b840-baeb-4f7e-8b4b-57d98fe6b5c6.json`
- `C:\Users\Rafael Machado\.cloudflared\talkfood-app.json`

**Se não encontrar**, você pode precisar fazer login novamente:

```powershell
# Remover certificado antigo (se necessário)
Remove-Item "$env:USERPROFILE\.cloudflared\cert.pem" -ErrorAction SilentlyContinue

# Fazer login
cloudflared tunnel login
```

Depois, copie as credenciais para o projeto:
```powershell
New-Item -ItemType Directory -Force -Path ".cloudflare"
Copy-Item "$env:USERPROFILE\.cloudflared\0a37b840-baeb-4f7e-8b4b-57d98fe6b5c6.json" -Destination ".cloudflare\talkfood-app.json"
```

### 2. Configurar DNS no Cloudflare

No Cloudflare Dashboard (https://dash.cloudflare.com):

1. Selecione o domínio **talkfoodsoftwerk.net**
2. Vá em **DNS** > **Records**
3. Adicione/Edite:

   **CNAME para @ (domínio raiz):**
   ```
   Type: CNAME
   Name: @
   Target: 0a37b840-baeb-4f7e-8b4b-57d98fe6b5c6.cfargotunnel.com
   Proxy: ON (laranja)
   ```

   **CNAME para www:**
   ```
   Type: CNAME
   Name: www
   Target: 0a37b840-baeb-4f7e-8b4b-57d98fe6b5c6.cfargotunnel.com
   Proxy: ON (laranja)
   ```

### 3. Testar o Tunnel

Teste manualmente antes de rodar a aplicação:
```powershell
cloudflared tunnel --config cloudflare-config.yml run
```

Se funcionar, você verá mensagens de conexão. Pressione `Ctrl+C` para parar.

### 4. Executar a Aplicação

```powershell
dotnet run --project Backend.Api
```

A aplicação irá:
- ✅ Iniciar na porta 8080
- ✅ Conectar automaticamente ao Cloudflare Tunnel
- ✅ Expor em `https://talkfoodsoftwerk.net`

### 5. Verificar Funcionamento

Acesse:
- `https://talkfoodsoftwerk.net/api/health` - Deve retornar `{"status":"ok","message":"API está funcionando"}`
- `https://talkfoodsoftwerk.net/swagger` - Swagger UI (se em desenvolvimento)

## 📁 Arquivos de Configuração

- ✅ `cloudflare-config.yml` - Configuração do tunnel
- ✅ `Backend.Api/appsettings.json` - Configuração da aplicação
- ⚠️ `.cloudflare/talkfood-app.json` - Credenciais (precisa ser criado)

## 🔧 Comandos Úteis

```powershell
# Listar tunnels
cloudflared tunnel list

# Ver informações do tunnel
cloudflared tunnel info 0a37b840-baeb-4f7e-8b4b-57d98fe6b5c6

# Testar configuração
cloudflared tunnel --config cloudflare-config.yml run

# Verificar credenciais
Test-Path ".cloudflare\talkfood-app.json"
```

## ⚠️ Importante

1. **Credenciais**: O arquivo `.cloudflare/talkfood-app.json` contém informações sensíveis e NÃO deve ser commitado no Git (já está no .gitignore)

2. **DNS**: Após configurar o DNS, pode levar alguns minutos para propagar

3. **Porta**: Certifique-se de que a porta 8080 está livre e não bloqueada pelo firewall

4. **Tunnel**: O tunnel precisa estar rodando enquanto a aplicação estiver ativa

## 🎯 Checklist Final

- [ ] Credenciais copiadas para `.cloudflare/talkfood-app.json`
- [ ] DNS configurado no Cloudflare Dashboard
- [ ] Tunnel testado manualmente
- [ ] Aplicação executando na porta 8080
- [ ] Acesso funcionando em `https://talkfoodsoftwerk.net`
