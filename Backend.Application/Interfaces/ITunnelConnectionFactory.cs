namespace Backend.Application.Interfaces;

public interface ITunnelConnectionFactory
{
    ITunnelConnectionStrategy CreateStrategy(string strategyType);
    IEnumerable<string> GetAvailableStrategies();
}
