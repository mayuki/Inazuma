﻿using System;
using System.Runtime.CompilerServices;

namespace System.Diagnostics
{
    public sealed class Debugger
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Break();
    }
}
