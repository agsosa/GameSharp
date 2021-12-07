using GameSharp.External;
using GameSharp.External.Injection;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GameSharp.Notepadpp.Injector
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Finding processes");
            foreach (var process in Process.GetProcessesByName("archeage"))
            {
                Console.WriteLine("Process" + process);
            }
            Console.WriteLine("Finding processes done");
            Process notepadpp = Process.GetProcessesByName("archeage").FirstOrDefault();

            Console.WriteLine(notepadpp);

           /* if (notepadpp == null)
            {
                Console.WriteLine("Trying to open the process!");
                // The process we are injecting into.
                notepadpp = Process.Start("notepad.exe");
                notepadpp.WaitForInputIdle();
            }*/

            GameSharpProcess gameSharp = new GameSharpProcess(notepadpp);

            if (gameSharp == null)
            {
                throw new Exception("Process not found.");
            }

            string pathToDll = Path.Combine(Environment.CurrentDirectory, "GameSharp.Notepadpp.dll");

            // My remote thread injector, you can replace this with any injector.
            IInjection injector = new RemoteThreadInjection(gameSharp);
            injector.InjectAndExecute(new Injectable(pathToDll, "Main"), attach: true, launchConsole: true);
        }
    }
}
