using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System
{
    public class Console
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteLine();
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteLine(Object arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteLine(Char[] arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteLine(Char arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteLine(Boolean arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteLine(Decimal arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteLine(Single arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteLine(Double arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteLine(Int16 arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteLine(UInt16 arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteLine(Int32 arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteLine(UInt32 arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteLine(Int64 arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteLine(UInt64 arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteLine(String arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteLine(String format, Object arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteLine(String format, Object arg0, Object arg1);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteLine(String format, Object arg0, Object arg1, Object arg2);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteLine(String format, Object arg0, Object arg1, Object arg2, Object arg3);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Write(Object arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Write(Char[] arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Write(Char arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Write(Boolean arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Write(Decimal arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Write(Single arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Write(Double arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Write(Int16 arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Write(UInt16 arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Write(Int32 arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Write(UInt32 arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Write(Int64 arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Write(UInt64 arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Write(String arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Write(String format, Object arg0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Write(String format, Object arg0, Object arg1);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Write(String format, Object arg0, Object arg1, Object arg2);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Write(String format, Object arg0, Object arg1, Object arg2, Object arg3);
    }
}
