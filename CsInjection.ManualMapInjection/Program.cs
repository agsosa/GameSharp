﻿using System.Diagnostics;
using System.IO;
using System.Linq;
using CsInjection.ManualMapInjection.Injection;

namespace CsInjection.ManualMapInjection
{
    class Program
    {
        static void Main(string[] args)
        {
            //Process targetProcess = Process.GetProcessesByName("notepad++").FirstOrDefault();
            Process targetProcess = Process.GetProcessesByName("League of legends").FirstOrDefault();
            ManualMapInjector injector = new ManualMapInjector(targetProcess);
            FileInfo fileInfo = new FileInfo(@"CsInjection.Cpp.Bootstrap.dll");
            injector.Inject(fileInfo.FullName);
        }
    }
}