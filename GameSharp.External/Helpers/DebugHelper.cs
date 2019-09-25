﻿using GameSharp.Core.Memory;
using GameSharp.Core.Module;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameSharp.External.Helpers
{
    class DebugHelper
    {
        private List<MemoryPatch> MemoryPatches = new List<MemoryPatch>();

        readonly GameSharpProcess Process;

        private DebugHelper(GameSharpProcess process)
        {
            Process = process;
        }

        public static void SafeAttach(GameSharpProcess process)
        {
            DebugHelper debugHelper = new DebugHelper(process);

            debugHelper.ValidateDbgBreakPoint();
            debugHelper.AttachManagedDebugger();
            debugHelper.HideFromPEB();
            debugHelper.DisposeOfPatches();
        }

        private void HideFromPEB()
        {
            //throw new NotImplementedException();
        }

        private void DisposeOfPatches()
        {
            foreach (var p in MemoryPatches)
            {
                p.Dispose();
            }
        }

        private void ValidateDbgBreakPoint()
        {
            IMemoryModule ntdll = Process.Modules["ntdll.dll"];

            IMemoryAddress dbgBreakPointPtr = ntdll.GetProcAddress("DbgBreakPoint");

            byte dbgBreakPointByte = dbgBreakPointPtr.Read<byte>();

            if (dbgBreakPointByte == 0xC3)
                MemoryPatches.Add(new MemoryPatch(dbgBreakPointPtr, new byte[] { 0xCC }));
        }

        private void AttachManagedDebugger()
        {
            EnvDTE.DTE dte;
            try
            {
                dte = (EnvDTE.DTE)Marshal.GetActiveObject("VisualStudio.DTE.16.0");
            }
            catch (COMException)
            {
                throw new Exception("Visual studio v2019 not found.");
            }

            int tryCount = 5;

            do
            {
                Process.NativeProcess.WaitForInputIdle();

                try
                {
                    EnvDTE.Processes processes = dte.Debugger.LocalProcesses;
                    foreach (EnvDTE80.Process2 proc in processes.Cast<EnvDTE80.Process2>().Where(proc => proc.Name.IndexOf(Process.NativeProcess.ProcessName) != -1))
                    {
                        EnvDTE80.Engine debugEngine = proc.Transport.Engines.Item("Managed (v4.6, v4.5, v4.0)");
                        proc.Attach2(debugEngine);
                        break;
                    }
                    break;
                }
                catch { }

                Thread.Sleep(1000);
            } while (tryCount-- > 0);
        }
    }
}
