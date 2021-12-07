﻿using GameSharp.Core.Memory;
using GameSharp.Core.Native.Enums;
using GameSharp.Internal;
using GameSharp.Internal.Extensions;
using GameSharp.Internal.Memory;
using System;
using System.Runtime.InteropServices;

namespace GameSharp.Notepadpp.FunctionWrapper
{
    public class InjectedNtQueryInformationProcess : SafeFunction
    {
        private static readonly MemoryPointer Allocation = GameSharpProcess.Instance.AllocateManagedMemory(100);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint InjectedNtQueryInformationProcessDelegate(IntPtr processHandle, int processInformationClass, [Out] IntPtr processInformation,
            uint processInformationLength, [Out] IntPtr returnLength);

        protected override Delegate InitializeDelegate()
        {
            Allocation.Write(new byte[]
            {
                // Writing the shellcode, because if we just copy the current function then 
                // we can also copy a ret if that has been put at the start of the function.
                0x4C, 0x8B, 0xD1, 0xB8, 0x19, 0x00, 0x00, 0x00, 0xF6, 0x04, 0x25, 0x08, 
                0x03, 0xFE, 0x7F, 0x01, 0x75, 0x03, 0x0F, 0x05, 0xC3, 0xCD, 0x2E, 0xC3
            });

            return Allocation.ToDelegate<InjectedNtQueryInformationProcessDelegate>();
        }

        public uint Call(IntPtr handle, ProcessInformationClass pic, out MemoryPointer result, int resultLength, out MemoryPointer bytesRead)
        {
            MemoryPointer bytesReadInternal = GameSharpProcess.Instance.AllocateManagedMemory(resultLength);
            MemoryPointer resultInternal = GameSharpProcess.Instance.AllocateManagedMemory(resultLength);

            uint retval = Call<uint>(handle, pic, resultInternal.Address, (uint)resultLength, bytesReadInternal.Address);

            bytesRead = bytesReadInternal;
            result = resultInternal;

            return retval;
        }
    }
}
