using GameSharp.Core.Memory;
using GameSharp.Core.Module;
using GameSharp.Core.Services;
using GameSharp.Internal;
using GameSharp.Internal.Extensions;
using GameSharp.Internal.Memory;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GameSharp.Notepadpp.FunctionWrapper
{
    public class ExecuteBufferW : SafeFunction
    {
        //https://github.com/aws/lumberyard/blob/master/dev/Gems/CryLegacy/Code/Source/CryScriptSystem/ScriptSystem.cpp
        //bool CScriptSystem::ExecuteBuffer(const char* sBuffer, size_t nSize, const char* sBufferDescription, IScriptTable* pEnv)

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, SetLastError = true, CharSet = CharSet.Unicode)]
        private delegate int ExecuteBufferWDelegate(Object hWnd, [MarshalAs(UnmanagedType.LPArray)] string sBuffer, UIntPtr nSize, [MarshalAs(UnmanagedType.LPArray)] string sBufferDescription, Object pTable);

        protected override Delegate InitializeDelegate()
        {
            GameSharpProcess process = GameSharpProcess.Instance;

            ModulePointer cryDll = process.Modules["cryscriptsystem.dll"];

            LoggingService.Info("Found cryscriptsystem.dll address" + cryDll.ToString());

            MemoryPointer executeBufferWPtr = cryDll.GetProcAddress("IScriptSystem@ExecuteBuffer");
            MemoryPointer b = cryDll.GetProcAddress("IScriptSystem::ExecuteBuffer");
            MemoryPointer c = cryDll.GetProcAddress("ExecuteBuffer");
            MemoryPointer d = cryDll.GetProcAddress("CScriptSystem::ExecuteBuffer");
            MemoryPointer e = cryDll.GetProcAddress("IScriptSystem@@ExecuteBuffer");
            MemoryPointer f = cryDll.GetProcAddress("CScriptSystem@@ExecuteBuffer");
            MemoryPointer g = cryDll.GetProcAddress("CreateScriptSystem");

            LoggingService.Info("Found ExecuteBuffer address a " + executeBufferWPtr);
            LoggingService.Info("Found ExecuteBuffer address b " + b);
            LoggingService.Info("Found ExecuteBuffer address c " + c); 
            LoggingService.Info("Found ExecuteBuffer address d " + d);
            LoggingService.Info("Found ExecuteBuffer address e " + e);
            LoggingService.Info("Found ExecuteBuffer address f " + f);
            LoggingService.Info("Found ExecuteBuffer address g " + g);

            return executeBufferWPtr.ToDelegate<ExecuteBufferWDelegate>();
        }

        public bool Call(Object hWnd, [MarshalAs(UnmanagedType.LPArray)] string sBuffer, UIntPtr nSize, [MarshalAs(UnmanagedType.LPArray)] string sBufferDescription, Object pTable)
        {
            // 0 means fail
            return Call<bool>(hWnd, sBuffer, nSize, sBufferDescription, pTable);
        }
    }
}
