using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inazuma.Mono.Cecil;

namespace Inazuma.PetitClr.Core.Structure.Class
{
    public class EEClass
    {
        private Dictionary<FieldDefinition, FieldDesc> _descByDefs = new Dictionary<FieldDefinition, FieldDesc>();

        public FieldDesc LookupFieldDesc(FieldDefinition fieldDef)
        {
            FieldDesc fieldDesc = null;

            if (ParentMethodTable != null)
            {
                fieldDesc = ParentMethodTable.EEClass.LookupFieldDesc(fieldDef);
                if (fieldDesc != null)
                {
                    return fieldDesc;
                }
            }

            // TODO: to be thread safety
            if (!_descByDefs.ContainsKey(fieldDef))
            {
                foreach (var f in FieldDescList)
                {
                    if (f.Definition == fieldDef)
                    {
                        fieldDesc = _descByDefs[fieldDef] = f;
                        break;
                    }
                }
            }
            else
            {
                fieldDesc = _descByDefs[fieldDef];
            }

            return fieldDesc;
        }

        public MethodTable ParentMethodTable { get; set; }

        public FieldDesc[] FieldDescList { get; set; } // InstanceFields + StaticFields (ThreadStatic+Static)

        public int NumInstanceFields { get; set; }
        public int NumStaticFields { get; set; }
        public int NumThreadStaticFields { get; set; }

        public UInt32 AttrClass { get; set; }

        public bool HasNonPublicFields { get; set; }

        public bool HasLayout
        {
            get
            {
                throw ThrowHelper.NotImplementedYet;
            }
            set
            {
                throw ThrowHelper.NotImplementedYet;
            }
        }
        public bool HasOverLayedFields
        {
            get
            {
                throw ThrowHelper.NotImplementedYet;
            }
            set
            {
                throw ThrowHelper.NotImplementedYet;
            }
        }
        public bool IsNested
        {
            get
            {
                throw ThrowHelper.NotImplementedYet;
            }
            set
            {
                throw ThrowHelper.NotImplementedYet;
            }
        }

        public bool IsDelegate
        {
            get
            {
                throw ThrowHelper.NotImplementedYet;
            }
            set
            {
                throw ThrowHelper.NotImplementedYet;
            }
        }

        public bool IsBlittable
        {
            // TODO: not implemented yet
            get { return false; }
        }
    }
}
