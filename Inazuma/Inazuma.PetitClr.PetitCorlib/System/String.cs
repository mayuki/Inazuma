using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System
{
    public class String
    {
        public String()
        {}

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static string Format(string format, object[] arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static string Format(string format, object arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static string Format(string format, object arg0, object arg1);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static string Format(string format, object arg0, object arg1, object arg2);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static string Concat<T>(IEnumerable<T> args);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static string Concat(object arg);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static string Concat(object[] args);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static string Concat(IEnumerable<string> args);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static string Concat(string[] args);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static string Concat(object arg0, object arg1);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static string Concat(string arg0, string arg1);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static string Concat(object arg0, object arg1, object arg2);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static string Concat(string arg0, string arg1, string arg2);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static string Concat(object arg0, object arg1, object arg2, object arg3);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static string Concat(string arg0, string arg1, string arg2, string arg3);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool IsNullOrEmpty(string s);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool IsNullOrWhitespace(string s);

        public const string Empty = "";

        public int Length
        {
            get
            {
                Debugger.Break();
                // TODO: not implemented
                return 0;
            }
        }

    }
}
