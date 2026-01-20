using Backend.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Backend.Infrastructure.Tunnel.Strategies;

public class LocalTunnelStrategy : ITunnelConnectionStrategy
{
    private readonly int _port;
    private readonly ILogger<LocalTunnelStrategy>? _logger;

    public LocalTunnelStrategy(int port, ILogger<LocalTunnelStrategy>? logger = null)
    {
        _port = port;
        _logger = logger;
    }

    public bool IsConnected => true; // Sempre conectado localmente

    public string TunnelUrl => $"http://localhost:{_port}";

    public string StrategyName => "Local";

    public Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation($"Modo local ativo na porta {_port}");
        return Task.FromResult(true);
    }

    public Task<bool> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Desconectando modo local");
        return Task.FromResult(true);
    }
}
