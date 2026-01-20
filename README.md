# Backend - Sistema de Restaurante

Sistema backend refatorado completamente em C# com ASP.NET Core, seguindo padrões de Clean Architecture, Strategy Pattern, DRY e outras boas práticas.

## 🏗️ Arquitetura

O projeto está organizado em camadas seguindo os princípios de Clean Architecture:

- **Backend.Domain**: Entidades do domínio, enums e interfaces base
- **Backend.Application**: Lógica de negócio, DTOs, serviços e interfaces de aplicação
- **Backend.Infrastructure**: Implementações de repositórios, Entity Framework Core, estratégias de tunnel
- **Backend.Api**: Controllers, middlewares e configuração da API

## 🎯 Padrões Implementados

### Strategy Pattern
Implementado para conexões de tunnel (Cloudflare e Local):
- `ITunnelConnectionStrategy`: Interface base
- `CloudflareTunnelStrategy`: Implementação para Cloudflare Tunnel
- `LocalTunnelStrategy`: Implementação para modo local
- `TunnelConnectionFactory`: Factory para criar estratégias

### Repository Pattern
Repositórios genéricos e específicos:
- `IRepository<T>`: Interface genérica base
- `BaseRepository<T>`: Implementação base com operações CRUD
- Repositórios específicos: `UserRepository`, `OrderRepository`, `TableRepository`, etc.

### Dependency Injection
Todos os serviços e repositórios são injetados via DI container do ASP.NET Core.

### DRY (Don't Repeat Yourself)
- BaseRepository para evitar duplicação de código
- DTOs reutilizáveis
- Middleware global para tratamento de erros

## 🚀 Configuração

### Porta Alternativa
O servidor está configurado para rodar na porta **8080** (diferente do padrão 3000/5000).

### Cloudflare Tunnel
O sistema suporta conexão automática com Cloudflare Tunnel:

1. Configure no `appsettings.json`:
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

2. Certifique-se de ter o `cloudflared` instalado e configurado.

### Database
Configure a connection string no `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=talkfood;Username=postgres;Password=postgres"
  }
}
```

### JWT
Configure as chaves JWT no `appsettings.json`:
```json
{
  "Jwt": {
    "SecretKey": "your-super-secret-key-change-in-production-minimum-32-characters",
    "Issuer": "Backend",
    "Audience": "Backend",
    "ExpirationMinutes": "1440"
  }
}
```

## 📦 Dependências Principais

- **ASP.NET Core 8.0**: Framework web
- **Entity Framework Core 8.0**: ORM
- **Npgsql.EntityFrameworkCore.PostgreSQL**: Provider PostgreSQL
- **BCrypt.Net-Next**: Hash de senhas
- **System.IdentityModel.Tokens.Jwt**: Autenticação JWT
- **FluentValidation**: Validação (opcional)
- **AutoMapper**: Mapeamento de objetos (opcional)

## 🔧 Executando o Projeto

1. Restaure as dependências:
```bash
dotnet restore
```

2. Aplique as migrations:
```bash
dotnet ef database update --project Backend.Infrastructure --startup-project Backend.Api
```

3. Execute o projeto:
```bash
dotnet run --project Backend.Api
```

O servidor estará disponível em `http://localhost:8080`

## 📝 Endpoints Principais

### Autenticação
- `POST /api/auth/session` - Autenticar usuário
- `POST /api/auth/users` - Criar usuário
- `GET /api/auth/me` - Obter usuário atual (autenticado)

### Categorias
- `GET /api/category` - Listar categorias (autenticado)
- `GET /api/category/public` - Listar categorias (público)
- `POST /api/category` - Criar categoria (Admin)

### Produtos
- `GET /api/product` - Listar produtos (autenticado)
- `GET /api/product/public` - Listar produtos (público)
- `POST /api/product` - Criar produto (Admin)
- `PUT /api/product` - Atualizar produto (Admin)
- `DELETE /api/product/{id}` - Deletar produto (Admin)

### Pedidos
- `POST /api/order` - Criar pedido (autenticado)
- `POST /api/order/public` - Criar pedido (público)
- `GET /api/order` - Listar pedidos (autenticado)
- `GET /api/order/public?table={table}&phone={phone}` - Buscar pedidos por mesa (público)
- `PUT /api/order/{id}/send` - Enviar pedido
- `PUT /api/order/{id}/finish` - Finalizar pedido

### Mesas
- `GET /api/table` - Listar mesas (autenticado)
- `GET /api/table/qr/{qrCode}` - Buscar mesa por QR Code (público)
- `POST /api/table` - Criar mesa (Admin)

### Caixa
- `GET /api/cashier/status` - Status do caixa (autenticado)
- `POST /api/cashier/open` - Abrir caixa (autenticado)
- `POST /api/cashier/close` - Fechar caixa (autenticado)

## 🔐 Autenticação

O sistema usa JWT Bearer tokens. Para autenticar:

1. Faça POST em `/api/auth/session` com email e senha
2. Use o token retornado no header: `Authorization: Bearer {token}`

## 🎨 Melhorias Implementadas

1. **Clean Architecture**: Separação clara de responsabilidades
2. **Strategy Pattern**: Flexibilidade para diferentes tipos de tunnel
3. **Repository Pattern**: Abstração da camada de dados
4. **Dependency Injection**: Baixo acoplamento e alta testabilidade
5. **Porta Alternativa**: 8080 ao invés de padrões comuns
6. **Middleware Global**: Tratamento centralizado de erros
7. **DTOs**: Separação entre entidades de domínio e modelos de API
8. **Async/Await**: Operações assíncronas em toda a aplicação

## 📄 Licença

Este projeto foi refatorado seguindo as melhores práticas de desenvolvimento em C#.
