# Guia de Migrations

## Criar uma nova Migration

```bash
dotnet ef migrations add NomeDaMigration --project Backend.Infrastructure --startup-project Backend.Api
```

## Aplicar Migrations

```bash
dotnet ef database update --project Backend.Infrastructure --startup-project Backend.Api
```

## Reverter Migration

```bash
dotnet ef database update NomeDaMigrationAnterior --project Backend.Infrastructure --startup-project Backend.Api
```

## Remover última Migration (antes de aplicar)

```bash
dotnet ef migrations remove --project Backend.Infrastructure --startup-project Backend.Api
```

## Gerar Script SQL

```bash
dotnet ef migrations script --project Backend.Infrastructure --startup-project Backend.Api --output migration.sql
```

## Instalar EF Core Tools (se necessário)

```bash
dotnet tool install --global dotnet-ef
```
