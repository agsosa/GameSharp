﻿using GameSharp.Core.Memory;
using GameSharp.Core.Module;
using GameSharp.Core.Native.PInvoke;
using GameSharp.External.Memory;
using System;
using System.Diagnostics;

namespace GameSharp.External.Module
{
    public class ExternalModulePointer : ModulePointer
    {
        public override MemoryPointer MemoryPointer { get; }

        public GameSharpProcess GameSharpProcess { get; }

        public ExternalModulePointer(GameSharpProcess process, ProcessModule processModule) : base(processModule)
        {
            GameSharpProcess = process;
            MemoryPointer = new ExternalMemoryPointer(GameSharpProcess, processModule.BaseAddress);
        }

        public override MemoryPointer GetProcAddress(string name)
        {
            MemoryPointer address = new ExternalMemoryPointer(GameSharpProcess, Kernel32.GetProcAddress(ProcessModule.BaseAddress, name));
            if (address == null)
            {
                throw new NullReferenceException($"Couldn't find function {name} in module {ProcessModule.ModuleName}");
            }
            return address;
        }
    }
}
