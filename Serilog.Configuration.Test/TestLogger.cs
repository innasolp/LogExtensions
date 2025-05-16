using Microsoft.Extensions.Logging;

namespace Serilog.Configuration.Test;

public class TestLogger<T>(ILogger<TestLogger<T>> logger)
{
    private readonly ILogger<TestLogger<T>> _logger = logger;

    public void LogInfo(string message)
    {
        _logger.LogInformation(message);
    }

    public void LogWarning(string message)
    {
        _logger.LogWarning(message);
    }

    public void LogError(Exception exception)
    {
        _logger.LogError(exception, exception.Message);
    }
}
