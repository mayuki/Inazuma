using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inazuma.PetitClr.Core.Structure
{
    //
    // from coreclr\src\inc\corhdr.h -----
    //
    public enum CorElementType : byte
    {
        End = 0x00,
        Void = 0x01,
        Boolean = 0x02,
        Char = 0x03,
        I1 = 0x04,
        U1 = 0x05,
        I2 = 0x06,
        U2 = 0x07,
        I4 = 0x08,
        U4 = 0x09,
        I8 = 0x0a,
        U8 = 0x0b,
        R4 = 0x0c,
        R8 = 0x0d,
        String = 0x0e,

        // every type above PTR will be simple type
        Ptr = 0x0f,     // ptr <type>
        ByRef = 0x10,     // byref <type>

        // Please use Valuetype. element_type_valueclass is deprecated.
        ValueType = 0x11,     // valuetype <class token>
        Class = 0x12,     // class <class token>
        Var = 0x13,     // a class type variable var <number>
        Array = 0x14,     // mdarray <type> <rank> <bcount> <bound1> ... <lbcount> <lb1> ...
        GenericInst = 0x15,     // genericinst <generic type> <argcnt> <arg1> ... <argn>
        TypedByRef = 0x16,     // typedref  (it takes no args) a typed referece to some other type

        I = 0x18,     // native integer size
        U = 0x19,     // native unsigned integer size
        FnPtr = 0x1b,     // fnptr <complete sig for the function including calling convention>
        Object = 0x1c,     // shortcut for system.object
        SzArray = 0x1d,     // shortcut for single dimension zero lower bound array
        // SZARRAY <type>
        Mvar = 0x1e,     // a method type variable mvar <number>

        // This is only for binding
        CmodReqd = 0x1f,     // required c modifier : e_t_cmod_reqd <mdtyperef/mdtypedef>
        CmodOpt = 0x20,     // optional c modifier : e_t_cmod_opt <mdtyperef/mdtypedef>

        // This is for signatures generated internally (which will not be persisted in any way).
        Internal = 0x21,     // internal <typehandle>

        // Note that this is the max of base type excluding modifiers
        Max = 0x22,     // first invalid element type


        Modifier = 0x40,
        Sentinel = 0x01 | Modifier, // sentinel for varargs
        Pinned = 0x05 | Modifier,
    }
}
