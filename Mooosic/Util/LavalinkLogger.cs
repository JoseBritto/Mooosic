using Lavalink4NET.Logging;
using Serilog.Events;

namespace Mooosic.Util;

public class LavalinkLogger : ILogger
{
    private Serilog.ILogger _logger;

    public LavalinkLogger()
    {
        _logger = Serilog.Log.Logger.ForContext("SourceContext", "Lavalink");
    }
    
    public void Log(object source, string message, LogLevel level = LogLevel.Information, Exception? exception = null)
    {
        if (message == null)
            message = exception?.ToString();
        
        if(message == null)
            _logger.Error("Lavalink4Net sent a null log!");
        
        switch (level)
        {
            case LogLevel.Debug:
                // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                _logger.Write(LogEventLevel.Debug, exception, message); 
                break;
            case LogLevel.Information:
                // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                _logger.Write(LogEventLevel.Information, exception, message); 
                break;
            case LogLevel.Warning:
                // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                _logger.Write(LogEventLevel.Warning, exception, message); 
                break;
            case LogLevel.Error:
                // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                _logger.Write(LogEventLevel.Error, exception, message); 
                break;
            case LogLevel.Trace:
                // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                _logger.Write(LogEventLevel.Verbose, exception, message); 
                break;
            
            
        }
        
    }
}