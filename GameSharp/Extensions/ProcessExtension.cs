using GameSharp.Memory.Module;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace GameSharp.Extensions
{
    public static class ProcessExtension
    {
        internal static Module GetProcessModule(this Process process, string moduleName)
        {
            int retryCount = 5;
            Module module = null;
            do
            {
                // We do a refresh in case something has changed in the process, for example a DLL has been injected.
                process.Refresh();

                module = new Module(process.Modules.Cast<ProcessModule>().SingleOrDefault(m => string.Equals(m.ModuleName, moduleName, StringComparison.OrdinalIgnoreCase)));

                if (module != null)
                    break;

                Thread.Sleep(1000);
            } while (retryCount-- > 0);

            return module;
        }
    }
}