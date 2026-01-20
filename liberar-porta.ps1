# Script para liberar a porta 8080

Write-Host "=== Liberando Porta 8080 ===" -ForegroundColor Cyan

# Encontrar processos usando a porta 8080
Write-Host "`n[1/3] Procurando processos na porta 8080..." -ForegroundColor Yellow
$connections = netstat -ano | findstr ":8080" | findstr "LISTENING"

if ($connections) {
    $pids = @()
    foreach ($line in $connections) {
        if ($line -match '\s+(\d+)$') {
            $pid = $matches[1]
            $pids += $pid
        }
    }
    
    $uniquePids = $pids | Select-Object -Unique
    
    if ($uniquePids) {
        Write-Host "Processos encontrados:" -ForegroundColor Yellow
        foreach ($pid in $uniquePids) {
            $process = Get-Process -Id $pid -ErrorAction SilentlyContinue
            if ($process) {
                Write-Host "  PID: $pid - $($process.ProcessName) - $($process.Path)" -ForegroundColor White
            } else {
                Write-Host "  PID: $pid - (processo não encontrado)" -ForegroundColor Gray
            }
        }
        
        Write-Host "`n[2/3] Parando processos..." -ForegroundColor Yellow
        foreach ($pid in $uniquePids) {
            try {
                Stop-Process -Id $pid -Force -ErrorAction Stop
                Write-Host "  ✓ Processo $pid parado" -ForegroundColor Green
            } catch {
                Write-Host "  ✗ Erro ao parar processo $pid : $_" -ForegroundColor Red
            }
        }
        
        Start-Sleep -Seconds 2
        
        Write-Host "`n[3/3] Verificando..." -ForegroundColor Yellow
        $stillInUse = netstat -ano | findstr ":8080" | findstr "LISTENING"
        if ($stillInUse) {
            Write-Host "⚠ A porta ainda está em uso. Pode ser necessário reiniciar." -ForegroundColor Yellow
        } else {
            Write-Host "✓ Porta 8080 liberada!" -ForegroundColor Green
        }
    } else {
        Write-Host "Nenhum processo encontrado na porta 8080" -ForegroundColor Green
    }
} else {
    Write-Host "✓ Porta 8080 está livre" -ForegroundColor Green
}

Write-Host "`nAgora você pode iniciar a aplicação:" -ForegroundColor Cyan
Write-Host "  dotnet run --project Backend.Api" -ForegroundColor White
