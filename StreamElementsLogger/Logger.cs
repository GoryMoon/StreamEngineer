using NLog;
using ILogger = GoryMoon.StreamEngineer.Data.ILogger;

namespace StreamElementsLogger;

public class Logger : ILogger
{
    private readonly NLog.Logger _logger; 
    public Logger()
    {
        LogManager.Setup().LoadConfiguration(builder =>
        {
            builder.ForLogger().WriteToConsole();
            builder.ForLogger().WriteToFile("se.log");
        });
        _logger = LogManager.GetLogger("StreamElementsLogger");
    }

    public void WriteLine(string msg) => _logger.Info(msg);
    public void WriteError(string msg) => _logger.Error(msg);
    public void WriteError(Exception e, string msg = "") => _logger.Error(e, msg);

    public void WriteAndChat(string msg) => WriteLine(msg);
}