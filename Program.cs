using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace watch
{
    class Program
    {
        static TimeSpan Delay = new TimeSpan(0,0,4);

        static void Main(string[] args)
        {
            var argIndex = 0;
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: ");

                var assembly = typeof(Program).Assembly.Location;
                var thisExe = Path.GetFileNameWithoutExtension(assembly);
                Console.WriteLine($"  {thisExe} [-d delaySecs] <executable> [arguments]");
                return;
            }
            if (args.Length > 2 && (args[argIndex] == "-d" || args[argIndex] == "/d"))
            {
                Delay = new TimeSpan(0, 0, int.Parse(args[argIndex + 1]));
                argIndex += 2;
            }
            var exe = args[argIndex];
            var exeArgs = args.Skip(argIndex + 1).ToArray();

            var linesUsed = RunApp(exe, exeArgs, 0);

            while (true)
            {
                var top = Console.CursorTop;

                var idealStart = top - linesUsed;
                Console.CursorTop = Math.Max(0, idealStart);

                var toSuppress = Math.Max(0, -idealStart);
                linesUsed = RunApp(exe, exeArgs, toSuppress);

                Thread.Sleep(Delay);
            }
        }

        static int RunApp(string app, IEnumerable<string> args, int outOffset)
        {
            var startInfo = new ProcessStartInfo(app, string.Join(" ", args));
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;

            var process = new Process();
            process.StartInfo = startInfo;
            process.Start();

            process.WaitForExit();

            using (var outputReader = process.StandardOutput)
            {
                var text = outputReader.ReadToEnd();
                var lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines.Skip(outOffset))
                {
                    Console.WriteLine(line);
                }
                return lines.Length;
            }
        }
    }
}
