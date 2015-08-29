using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Inazuma.PetitClr.Core.Structure
{
    [DebuggerDisplay("ObjectInstance: Type={Type}; I={I}; R={R}; MethodTable={MethodTable}; ObjectRef={ObjectRef}")]
    [StructLayout(LayoutKind.Explicit)]
    public class ObjectInstance
    {
        [FieldOffset(0)]
        public Int64 I;
        [FieldOffset(0)]
        public Double R;

        [FieldOffset(8)]
        public CorInfoType Type;

        [FieldOffset(16)] // aligned
        public MethodTable MethodTable;
        [FieldOffset(24)] // aligned
        public ObjectInstance[] FieldInstances; // ObjectRef or Value
        [FieldOffset(32)] // aligned
        public Object ObjectRef; // ObjectRef

        public ObjectInstance GetInstanceOrCopiedValue()
        {
            if (Type == CorInfoType.Class || Type == CorInfoType.String)
            {
                return this;
            }

            // Copy ValueType or Primitive
            return new ObjectInstance() { I = I, R = R, Type = Type, MethodTable = MethodTable, FieldInstances = (FieldInstances != null) ? FieldInstances.Select(x => x).ToArray() : null };
        }

        public static readonly ObjectInstance Null = new ObjectInstance();

        public bool IsReference
        {
            get { return (Type == CorInfoType.Class || Type == CorInfoType.String); }
        }
        public bool IsNull
        {
            get { return this.I == 0 && Type == CorInfoType.Undef; }
        }

        public static ObjectInstance FromClrObject(Boolean value)
        {
            return new ObjectInstance { I = ((Boolean)value) ? 1 : 0, Type = CorInfoType.Bool };
        }
        public static ObjectInstance FromClrObject(SByte value)
        {
            return new ObjectInstance { I = ((Int64)value), Type = CorInfoType.Byte };
        }
        public static ObjectInstance FromClrObject(Byte value)
        {
            return new ObjectInstance { I = ((Int64)value), Type = CorInfoType.UByte };
        }
        public static ObjectInstance FromClrObject(Int16 value)
        {
            return new ObjectInstance { I = ((Int64)value), Type = CorInfoType.Short };
        }
        public static ObjectInstance FromClrObject(Int32 value)
        {
            return new ObjectInstance { I = ((Int64)value), Type = CorInfoType.Int };
        }
        public static ObjectInstance FromClrObject(Int64 value)
        {
            return new ObjectInstance { I = ((Int64)value), Type = CorInfoType.Long };
        }
        public static ObjectInstance FromClrObject(UInt16 value)
        {
            return new ObjectInstance { I = ((Int64)value), Type = CorInfoType.UShort };
        }
        public static ObjectInstance FromClrObject(UInt32 value)
        {
            return new ObjectInstance { I = ((Int64)value), Type = CorInfoType.UInt };
        }
        public static ObjectInstance FromClrObject(UInt64 value)
        {
            return new ObjectInstance { I = ((Int64)value), Type = CorInfoType.ULong };
        }
        public static ObjectInstance FromClrObject(Single value)
        {
            return new ObjectInstance { R = ((Double)value), Type = CorInfoType.Float };
        }
        public static ObjectInstance FromClrObject(Double value)
        {
            return new ObjectInstance { R = ((Double)value), Type = CorInfoType.Double };
        }

        public static ObjectInstance FromClrObject(object value)
        {
            if (value is String)
            {
                return new ObjectInstance { ObjectRef = value, Type = CorInfoType.String };
            }
            else if (value == null)
            {
                return new ObjectInstance { ObjectRef = null, MethodTable = null, Type = CorInfoType.Class, I = 0 };
            }


            throw ThrowHelper.NotImplementedYet;
        }

        public object ToClrObject()
        {
            switch (Type)
            {
                case CorInfoType.Bool:
                    return (object)(bool)(I != 0);
                case CorInfoType.Char:
                    return (object)(char)I;
                case CorInfoType.Byte:
                    return (object)(sbyte)I;
                case CorInfoType.Short:
                    return (object)(short)I;
                case CorInfoType.Int:
                    return (object)(int)I;
                case CorInfoType.Long:
                    return (object)I;
                case CorInfoType.UByte:
                    return (object)(byte)I;
                case CorInfoType.UShort:
                    return (object)(ushort)I;
                case CorInfoType.UInt:
                    return (object)(uint)I;
                case CorInfoType.ULong:
                    return (object)(ulong)I;
                case CorInfoType.Float:
                    return (object)(float)R;
                case CorInfoType.Double:
                    return (object)R;
                //case CorInfoType.String:
                //case CorInfoType.Class:
                default:
                    return ObjectRef;
            }
        }
    }
}
