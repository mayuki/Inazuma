using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Inazuma.Mono.Cecil;
using Inazuma.PetitClr.Core.Loader;
using Inazuma.PetitClr.Core.Structure.Class;

namespace Inazuma.PetitClr.Core.Structure
{

    [DebuggerDisplay("MethodTable: {ClassName} ({MdToken})")]
    public class MethodTable
    {
        public MetadataToken MdToken;
        public int NumVirtuals;

        public EEClass EEClass;
        public string ClassName; // for debug

        public MethodTable ParentMethodTable;
        public object Module;

        public MethodTable[] Interfaces;
        public MethodDesc[] MethodSlots;

        public ObjectInstance[] StaticFields;

        public MethodDesc LookupMethod(MetadataToken token)
        {
            return MethodSlots.First(x => x.MdToken == token);
        }

        public Dictionary<MethodReference, int> InterfaceMethodSlotMap;
    }

    class MethodTableBuilder
    {
        private ClassLoader _classLoader;
        private TypeDefinition _typeDefinition;

        private MethodTable _halfBakedMethodTable;

        public MethodTableBuilder(ClassLoader classLoader, TypeDefinition typeDef)
        {
            _classLoader = classLoader;
            _typeDefinition = typeDef;
        }

        public static MethodTable BuildMethodTable(ClassLoader classLoader, TypeDefinition typeDef)
        {
            return new MethodTableBuilder(classLoader, typeDef).BuildMethodTable();
        }

        public MethodTable BuildMethodTable()
        {
            _halfBakedMethodTable = new MethodTable()
            {
                MdToken = _typeDefinition.MetadataToken,
                ClassName = _typeDefinition.FullName,
                ParentMethodTable = (_typeDefinition.BaseType != null) ? _classLoader.LoadTypeFromTypeRef(_typeDefinition.BaseType) : null,
                EEClass = null,
                Interfaces = CreateInterfacesFromTypeDef(),
                MethodSlots = CreateMethodSlotsFromTypeDef(_typeDefinition),
                InterfaceMethodSlotMap = _typeDefinition.HasInterfaces ? new Dictionary<MethodReference, int>() : null,
            };

            _halfBakedMethodTable.EEClass = new EEClass()
            {
                ParentMethodTable = ParentMethodTable,
            };

            if (_typeDefinition.HasInterfaces)
            {
                CreateInterfaceVtableMap();
            }

            // Go thru all fields and initialize their FieldDescs.
            InitializeFieldDescs();

            // Place regular static fields
            PlaceRegularStaticFields();

            // Place thread static fields
            PlaceThreadStaticFields();

            // Create static field slots
            _halfBakedMethodTable.StaticFields = new ObjectInstance[HalfBakedClass.NumStaticFields + HalfBakedClass.NumThreadStaticFields];
            for (var i = 0; i < _halfBakedMethodTable.StaticFields.Length; i++)
            {
                _halfBakedMethodTable.StaticFields[i] = ObjectInstance.Null;
            }

            if (false /* IsBlittable || IsManagedSequential */)
            {
                // TODO: not implemented yet
            }
            else
            {
                // HandleExplicitLayout fails for the GenericTypeDefinition when
                // it will succeed for some particular instantiations.
                // Thus we only do explicit layout for real instantiations, e.g. C<int>, not
                // the open types such as the GenericTypeDefinition C<!0> or any
                // of the "fake" types involving generic type variables which are
                // used for reflection and verification, e.g. C<List<!0>>.
                // 
                if (false /* !bmtGenerics->fContainsGenericVariables && HasExplicitFieldOffsetLayout */)
                {
                    //HandleExplicitLayout(pByValueClassCache);
                    // TODO: not implemented yet
                }
                else
                {
                    // Place instance fields
                    PlaceInstanceFields();
                }
            }

            return _halfBakedMethodTable;
        }

        private EEClass HalfBakedClass { get { return _halfBakedMethodTable.EEClass; } }
        private MethodTable ParentMethodTable { get { return _halfBakedMethodTable.ParentMethodTable; } }
        private bool HasParent { get { return _halfBakedMethodTable.ParentMethodTable != null; } }

