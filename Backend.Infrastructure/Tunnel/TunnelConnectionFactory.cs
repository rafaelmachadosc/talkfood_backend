using Backend.Application.Interfaces;
using Backend.Infrastructure.Tunnel.Strategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Backend.Infrastructure.Tunnel;

public class TunnelConnectionFactory : ITunnelConnectionFactory
{
    private readonly IConfiguration _configuration;
    private readonly ILoggerFactory _loggerFactory;

    public TunnelConnectionFactory(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        _configuration = configuration;
        _loggerFactory = loggerFactory;
    }

    public ITunnelConnectionStrategy CreateStrategy(string strategyType)
    {
        var logger = _loggerFactory.CreateLogger<ITunnelConnectionStrategy>();

        return strategyType.ToLowerInvariant() switch
        {
            "cloudflare" => new CloudflareTunnelStrategy(
                _configuration["Tunnel:Cloudflare:Name"] ?? "talkfood-app",
                _configuration["Tunnel:Cloudflare:ConfigFile"],
                _configuration["Tunnel:Cloudflare:Domain"],
                _loggerFactory.CreateLogger<CloudflareTunnelStrategy>()
            ),
            "local" => new LocalTunnelStrategy(
                int.Parse(_configuration["Server:Port"] ?? "8081"),
                _loggerFactory.CreateLogger<LocalTunnelStrategy>()
            ),
            _ => throw new ArgumentException($"Estratégia de tunnel desconhecida: {strategyType}", nameof(strategyType))
        };
    }

    public IEnumerable<string> GetAvailableStrategies()
    {
        return new[] { "cloudflare", "local" };
    }
}
