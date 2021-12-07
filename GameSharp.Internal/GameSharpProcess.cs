﻿using GameSharp.Core;
using GameSharp.Core.Memory;
using GameSharp.Core.Module;
using GameSharp.Core.Native.Enums;
using GameSharp.Core.Native.PInvoke;
using GameSharp.Core.Native.Structs;
using GameSharp.Core.Services;
using GameSharp.Internal.Memory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace GameSharp.Internal
{
    public sealed class GameSharpProcess : IProcess
    {
        public static GameSharpProcess Instance { get; } = new GameSharpProcess();
        public Dictionary<string, ModulePointer> Modules => RefreshModules();
        public Process Native { get; }
        public IntPtr NativeHandle { get; }
        public ProcessModule MainModule { get; }
        public bool Is64Bit { get; }
        public PEB PEB { get; }
        private GameSharpProcess()
        {
            ExceptionService.Initialize();

            Native = Process.GetCurrentProcess();
            NativeHandle = Native.Handle;
            MainModule = Native.MainModule;
            Is64Bit = IntPtr.Size == 8;
            PEB = new PEB(this);
        }

        public MemoryPointer GetPebAddress()
        {
            ProcessBasicInformation pbi = new ProcessBasicInformation();

            Ntdll.NtQueryInformationProcess(NativeHandle, ProcessInformationClass.ProcessBasicInformation, ref pbi, Marshal.SizeOf(pbi), out int _);

            return new InternalMemoryPointer(pbi.PebBaseAddress);
        }

        public ModulePointer LoadLibrary(string libraryPath, bool resolveReferences)
        {
            if (!File.Exists(libraryPath))
            {
                throw new FileNotFoundException(libraryPath);
            }

            IntPtr libraryAddress = resolveReferences
                ? Kernel32.LoadLibrary(libraryPath)
                : Kernel32.LoadLibraryExW(libraryPath, IntPtr.Zero, LoadLibraryFlags.DontResolveDllReferences);

            if (libraryAddress == IntPtr.Zero)
            {
                throw new Win32Exception($"Couldn't load the library {libraryPath}.");
            }

            return Modules[Path.GetFileName(libraryPath.ToLower())];
        }

        public MemoryPointer AllocateManagedMemory(int size)
        {
            return new InternalMemoryPointer(Marshal.AllocHGlobal(size));
        }

        public Dictionary<string, ModulePointer> RefreshModules()
        {
            Native.Refresh();

            Dictionary<string, ModulePointer> modules = new Dictionary<string, ModulePointer>();

            foreach (ProcessModule processModule in Native.Modules)
            {
                if (!modules.ContainsKey(processModule.ModuleName.ToLower()))
                {
                    modules.Add(processModule.ModuleName.ToLower(), new Module.InternalModulePointer(processModule));
                }
            }

            return modules;
        }
    }
}