        private void InitializeFieldDescs()
        {
            var numStaticFields = 0;
            var numThreadStaticFields = 0;
            var currentDeclaredField = 0;

            var fieldDescs = new List<FieldDesc>();
            for (var i = 0; i < _typeDefinition.Fields.Count; i++)
            {
                var fieldDef = _typeDefinition.Fields[i];

                // We don't store static final primitive fields in the class layout
                if (fieldDef.IsLiteral)
                {
                    continue;
                }

                if (!fieldDef.IsPublic)
                {
                    HalfBakedClass.HasNonPublicFields = true;
                }

                if (fieldDef.IsStatic)
                {
                    if (false)
                    {
                        // TODO: ThreadStatic attr
                        // TODO: not implemented yet
                    }
                    else
                    {
                        fieldDescs.Add(new FieldDesc(fieldDef) { Offset = -1 /* FIELD_OFFSET_UNPLACED */ });
                        numStaticFields++;
                    }
                }
                else
                {
                    fieldDescs.Add(new FieldDesc(fieldDef) { Offset = -1 /* FIELD_OFFSET_UNPLACED */ });
                    currentDeclaredField++;
                }
            }

            HalfBakedClass.FieldDescList = fieldDescs.ToArray();
            HalfBakedClass.NumInstanceFields = HasParent ? ParentMethodTable.EEClass.NumInstanceFields + currentDeclaredField : 0;
            HalfBakedClass.NumStaticFields = numStaticFields;
            HalfBakedClass.NumThreadStaticFields = numThreadStaticFields;

            // Cache Field descriptors
            _classLoader.RegisterFieldDescRange(HalfBakedClass.FieldDescList);
        }

        private void CreateInterfaceVtableMap()
        {
            // TODO: place interface method impls.
        }

        private void PlaceRegularStaticFields()
        {
            var cumulativeStaticFieldPos = 0;
            foreach (var fieldDesc in HalfBakedClass.FieldDescList)
            {
                if (fieldDesc.IsStatic)
                {
                    fieldDesc.Offset = cumulativeStaticFieldPos++;
                }
            }
        }

        private void PlaceThreadStaticFields()
        { }

        private void PlaceInstanceFields()
        {
            var cumulativeInstanceFieldPos = HasParent ? ParentMethodTable.EEClass.NumInstanceFields : 0;
            foreach (var fieldDesc in HalfBakedClass.FieldDescList)
            {
                if (!fieldDesc.IsStatic)
                {
                    fieldDesc.Offset = cumulativeInstanceFieldPos++;
                }
            }
        }

        private MethodTable[] CreateInterfacesFromTypeDef()
        {
            var parentInterfaces = _typeDefinition.BaseType != null ? _classLoader.LoadTypeFromTypeRef(_typeDefinition.BaseType).Interfaces : new MethodTable[0];
            return parentInterfaces.Concat(_typeDefinition.Interfaces.Select(x => BuildMethodTable(_classLoader, x.Resolve()))).ToArray();
        }

        private MethodDefinition[] GetVirtualMethodDefsFromTypeDef(TypeDefinition typeDef)
        {
            var methods = new List<MethodDefinition>();
            var declaredMethods = typeDef.Methods.Where(x => x.IsVirtual && !x.IsStatic && !x.DeclaringType.IsInterface).ToArray();
            var nonOverrideMethods = new List<MethodDefinition>();
            if (typeDef.BaseType != null)
            {
                var parentMethods = GetVirtualMethodDefsFromTypeDef(typeDef.BaseType.Resolve());

                for (var i = 0; i < declaredMethods.Length; i++)
                {
                    var declMethod = declaredMethods[i];
                    var overrided = false;

                    for (var j = 0; j < parentMethods.Length; j++)
                    {
                        var parentMethod = parentMethods[j];

                        if (parentMethod.Name == declMethod.Name /* && ... */)
                        {
                            parentMethods[j] = declMethod;
                            overrided = true;
                            break;
                        }
                    }

                    if (!overrided)
                    {
                        nonOverrideMethods.Add(declMethod);
                    }
                }
                methods.AddRange(parentMethods); // virtual (parent/override)
                methods.AddRange(nonOverrideMethods); // virtual
            }
            else
            {
                methods.AddRange(declaredMethods); // virtual
            }
            return methods.ToArray();
        }

        private MethodDesc[] CreateMethodSlotsFromTypeDef(TypeDefinition typeDef)
        {
            var methods = new List<MethodDefinition>();

            methods.AddRange(GetVirtualMethodDefsFromTypeDef(typeDef)); // virtual
            methods.AddRange(typeDef.Methods.Where(x => !x.IsVirtual && !x.IsStatic)); // non-virtual
            methods.AddRange(typeDef.Methods.Where(x => x.IsStatic)); // static

            return methods
                    .Select((x, i) =>
                    {
                        var methodDesc = _classLoader.LookupMethodDescFromMethodDef(x);
                        if (methodDesc == null)
                        {
                            methodDesc = new MethodDesc()
                            {
                                MdToken = x.MetadataToken,
                                Name = x.FullName,
                                Definition = x,
                                Slot = i
                            };
                            _classLoader.RegisterMethodDesc(x, methodDesc);
                        }
                        return methodDesc;
                    })
                    .ToArray();
        }
    }
}
