using GameSharp.Core.Memory;
using GameSharp.Core.Module;
using GameSharp.Core.Services;
using GameSharp.Internal;
using GameSharp.Internal.Extensions;
using GameSharp.Internal.Memory;
using System;
using System.Runtime.InteropServices;

namespace GameSharp.Notepadpp.Hooks
{
    public class HookExecuteBufferW : Hook
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        private delegate bool HookExecuteBufferWDelegate(Object hWnd, [MarshalAs(UnmanagedType.LPArray)]string sBuffer, UIntPtr nSize, [MarshalAs(UnmanagedType.LPArray)]string sBufferDescription, Object pTable);

        private bool DetourMethod(Object hWnd, [MarshalAs(UnmanagedType.LPArray)] string sBuffer, UIntPtr nSize, [MarshalAs(UnmanagedType.LPArray)] string sBufferDescription, Object pTable)
        {
            LoggingService.Info("HookExecuteBufferW called");
            LoggingService.Info(sBufferDescription);
            LoggingService.Info(sBuffer);

            // int result = CallOriginal<int>(hWnd, "This is the new text", caption, type);

            //return result;
            return true;
            //return CallOriginal<bool>(hWnd, sBuffer, nSize, sBufferDescription, pTable);
        }

        public override Delegate GetDetourDelegate()
        {
            return new HookExecuteBufferWDelegate(DetourMethod);
        }

        public override Delegate GetHookDelegate()
        {
            GameSharpProcess process = GameSharpProcess.Instance;

            ModulePointer cryDll = process.Modules["cryscriptsystem.dll"];

            MemoryPointer executeBufferWPtr = cryDll.GetProcAddress("IScriptSystem::ExecuteBuffer");

            LoggingService.Info("Found ExecuteBuffer address" + executeBufferWPtr);


            return executeBufferWPtr.ToDelegate<HookExecuteBufferWDelegate>();
        }
    }
}
