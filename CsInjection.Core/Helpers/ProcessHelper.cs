using System;
using System.Diagnostics;

namespace CsInjection.Core.Helpers
{
    public static class ProcessHelper
    {
        public static Process GetCurrentProcess = Process.GetCurrentProcess();
        public static IntPtr GetMainModuleBaseAddress = Process.GetCurrentProcess().MainModule.BaseAddress;
    }
}