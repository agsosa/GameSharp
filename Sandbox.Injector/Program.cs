﻿using CsInjection.Core.Helpers;
using CsInjection.Injection;
using System;
using System.Diagnostics;
using System.IO;

namespace Sandbox.Injector
{
    class Program
    {
        static readonly string dir = Environment.CurrentDirectory;
        static readonly string exe = Path.Combine(dir, "Sandbox.App.exe");
        static readonly string dll = Path.Combine(dir, "CsInjection.Bootstrapper.dll");

        static void Main(string[] args)
        {
            Process targetProcess = Process.Start(exe);
            ManualMapInjection injector = new ManualMapInjection(targetProcess);

            if (Debugger.IsAttached)
                targetProcess.Attach();

            injector.Inject(dll);
        }
    }
}