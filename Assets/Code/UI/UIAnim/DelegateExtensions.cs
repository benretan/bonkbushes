
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extensions
{
    public static class DelegateExtensions
    {
        public static void SafeInvoke(this Action action)
        {
            if (action != null)
                action();
        }
        public static void SafeInvoke<T>(this Action<T> action, T arg1)
        {
            if (action != null)
                action(arg1);
        }
        public static void SafeInvoke<T1, T2>(this Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            if (action != null)
                action(arg1, arg2);
        }

        public static void InvokeNextFrame(this Action action)
        {

        }
        public static void InvokeNextFrame<T>(this Action<T> action, T arg1)
        {

        }
        public static void InvokeNextFrame<T1, T2>(this Action<T1, T2> action, T1 arg1, T2 arg2)
        {

        }
    }
}