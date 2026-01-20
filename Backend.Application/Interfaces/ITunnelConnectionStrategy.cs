namespace Backend.Application.Interfaces;

public interface ITunnelConnectionStrategy
{
    Task<bool> ConnectAsync(CancellationToken cancellationToken = default);
    Task<bool> DisconnectAsync(CancellationToken cancellationToken = default);
    bool IsConnected { get; }
    string TunnelUrl { get; }
    string StrategyName { get; }
}
