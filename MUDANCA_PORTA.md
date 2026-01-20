# Mudança de Porta: 8080 → 8081

## ✅ Arquivos Atualizados

### Backend
1. ✅ `Backend.Api/Program.cs` - Porta alterada para 8081
2. ✅ `Backend.Api/appsettings.json` - Configuração de porta atualizada
3. ✅ `Backend.Api/Properties/launchSettings.json` - URL de desenvolvimento atualizada
4. ✅ `cloudflare-config.yml` - Serviços do tunnel atualizados para 8081

### Frontend
⚠️ **IMPORTANTE**: Se você tem um frontend separado, você precisa atualizar:

1. **URLs da API** - Todas as chamadas para a API devem apontar para:
   - Desenvolvimento: `http://localhost:8081`
   - Produção: `https://www.talkfoodsoftwerk.net` (não muda, o tunnel roteia automaticamente)

2. **Arquivos para verificar no frontend:**
   - Arquivos de configuração (`.env`, `config.js`, `config.ts`, etc.)
   - Arquivos de serviços/API clients
   - Variáveis de ambiente
   - Arquivos de proxy (se usar)

## 🚀 Como Testar

### 1. Iniciar a Aplicação
```powershell
cd "c:\Users\Rafael Machado\Downloads\Backend"
dotnet run --project Backend.Api
```

A aplicação deve iniciar na porta **8081**.

### 2. Testar Localmente
```powershell
Invoke-WebRequest -Uri "http://127.0.0.1:8081/api/health" -UseBasicParsing
```

Deve retornar: `{"status":"ok","message":"API está funcionando"}`

### 3. Iniciar o Tunnel
```powershell
cloudflared tunnel --config cloudflare-config.yml run
```

### 4. Testar via Domínio
Acesse: `https://www.talkfoodsoftwerk.net/api/health`

## 📝 Notas

- O tunnel do Cloudflare roteia automaticamente, então o domínio público **não precisa mudar**
- Apenas a porta local mudou de 8080 para 8081
- Se você tiver um frontend, atualize as URLs da API para usar a nova porta em desenvolvimento

## 🔍 Verificar Frontend

Se você tem um frontend, procure por:
- `localhost:8080`
- `127.0.0.1:8080`
- `:8080`
- Variáveis de ambiente com `8080`

E atualize para `8081`.
