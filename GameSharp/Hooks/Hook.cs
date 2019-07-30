﻿// Part of the credits go to Lolp1 for giving the idea how to finish.
// https://github.com/lolp1/Process.NET/blob/master/src/Process.NET/Applied/Detours/Detour.cs
//

using GameSharp.Extensions;
using GameSharp.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GameSharp.Hooks
{
    public abstract class Hook
    {
        /// <summary>
        ///     This var is not used within the detour itself. It is only here
        ///     to keep a reference, to avoid the GC from collecting the delegate instance!
        /// </summary>
        private readonly Delegate HookDelegate;

        /// <summary>
        ///     This variable contains the jump from the module where the hookable function resides into our module.
        ///     Bypass for an anti-cheat which is validating the return address of a function to reside in it's own module.
        /// </summary>
        private Patch CodeCavePatch { get; set; }

        private IntPtr HookPtr { get; }

        private Patch HookPatch { get; set; }

        private IntPtr TargetFuncPtr { get; }

        private Delegate TargetDelegate { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Hook" /> class.
        ///     
        ///     A hook can be used to create a detour from an original function into your own function.
        ///     You can then proceed to call the original function by using the method <see cref="CallOriginal" />.
        /// </summary>
        /// <param name="target">The target delegate we want to detour.</param>
        /// <param name="hook">The hook delegate where want it to go.</param>
        public Hook()
        { 
            TargetDelegate = GetHookDelegate();
            TargetFuncPtr = TargetDelegate.ToFunctionPtr();

            HookDelegate = GetDetourDelegate();
            HookPtr = HookDelegate.ToFunctionPtr();

            InitializeAntiCheatHook();
        }

        private void InitializeAntiCheatHook()
        {
            byte[] bytes = GetHookBytes(HookPtr);
            ProcessModule module = TargetFuncPtr.GetModuleWhichBelongsToAddress();

            if (module == null)
                throw new NullReferenceException("Cannot find a module which belongs to the specified pointer.");

            IntPtr codeCave = module.FindCodeCaveInModule((uint) bytes.Length);
            CodeCavePatch = new Patch(codeCave, bytes);

            byte[] retToCodeCave = GetHookBytes(CodeCavePatch.PatchAddress);
            HookPatch = new Patch(TargetFuncPtr, retToCodeCave);
        }

        private byte[] GetHookBytes(IntPtr ptrToJumpTo)
        {
            // PUSH opcode http://ref.x86asm.net/coder32.html#x68
            List<byte> bytes = new List<byte> { 0x68 };

            // Push our hook address onto the stack
            byte[] hookPtrAddress = IntPtr.Size == 4 ? BitConverter.GetBytes(ptrToJumpTo.ToInt32()) : BitConverter.GetBytes(ptrToJumpTo.ToInt64());

            bytes.AddRange(hookPtrAddress);

            // RETN opcode http://ref.x86asm.net/coder32.html#xC3
            bytes.Add(0xC3);

            return bytes.ToArray();
        }

        /// <summary>
        ///     Reverts the bytes back to their original value.
        /// </summary>
        public void Disable()
        {
            CodeCavePatch.Disable();
            HookPatch.Disable();
        }

        /// <summary>
        ///     Writes the bytes we have created for our hook to the process memory.
        /// </summary>
        /// <returns></returns>
        public void Enable()
        {
            CodeCavePatch.Enable();
            HookPatch.Enable();
        }

        /// <summary>
        ///     Source code found on
        ///     https://github.com/lolp1/Process.NET/blob/master/src/Process.NET/Applied/Detours/Detour.cs#L215
        ///     
        ///     Calls the original function, and returns a return value.
        /// </summary>
        /// <param name="args">
        ///     The arguments to pass. You MUST pass 'null' if it is a 'void' argument list.
        /// </param>
        /// <returns>An object containing the original functions return value.</returns>
        public T CallOriginal<T>(params object[] args)
        {
            Disable();

            object ret = TargetDelegate.DynamicInvoke(args);

            Enable();

            return (T) ret;
        }

        /// <summary>
        ///     This should return an UnmanagedFunctionPointer delegate.
        ///
        ///     e.g. Marshal.GetDelegateForFunctionPointer<DELEGATE>(ADDRESS);
        /// </summary>
        /// <returns></returns>
        public abstract Delegate GetHookDelegate();

        /// <summary>
        ///     This should return the delegate to the hook with the function where it needs to go.
        ///
        ///     e.g. return new OnAfkDelegate(DetourMethod);
        /// </summary>
        public abstract Delegate GetDetourDelegate();

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources. In this
        ///     case, it will disable the <see cref="Hook" /> instance and suppress the finalizer.
        /// </summary>
        public void Dispose()
        {
            Disable();
            GC.SuppressFinalize(this);
        }
    }
}