using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inazuma.PetitClr.Core.Structure
{
    public enum CorInfoType : byte
    {
        Undef = 0x0,
        Void = 0x1,
        Bool = 0x2,
        Char = 0x3,
        Byte = 0x4,
        UByte = 0x5,
        Short = 0x6,
        UShort = 0x7,
        Int = 0x8,
        UInt = 0x9,
        Long = 0xa,
        ULong = 0xb,
        NativeInt = 0xc,
        NativeUInt = 0xd,
        Float = 0xe,
        Double = 0xf,
        String = 0x10,         // Not used, should remove
        Ptr = 0x11,
        ByRef = 0x12,
        ValueClass = 0x13,
        Class = 0x14,
        RefAny = 0x15,

        // Var is for a generic type variable.
        // Generic type variables only appear when the JIT is doing
        // verification (not NOT compilation) of generic code
        // for the EE, in which case we're running
        // the JIT in "import only" mode.

        Var = 0x16,
        Count,                         // number of jit types
    }
}
