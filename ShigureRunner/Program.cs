using System;
using NLog;
using NLog.Config;
using NLog.Targets;
using Shigure;

namespace ShigureRunner
{
    class Program
    {
        static void ConfigureLogger()
        {
            // http://kristjansson.us/?p=686
            // Step 1. Create configuration object
            LoggingConfiguration config = new LoggingConfiguration();

            // Step 2. Create targets and add them to the configuration
            ColoredConsoleTarget consoleTarget = new ColoredConsoleTarget();
            config.AddTarget("console", consoleTarget);

            // Step 4. Define rules
            //LoggingRule rule1 = new LoggingRule("*", LogLevel.Debug, consoleTarget);
            LoggingRule rule1 = new LoggingRule("*", LogLevel.Info, consoleTarget);
            config.LoggingRules.Add(rule1);

            // Step 5. Activate the configuration
            LogManager.Configuration = config;
        }

        static void Main(string[] args)
        {
            ConfigureLogger();

            var client = new Client();
            client.Run(args);
        }
    }
}
