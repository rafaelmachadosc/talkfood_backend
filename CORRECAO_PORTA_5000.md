# Correção: Backend rodando na porta 5000 ao invés de 8081

## ✅ Problema Identificado

O backend estava rodando na porta **5000** (padrão do ASP.NET Core) ao invés da porta configurada **8081**.

## ✅ Correções Aplicadas

### 1. launchSettings.json Atualizado

**Antes:**
```json
"applicationUrl": "http://localhost:8081"
```

**Depois:**
```json
"environmentVariables": {
  "ASPNETCORE_ENVIRONMENT": "Development",
  "ASPNETCORE_URLS": "http://0.0.0.0:8081"
}
```

**Mudanças:**
- ✅ Removida `applicationUrl` (pode causar conflito)
- ✅ Adicionada variável `ASPNETCORE_URLS` (tem prioridade)
- ✅ Configurada para `0.0.0.0:8081` (igual ao Program.cs)

### 2. Program.cs (já estava correto)
```csharp
builder.WebHost.UseUrls($"http://0.0.0.0:8081");
```

### 3. cloudflare-config.yml (já estava correto)
```yaml
service: http://127.0.0.1:8081
```

## 🚀 Próximos Passos

### 1. Parar Processos na Porta 5000

Se ainda houver processo na porta 5000:

```powershell
# Ver qual processo está usando
netstat -ano | findstr ":5000.*LISTENING"

# Parar o processo (substitua <PID> pelo número)
taskkill /F /PID <PID>
```

### 2. Parar Todos os Processos Dotnet

```powershell
Get-Process -Name "dotnet","Backend.Api" -ErrorAction SilentlyContinue | Stop-Process -Force
```

### 3. Iniciar a Aplicação

```powershell
cd "c:\Users\Rafael Machado\Downloads\Backend"
dotnet run --project Backend.Api
```

### 4. Verificar Porta

```powershell
netstat -ano | findstr ":8081.*LISTENING"
```

Deve mostrar que a porta 8081 está em LISTENING.

### 5. Testar

```powershell
Invoke-WebRequest -Uri "http://127.0.0.1:8081/api/health" -UseBasicParsing
```

## 📝 Por Que Isso Aconteceu?

O `launchSettings.json` tinha `applicationUrl` que pode ser ignorado ou sobrescrito dependendo de como a aplicação é iniciada. A variável de ambiente `ASPNETCORE_URLS` tem **prioridade mais alta** e garante que a porta correta seja usada.

## ✅ Verificação Final

Após reiniciar, verifique:

1. ✅ Porta 8081 em LISTENING
2. ✅ Porta 5000 livre (ou não sendo usada pelo backend)
3. ✅ `http://127.0.0.1:8081/api/health` responde
4. ✅ Tunnel conecta em `http://127.0.0.1:8081`
5. ✅ `https://talkfoodsoftwerk.net/api/health` funciona

## 🔍 Comandos Úteis

```powershell
# Ver todas as portas em uso
netstat -ano | findstr "LISTENING" | findstr ":5000\|:8081"

# Ver processos dotnet
Get-Process -Name "dotnet","Backend.Api" -ErrorAction SilentlyContinue

# Parar tudo
Get-Process -Name "dotnet","Backend.Api" -ErrorAction SilentlyContinue | Stop-Process -Force

# Testar aplicação
Invoke-WebRequest -Uri "http://127.0.0.1:8081/api/health" -UseBasicParsing
```
