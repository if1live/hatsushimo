using System;
using WebSocketSharp;
using Hatsushimo;
using Hatsushimo.Packets;
using Hatsushimo.NetChan;
using System.IO;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Newtonsoft.Json;

namespace Shigure
{
    public class Client
    {
        class Options
        {
            [Option("bot", Required = true, HelpText = "bot mode")]
            public string Mode { get; set; }

            [Option("lifetime", Default = 10, Required = false, HelpText = "lifetime")]
            public int Lifetime { get; set; }
        }

        IThinker CreateThinker(Connection conn, Options opts)
        {
            if (opts.Mode == "assert")
            {
                IThinker thinker = new AssertThinker(conn);
                return thinker;
            }
            else if (opts.Mode == "simple")
            {
                IThinker thinker = new SimpleThinker(conn);
                return thinker;
            }
            else if (opts.Mode == "wander")
            {
                IThinker thinker = new WanderThinker(conn, opts.Lifetime);
                return thinker;
            }
            else
            {
                // default
                Console.WriteLine($"unknwon bot mode: {opts.Mode}");
                return null;
            }
        }


        public void Run(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
            .WithParsed<Options>(opts => RunOptionsAndReturnExitCode(opts))
            .WithNotParsed<Options>((errs) => HandleParseError(errs));

        }

        private void HandleParseError(IEnumerable<Error> errs)
        {
            foreach (var e in errs)
            {
                Console.WriteLine(e.ToString());
            }
        }

        void RunOptionsAndReturnExitCode(Options opts)
        {
            string json = JsonConvert.SerializeObject(opts, Formatting.Indented);
            Console.WriteLine(json);

            using (var ws = new WebSocket($"ws://127.0.0.1:{Config.ServerPort}/game"))
            {
                Console.CancelKeyPress += delegate
                {
                    Console.WriteLine("ctrl-c exit program");
                    ws.Close();
                };

                var conn = new Connection(ws);
                var thinker = CreateThinker(conn, opts);
                if (thinker == null) { return; }

                ws.OnMessage += (sender, e) =>
                {
                    conn.PushReceivedData(e.RawData);
                };
                ws.OnClose += (sender, e) =>
                {
                    Console.WriteLine("on close");
                };
                ws.OnError += (sender, e) =>
                {
                    Console.WriteLine("on error");
                };
                ws.Connect();

                var task = thinker.Run();
                task.Wait();
            }
        }
    }
}
