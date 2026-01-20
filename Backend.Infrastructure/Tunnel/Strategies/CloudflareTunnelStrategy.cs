using System.Diagnostics;
using Backend.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Backend.Infrastructure.Tunnel.Strategies;

public class CloudflareTunnelStrategy : ITunnelConnectionStrategy
{
    private Process? _tunnelProcess;
    private readonly string _tunnelName;
    private readonly string? _configFile;
    private readonly string? _domain;
    private readonly ILogger<CloudflareTunnelStrategy>? _logger;
    private string? _tunnelUrl;

    public CloudflareTunnelStrategy(string tunnelName, string? configFile = null, string? domain = null, ILogger<CloudflareTunnelStrategy>? logger = null)
    {
        _tunnelName = tunnelName;
        _configFile = configFile;
        _domain = domain;
        _logger = logger;
    }

    public bool IsConnected => _tunnelProcess != null && !_tunnelProcess.HasExited;

    public string TunnelUrl => _tunnelUrl ?? (!string.IsNullOrEmpty(_domain) ? $"https://{_domain}" : string.Empty);

    public string StrategyName => "Cloudflare";

    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (IsConnected)
            {
                _logger?.LogInformation("Tunnel Cloudflare já está conectado");
                return true;
            }

            _logger?.LogInformation($"Iniciando tunnel Cloudflare: {_tunnelName}");

            string arguments;
            if (!string.IsNullOrEmpty(_configFile) && File.Exists(_configFile))
            {
                arguments = $"tunnel --config {_configFile} run";
                _logger?.LogInformation($"Usando arquivo de configuração: {_configFile}");
            }
            else
            {
                arguments = $"tunnel run {_tunnelName}";
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "cloudflared",
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Directory.GetCurrentDirectory()
            };

            _tunnelProcess = Process.Start(startInfo);
            
            if (_tunnelProcess == null)
            {
                _logger?.LogError("Falha ao iniciar processo do Cloudflare Tunnel");
                return false;
            }

            // Aguardar um pouco para o tunnel iniciar
            await Task.Delay(2000, cancellationToken);

            // Se tiver domínio configurado, usar diretamente
            if (!string.IsNullOrEmpty(_domain))
            {
                _tunnelUrl = $"https://{_domain}";
                _logger?.LogInformation($"Tunnel configurado para domínio: {_tunnelUrl}");
            }

            // Ler a URL do tunnel da saída padrão (para tunnels temporários)
            _tunnelProcess.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    if (e.Data.Contains("https://"))
                    {
                        var urlMatch = System.Text.RegularExpressions.Regex.Match(e.Data, @"https://[\w\-\.]+\.trycloudflare\.com");
                        if (urlMatch.Success && string.IsNullOrEmpty(_tunnelUrl))
                        {
                            _tunnelUrl = urlMatch.Value;
                            _logger?.LogInformation($"Tunnel URL detectada: {_tunnelUrl}");
                        }
                    }
                    
                    // Log de status do tunnel
                    if (e.Data.Contains("Connection") || e.Data.Contains("Registered"))
                    {
                        _logger?.LogInformation($"Cloudflare Tunnel: {e.Data}");
                    }
                }
            };

            _tunnelProcess.BeginOutputReadLine();
            _tunnelProcess.BeginErrorReadLine();

            _logger?.LogInformation("Tunnel Cloudflare conectado com sucesso");
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Erro ao conectar tunnel Cloudflare");
            return false;
        }
    }

    public async Task<bool> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_tunnelProcess == null || _tunnelProcess.HasExited)
            {
                return true;
            }

            _logger?.LogInformation("Desconectando tunnel Cloudflare");

            _tunnelProcess.Kill();
            await _tunnelProcess.WaitForExitAsync(cancellationToken);
            _tunnelProcess.Dispose();
            _tunnelProcess = null;
            _tunnelUrl = null;

            _logger?.LogInformation("Tunnel Cloudflare desconectado");
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Erro ao desconectar tunnel Cloudflare");
            return false;
        }
    }
}
