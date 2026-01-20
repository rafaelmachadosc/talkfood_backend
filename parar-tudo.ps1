# Script para parar todos os processos usando a porta 8080

Write-Host "=== Parando processos na porta 8080 ===" -ForegroundColor Cyan

# Parar todos os processos dotnet
Write-Host "`n[1/2] Parando processos dotnet..." -ForegroundColor Yellow
Get-Process -Name "dotnet","Backend.Api" -ErrorAction SilentlyContinue | ForEach-Object {
    try {
        Stop-Process -Id $_.Id -Force
        Write-Host "  Processo $($_.Id) parado" -ForegroundColor Green
    } catch {
        Write-Host "  Erro ao parar processo $($_.Id)" -ForegroundColor Red
    }
}

Start-Sleep -Seconds 2

# Verificar porta
Write-Host "`n[2/2] Verificando porta 8080..." -ForegroundColor Yellow
$listening = netstat -ano | findstr ":8080.*LISTENING"

if ($listening) {
    Write-Host "AVISO: Porta ainda em uso!" -ForegroundColor Red
    Write-Host "`nProcessos encontrados:" -ForegroundColor Yellow
    $listening | ForEach-Object {
        if ($_ -match '\s+(\d+)$') {
            $processId = $matches[1]
            $proc = Get-Process -Id $processId -ErrorAction SilentlyContinue
            if ($proc) {
                Write-Host "  PID: $processId - $($proc.ProcessName)" -ForegroundColor White
                Write-Host "    Tente parar manualmente: taskkill /F /PID $processId" -ForegroundColor Yellow
            }
        }
    }
} else {
    Write-Host "✓ Porta 8080 está livre!" -ForegroundColor Green
    Write-Host "`nAgora você pode iniciar a aplicação:" -ForegroundColor Cyan
    Write-Host "  dotnet run --project Backend.Api" -ForegroundColor White
}
