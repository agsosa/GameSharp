using Injectable.Helpers;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Injectable.Functions
{
    public class IssueOrder
    {
        public enum OrderType
        {
            Move = 2,
            Attack = 3
        }

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        private delegate IntPtr IssueOrderDelegate(IntPtr thisPtr, OrderType order, Vector3 targetPos, IntPtr targetObject, bool b1, bool b2, int networkId);

        public static void Move(string message)
        {
            Marshal.GetDelegateForFunctionPointer<IssueOrderDelegate>(Offsets.IssueOrder).DynamicInvoke
                (0, OrderType.Move, 0, 0, 0, 0, 0);
        }
    }
}