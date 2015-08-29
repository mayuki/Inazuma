using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inazuma.Mono.Cecil;
using Inazuma.PetitClr.Core.Structure;
using MethodTable = Inazuma.PetitClr.Core.Structure.MethodTable;

namespace Inazuma.PetitClr.Core.Loader
{
    public class ClassLoader
    {
        private Dictionary<MetadataToken, MethodTable> _loadedTypes = new Dictionary<MetadataToken, MethodTable>();

        private Dictionary<MethodDefinition, MethodDesc> _methodDescByMethodDef = new Dictionary<MethodDefinition, MethodDesc>();
        private Dictionary<FieldDefinition, FieldDesc> _fieldDescByFieldDef = new Dictionary<FieldDefinition, FieldDesc>();

        public MethodTable LoadTypeFromTypeRef(TypeReference typeRef)
        {
            return LoadType(typeRef.Resolve()); // TODO: improve performance
        }

        public MethodTable LoadType(TypeDefinition typeDef)
        {
            if (!_loadedTypes.ContainsKey(typeDef.MetadataToken))
            {
                _loadedTypes[typeDef.MetadataToken] = MethodTableBuilder.BuildMethodTable(this, typeDef);
            }
            return _loadedTypes[typeDef.MetadataToken];
        }

        public void RegisterMethodDesc(MethodDefinition methodDef, MethodDesc methodDesc)
        {
            _methodDescByMethodDef[methodDef] = methodDesc;
        }
        public MethodDesc LookupMethodDescFromMethodDef(MethodDefinition methodDef)
        {
            return _methodDescByMethodDef.ContainsKey(methodDef) ? _methodDescByMethodDef[methodDef] : null;
        }

        public void RegisterFieldDesc(FieldDefinition fieldDef, FieldDesc fieldDesc)
        {
            _fieldDescByFieldDef[fieldDef] = fieldDesc;
        }
        public void RegisterFieldDescRange(IEnumerable<FieldDesc> fieldDescs)
        {
            foreach (var fieldDesc in fieldDescs)
            {
                _fieldDescByFieldDef[fieldDesc.Definition] = fieldDesc;
            }
        }
        public FieldDesc LookupFieldDescFromFieldDef(FieldDefinition fieldDef)
        {
            return _fieldDescByFieldDef.ContainsKey(fieldDef) ? _fieldDescByFieldDef[fieldDef] : null;
        }
    }
}
