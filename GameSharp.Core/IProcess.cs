﻿using GameSharp.Core.Memory;
using GameSharp.Core.Module;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameSharp.Core
{
    public interface IProcess
    {
        bool Is64Bit { get; }
        Process Native { get; }
        ProcessModule MainModule { get; }
        IntPtr NativeHandle { get; }
        Dictionary<string, ModulePointer> Modules { get; }
        ModulePointer LoadLibrary(string pathToDll, bool resolveReferences = true);
        MemoryPointer AllocateManagedMemory(int size);
        Dictionary<string, ModulePointer> RefreshModules();
        MemoryPointer GetPebAddress();
        PEB PEB { get; }
    }
}
