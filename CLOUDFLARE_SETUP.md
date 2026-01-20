# Configuração do Cloudflare Tunnel para talkfoodsoftwerk.net

## Pré-requisitos

1. Conta no Cloudflare com o domínio `talkfoodsoftwerk.net` configurado
2. Cloudflare Tunnel instalado (`cloudflared`)
3. Acesso ao Cloudflare Dashboard

## Passo a Passo

### 1. Instalar Cloudflare Tunnel

**Windows:**
```powershell
# Via winget
winget install --id Cloudflare.cloudflared

# Ou baixe de: https://github.com/cloudflare/cloudflared/releases
```

**Linux/Mac:**
```bash
# Linux
wget https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64
chmod +x cloudflared-linux-amd64
sudo mv cloudflared-linux-amd64 /usr/local/bin/cloudflared

# Mac
brew install cloudflared
```

### 2. Fazer Login no Cloudflare

```bash
cloudflared tunnel login
```

Isso abrirá seu navegador para autenticar. Selecione o domínio `talkfoodsoftwerk.net`.

### 3. Criar o Tunnel

```bash
cloudflared tunnel create talkfood-app
```

Isso criará o tunnel e salvará as credenciais em `~/.cloudflare/talkfood-app.json` (ou `.cloudflare/talkfood-app.json` no Windows).

### 4. Configurar o DNS

No Cloudflare Dashboard:
1. Vá para **DNS** > **Records**
2. Adicione um registro CNAME:
   - **Name**: `@` (ou deixe em branco para o domínio raiz)
   - **Target**: `{tunnel-id}.cfargotunnel.com`
   - **Proxy**: ✅ Proxied (laranja)
3. Adicione também para `www`:
   - **Name**: `www`
   - **Target**: `{tunnel-id}.cfargotunnel.com`
   - **Proxy**: ✅ Proxied (laranja)

Para obter o `{tunnel-id}`, execute:
```bash
cloudflared tunnel list
```

### 5. Configurar o Arquivo de Configuração

O arquivo `cloudflare-config.yml` já está criado na raiz do projeto:

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

**Importante:** Certifique-se de que o arquivo de credenciais está no caminho correto:
- Windows: `.cloudflare/talkfood-app.json` (na raiz do projeto)
- Linux/Mac: `~/.cloudflare/talkfood-app.json` ou `.cloudflare/talkfood-app.json`

### 6. Mover o Arquivo de Credenciais (se necessário)

Se o arquivo de credenciais foi criado em `~/.cloudflare/`, mova-o para a raiz do projeto:

**Windows:**
```powershell
# Criar pasta se não existir
New-Item -ItemType Directory -Force -Path .cloudflare

# Copiar arquivo (ajuste o caminho se necessário)
Copy-Item "$env:USERPROFILE\.cloudflare\talkfood-app.json" -Destination ".cloudflare\talkfood-app.json"
```

**Linux/Mac:**
```bash
mkdir -p .cloudflare
cp ~/.cloudflare/talkfood-app.json .cloudflare/talkfood-app.json
```

### 7. Testar o Tunnel Manualmente

Antes de rodar a aplicação, teste o tunnel:

```bash
cloudflared tunnel --config cloudflare-config.yml run
```

Você deve ver mensagens indicando que o tunnel está conectado e rodando.

### 8. Configurar a Aplicação

No arquivo `appsettings.json` (ou `appsettings.Production.json`):

```json
{
  "Tunnel": {
    "Strategy": "cloudflare",
    "Cloudflare": {
      "Name": "talkfood-app",
      "ConfigFile": "cloudflare-config.yml",
      "Domain": "talkfoodsoftwerk.net"
    }
  }
}
```

### 9. Executar a Aplicação

```bash
dotnet run --project Backend.Api
```

A aplicação irá:
1. Iniciar na porta 8080
2. Conectar automaticamente ao Cloudflare Tunnel
3. Expor a API em `https://talkfoodsoftwerk.net`

### 10. Verificar

Acesse:
- `https://talkfoodsoftwerk.net/api/health` - Health check
- `https://talkfoodsoftwerk.net/swagger` - Swagger UI (se em desenvolvimento)

## Troubleshooting

### Tunnel não conecta

1. Verifique se o `cloudflared` está instalado e no PATH:
   ```bash
   cloudflared --version
   ```

2. Verifique se o arquivo de credenciais existe:
   ```bash
   # Windows
   Test-Path .cloudflare\talkfood-app.json
   
   # Linux/Mac
   ls -la .cloudflare/talkfood-app.json
   ```

3. Teste o tunnel manualmente:
   ```bash
   cloudflared tunnel --config cloudflare-config.yml run
   ```

### DNS não resolve

1. Verifique os registros CNAME no Cloudflare Dashboard
2. Aguarde alguns minutos para propagação DNS
3. Verifique se o proxy está ativado (ícone laranja)

### Erro de permissão

Certifique-se de que a aplicação tem permissão para:
- Ler o arquivo `cloudflare-config.yml`
- Ler o arquivo `.cloudflare/talkfood-app.json`
- Executar o comando `cloudflared`

## Comandos Úteis

```bash
# Listar tunnels
cloudflared tunnel list

# Ver informações do tunnel
cloudflared tunnel info talkfood-app

# Deletar tunnel (se necessário)
cloudflared tunnel delete talkfood-app

# Ver rotas do tunnel
cloudflared tunnel route dns list
```

## Segurança

- ✅ O arquivo `.cloudflare/talkfood-app.json` contém credenciais sensíveis
- ✅ Adicione `.cloudflare/` ao `.gitignore` (já está incluído)
- ✅ Não commite o arquivo de credenciais no repositório
- ✅ Use variáveis de ambiente para produção
