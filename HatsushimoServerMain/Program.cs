using System;
using Hatsushimo;
using HatsushimoServer;
using SharpRaven;
using SharpRaven.Data;
using NLog.Config;
using NLog;
using NLog.Targets;

namespace HatsushimoServerMain
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

        static readonly Logger log = LogManager.GetLogger("Hatsushimo");

        static void Main(string[] args)
        {
            ConfigureLogger();

            // log.Trace("trace log message");
            // log.Debug("debug log message");
            // log.Info("info log message");
            // log.Warn("warn log message");
            // log.Error("error log message");
            // log.Fatal("fatal log message");

            var svr = new Server();
            svr.Run(args);
        }
    }
}
