# Guia de Setup

## Pré-requisitos

1. .NET 8.0 SDK
2. PostgreSQL instalado e rodando
3. (Opcional) Cloudflare Tunnel configurado

## Passos de Instalação

### 1. Restaurar Dependências

```bash
dotnet restore
```

### 2. Configurar Banco de Dados

Edite o arquivo `Backend.Api/appsettings.json` e configure a connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=talkfood;Username=postgres;Password=postgres"
  }
}
```

### 3. Criar e Aplicar Migrations

```bash
dotnet ef migrations add InitialCreate --project Backend.Infrastructure --startup-project Backend.Api
dotnet ef database update --project Backend.Infrastructure --startup-project Backend.Api
```

### 4. Configurar JWT Secret Key

Edite o arquivo `Backend.Api/appsettings.json` e altere a chave secreta:

```json
{
  "Jwt": {
    "SecretKey": "sua-chave-secreta-super-segura-com-pelo-menos-32-caracteres"
  }
}
```

### 5. Executar a Aplicação

```bash
dotnet run --project Backend.Api
```

A aplicação estará disponível em `http://localhost:8080`

### 6. (Opcional) Configurar Cloudflare Tunnel

1. Instale o Cloudflare Tunnel:
   - Windows: Baixe de https://github.com/cloudflare/cloudflared/releases
   - Ou use: `winget install --id Cloudflare.cloudflared`

2. Configure o tunnel:
   ```bash
   cloudflared tunnel login
   cloudflared tunnel create talkfood-app
   ```

3. Configure no `appsettings.json`:
   ```json
   {
     "Tunnel": {
       "Strategy": "cloudflare",
       "Cloudflare": {
         "Name": "talkfood-app"
       }
     }
   }
   ```

## Estrutura de Pastas

```
Backend/
├── Backend.Domain/          # Entidades e enums
├── Backend.Application/      # Lógica de negócio e serviços
├── Backend.Infrastructure/   # Repositórios e EF Core
└── Backend.Api/             # Controllers e API
```

## Variáveis de Ambiente

Você pode usar variáveis de ambiente ao invés de `appsettings.json`:

- `ConnectionStrings__DefaultConnection`
- `Jwt__SecretKey`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Tunnel__Strategy`
- `Tunnel__Cloudflare__Name`

## Testando a API

Após iniciar, acesse:
- Swagger UI: `http://localhost:8080/swagger`
- Health Check: `http://localhost:8080/api/health`
