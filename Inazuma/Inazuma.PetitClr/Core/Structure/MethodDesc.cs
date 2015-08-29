using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Inazuma.Mono.Cecil;

namespace Inazuma.PetitClr.Core.Structure
{
    [DebuggerDisplay("MethodDesc: {Name} ({MdToken})")]
    public class MethodDesc
    {
        public MetadataToken MdToken;

        public string Name; // for debug

        public MethodDefinition Definition { get; set; }
        public int Slot { get; set; }
    }
}
