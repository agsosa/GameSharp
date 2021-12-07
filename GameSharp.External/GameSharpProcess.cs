﻿using GameSharp.Core;
using GameSharp.Core.Memory;
using GameSharp.Core.Module;
using GameSharp.Core.Native.Enums;
using GameSharp.Core.Native.PInvoke;
using GameSharp.Core.Native.Structs;
using GameSharp.External.Helpers;
using GameSharp.External.Memory;
using GameSharp.External.Module;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace GameSharp.External
{
    public class GameSharpProcess : IProcess
    {
        public Dictionary<string, ModulePointer> Modules => RefreshModules();
        public Process Native { get; }
        public IntPtr NativeHandle { get; }
        public ProcessModule MainModule { get; }
        public bool Is64Bit { get; }
        public PEB PEB { get; }
        public GameSharpProcess(Process process)
        {
            Native = process ?? throw new NullReferenceException("process");
            NativeHandle = Native.Handle;
            MainModule = Native.MainModule;
            Is64Bit = IntPtr.Size == 8;
            PEB = new PEB(this);
        }

        public MemoryPointer GetPebAddress()
        {
            ProcessBasicInformation pbi = new ProcessBasicInformation();

            Ntdll.NtQueryInformationProcess(NativeHandle, ProcessInformationClass.ProcessBasicInformation, ref pbi, pbi.Size, out int _);

            return new ExternalMemoryPointer(this, pbi.PebBaseAddress);
        }

        public ModulePointer LoadLibrary(string pathToDll, bool resolveReferences = true)
        {
            byte[] loadLibraryOpcodes = LoadLibraryHelper.LoadLibraryPayload(pathToDll);

            MemoryPointer allocatedMemory = AllocateManagedMemory(loadLibraryOpcodes.Length);

            if (Kernel32.WriteProcessMemory(Native.Handle, allocatedMemory.Address, loadLibraryOpcodes, loadLibraryOpcodes.Length, out IntPtr _))
            {
                ModulePointer kernel32Module = Modules["kernel32.dll"];
                MemoryPointer loadLibraryAddress;
                if (resolveReferences)
                {
                    loadLibraryAddress = kernel32Module.GetProcAddress("LoadLibraryW");
                }
                else
                {
                    loadLibraryAddress = kernel32Module.GetProcAddress("LoadLibraryExW");
                }

                if (loadLibraryAddress == null)
                {
                    throw new Win32Exception($"Couldn't get proc address, error code: {Marshal.GetLastWin32Error()}.");
                }

                if (Kernel32.CreateRemoteThread(Native.Handle, IntPtr.Zero, 0, loadLibraryAddress.Address, allocatedMemory.Address, 0, IntPtr.Zero) == IntPtr.Zero)
                {
                    throw new Win32Exception($"Couldn't create a remote thread, error code: {Marshal.GetLastWin32Error()}.");
                }
            }

            ModulePointer injectedModule;

            while (!Modules.TryGetValue(Path.GetFileName(pathToDll).ToLower(), out injectedModule))
            {
                Thread.Sleep(1);
            }

            return injectedModule;
        }

        public Dictionary<string, ModulePointer> RefreshModules()
        {
            Native.Refresh();

            Dictionary<string, ModulePointer> modules = new Dictionary<string, ModulePointer>();

            foreach (ProcessModule processModule in Native.Modules)
            {
                Console.WriteLine(processModule.ModuleName);
                if (!modules.ContainsKey(processModule.ModuleName.ToLower()))
                {
                    modules.Add(processModule.ModuleName.ToLower(), new ExternalModulePointer(this, processModule));
                }
            }

            return modules;
        }

        public void AttachDebugger()
        {
            DebugHelper.SafeAttach(this);
        }

        public MemoryPointer AllocateManagedMemory(int size)
        {
            return new ExternalMemoryPointer(this, Kernel32.VirtualAllocEx(Native.Handle, IntPtr.Zero, (uint)size, AllocationType.Reserve | AllocationType.Commit, MemoryProtection.ExecuteReadWrite));
        }

        public void SuspendThreads(bool suspend = true)
        {
            foreach (ProcessThread pT in Native.Threads)
            {
                IntPtr tHandle = Kernel32.OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (tHandle != IntPtr.Zero)
                {
                    if (suspend)
                    {
                        Kernel32.SuspendThread(tHandle);
                    }
                    else
                    {
                        Kernel32.ResumeThread(tHandle);
                    }

                    // Close the handle; https://docs.microsoft.com/nl-nl/windows/desktop/api/processthreadsapi/nf-processthreadsapi-openthread
                    Kernel32.CloseHandle(tHandle);
                }
                else
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(), $"Cannot open a thread handle to {pT.Id}");
                }
            }
        }

        public void AllocConsole()
        {
            IntPtr kernel32Module = Kernel32.GetModuleHandle("kernel32.dll");
            IntPtr allocConsoleAddress = Kernel32.GetProcAddress(kernel32Module, "AllocConsole");
            Kernel32.CreateRemoteThread(Native.Handle, IntPtr.Zero, 0, allocConsoleAddress, IntPtr.Zero, 0, IntPtr.Zero);
        }
    }
}
