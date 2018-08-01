using System;
using Shigure;

namespace ShigureRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new Client();
            client.Run(args);
        }
    }
}
