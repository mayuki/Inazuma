using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inazuma.Mono.Cecil;

namespace Inazuma.PetitClr.Core.Structure
{
    public class FieldDesc
    {
        public FieldDesc(FieldDefinition fieldDef)
        {
            Definition = fieldDef;
        }

        // TODO: not implemented
        public bool IsStatic { get { return Definition.IsStatic; } }

        public FieldDefinition Definition { get; set; }
        public int Offset { get; set; }
    }
}
