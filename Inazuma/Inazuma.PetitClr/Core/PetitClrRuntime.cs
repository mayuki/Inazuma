using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Inazuma.PetitClr.Core;
using Inazuma.PetitClr.Core.Structure;

namespace Inazuma.PetitClr.Core
{
    public class PetitClrRuntime
    {
        public PetitClrHeap GCHeap { get; set; }

        public Dictionary<string, Func<ObjectInstance, Frame, object[], object>> InternalCallMethods { get; set; }

        public static PetitClrRuntime Current { get; set; }

        static PetitClrRuntime()
        {
            Current = new PetitClrRuntime();
            Current.Initialize();
        }

        public void Initialize()
        {
            GCHeap = new PetitClrHeap();
            InternalCallMethods = new Dictionary<string, Func<ObjectInstance, Frame, object[], object>>();
        }

        public static ObjectInstance AllocateObject(MethodTable methodTable, CorInfoType type = CorInfoType.Class)
        {
            var newObject = PetitClrRuntime.Current.GCHeap.Alloc();
            newObject.MethodTable = methodTable;
            newObject.Type = type;

            newObject.FieldInstances = new ObjectInstance[methodTable.EEClass.NumInstanceFields];
            for (var i = 0; i < newObject.FieldInstances.Length; i++)
            {
                newObject.FieldInstances[i] = ObjectInstance.Null;
            }

            // TODO: implements type assertion
            //Debug.Assert(methodTable.EEClass.IsValueType == (newObject.Type == CorInfoType.ValueClass));

            return newObject;
        }
    }
}
