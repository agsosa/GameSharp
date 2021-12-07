﻿using GameSharp.Core.Services;
using GameSharp.Notepadpp.Hooks;
using RGiesecke.DllExport;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace GameSharp.Notepadpp
{
    public class Entrypoint
    {
        // Prevents it from being cleaned by the GC
       // private static readonly HookMessageBoxW messageBoxHook = new HookMessageBoxW();
        private static readonly HookExecuteBufferW executeBufferHook = new HookExecuteBufferW();

        [DllExport]
        public static void Main()
        {
            LoggingService.Info("I have been injected!");

            /*LoggingService.Info("Calling MessageBoxW!");
            if (!Functions.MessageBoxW.Call(IntPtr.Zero, "Through a SafeFunctionCall method", "Caption", 0))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }*/


            LoggingService.Info("Calling ExecuteBuffer!");

            if (!Functions.ExecuteBufferW.Call(IntPtr.Zero, "ExecuteBuffer Through a SafeFunctionCall method", UIntPtr.Zero, "Caption", 0))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            //LoggingService.Info("Enabling hook on MessageBoxW!");
            //messageBoxHook.Enable();

            LoggingService.Info("Enabling hook on Execute Buffer!");
            executeBufferHook.Enable();

        }
    }
}
