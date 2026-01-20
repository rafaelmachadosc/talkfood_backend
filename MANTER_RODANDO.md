# Como Manter a Aplicação e Tunnel Rodando

## ✅ Status Atual

Sua API está funcionando! Você conseguiu acessar:
- ✅ `https://www.talkfoodsoftwerk.net/api/health` - Funcionando!
- ✅ `http://localhost:8080/api/health` - Funcionando!

## 🔄 Manter Rodando

Para manter tudo funcionando, você precisa de **dois processos rodando simultaneamente**:

### Terminal 1 - Cloudflare Tunnel (SEMPRE RODANDO)

```powershell
cd "c:\Users\Rafael Machado\Downloads\Backend"
cloudflared tunnel --config cloudflare-config.yml run
```

**Importante:**
- Deixe este terminal aberto o tempo todo
- Se fechar, o domínio para de funcionar
- Você verá logs de conexão quando estiver ativo

### Terminal 2 - Aplicação (SEMPRE RODANDO)

```powershell
cd "c:\Users\Rafael Machado\Downloads\Backend"
dotnet run --project Backend.Api
```

**Importante:**
- Deixe este terminal aberto o tempo todo
- Se fechar, a API para de responder
- Você verá logs da aplicação

## 🚀 Iniciar como Serviço (Opcional - Produção)

Para produção, você pode configurar como serviços do Windows:

### 1. Criar Serviço para a Aplicação

Crie um arquivo `install-app-service.ps1`:
```powershell
# Executar como Administrador
$serviceName = "TalkFoodAPI"
$serviceDisplayName = "TalkFood API Service"
$serviceDescription = "API Backend do TalkFood"
$exePath = "C:\Users\Rafael Machado\Downloads\Backend\Backend.Api\bin\Release\net8.0\Backend.Api.exe"
$workingDir = "C:\Users\Rafael Machado\Downloads\Backend"

New-Service -Name $serviceName `
    -DisplayName $serviceDisplayName `
    -Description $serviceDescription `
    -BinaryPathName "$exePath" `
    -StartupType Automatic
```

### 2. Criar Serviço para o Tunnel

Crie um arquivo `install-tunnel-service.ps1`:
```powershell
# Executar como Administrador
$serviceName = "CloudflareTunnel"
$serviceDisplayName = "Cloudflare Tunnel"
$serviceDescription = "Tunnel Cloudflare para talkfoodsoftwerk.net"
$exePath = "C:\Program Files\Cloudflare\cloudflared.exe"
$configPath = "C:\Users\Rafael Machado\Downloads\Backend\cloudflare-config.yml"

New-Service -Name $serviceName `
    -DisplayName $serviceDisplayName `
    -Description $serviceDescription `
    -BinaryPathName "$exePath tunnel --config $configPath run" `
    -StartupType Automatic
```

## 📋 Verificação Rápida

Para verificar se tudo está rodando:

```powershell
# Ver processos
Get-Process -Name "dotnet","cloudflared" -ErrorAction SilentlyContinue

# Testar API local
Invoke-WebRequest -Uri "http://localhost:8080/api/health"

# Testar API via domínio
Invoke-WebRequest -Uri "https://www.talkfoodsoftwerk.net/api/health"
```

## ⚠️ Problemas Comuns

### Erro 404 Intermitente

Se às vezes funciona e às vezes não:
1. Verifique se ambos os processos estão rodando
2. Verifique os logs do tunnel para erros
3. Reinicie ambos os processos

### Tunnel Desconecta

Se o tunnel desconectar:
1. Verifique a conexão com a internet
2. Verifique se as credenciais ainda são válidas
3. Reinicie o tunnel

### Aplicação Para

Se a aplicação parar:
1. Verifique os logs para erros
2. Verifique se o PostgreSQL está rodando
3. Verifique a connection string

## 🔧 Comandos Úteis

```powershell
# Parar tudo
Stop-Process -Name "dotnet","cloudflared" -ErrorAction SilentlyContinue

# Reiniciar tudo
# Terminal 1:
cloudflared tunnel --config cloudflare-config.yml run

# Terminal 2:
dotnet run --project Backend.Api

# Ver logs do tunnel
cloudflared tunnel --config cloudflare-config.yml run --loglevel debug
```

## 📝 Notas Importantes

1. **Ambos os processos precisam estar rodando** - Se um parar, o domínio para de funcionar
2. **Mantenha os terminais abertos** - Fechar os terminais para os processos
3. **Para produção**, considere usar serviços do Windows ou um gerenciador de processos como PM2
4. **Monitore os logs** - Eles indicam problemas antes que afetem os usuários

## 🎯 Próximos Passos

Agora que está funcionando:
1. ✅ Configure o DNS no Cloudflare (já feito)
2. ✅ Teste todos os endpoints da API
3. ✅ Configure variáveis de ambiente para produção
4. ✅ Configure backup do banco de dados
5. ✅ Configure monitoramento
