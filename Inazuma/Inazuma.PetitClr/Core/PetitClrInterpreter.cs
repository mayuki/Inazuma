using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Inazuma.Mono.Cecil;
using Inazuma.Mono.Cecil.Cil;
using Inazuma.PetitClr.Core.Loader;
using Inazuma.PetitClr.Core.Metadata;
using Inazuma.PetitClr.Core.Structure;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Inazuma.PetitClr.Core
{
    public class PetitClrInterpreter
    {
        private Stack<ObjectInstance> _opStack = new Stack<ObjectInstance>();

        private Instruction[] _instructions;
        private int _instructionPtr;
        private ObjectInstance[] _localSlot;
        private MethodDefinition _methInfo2;
        private ClassLoader _classLoader;

        private IList<ObjectInstance> _args;
        private object _stubContext;
        private bool _directCall;
        private ObjectInstance _callThisArg;

        public MethodDefinition MethodInfo { get { return _methInfo2; } }

        public PetitClrInterpreter(ClassLoader classLoader, MethodDefinition methodDef, bool directCall, IList<ObjectInstance> args, object stubContext)
        {
            _classLoader = classLoader;
            _methInfo2 = methodDef;
            _directCall = directCall;
            _args = args;
            _stubContext = stubContext;

            _instructions = methodDef.Body.Instructions.ToArray();
            _instructionPtr = 0;
            _opStack = new Stack<ObjectInstance>();
            _localSlot = new ObjectInstance[methodDef.Body.Variables.Count];
        }

        private ObjectInstance InterpretMethodBody(MethodDefinition methDef, bool directCall, IList<ObjectInstance> args, object stubContext)
        {
            for (var doJmpCall = true; doJmpCall; )
            {
                var retVal = ExecuteMethodWrapper(_classLoader, methDef, directCall, args, stubContext, out doJmpCall);
                if (!doJmpCall)
                {
                    return retVal;
                }
            }
            throw ThrowHelper.NotImplementedYet;
        }

        private void ExecuteMethod(out ObjectInstance retVal, out bool doJmpCall, out MemberReference jmpCallToken)
        {
            jmpCallToken = null;
            doJmpCall = false;
            retVal = null;

            EvalLoop:
            try
            {
                this.ExecuteMethodCore(out retVal, out doJmpCall, out jmpCallToken);
            }
            catch (InazumaRuntimeException ex)
            {
                // TODO: check throwable
                // TODO: isuncatchable (ThreadAbort)
                var handleEx = false;

                if (_methInfo2.Body.HasExceptionHandlers)
                {
                    var curOffset = _instructions[_instructionPtr].Offset;

                    // Perform a filter scan or regular walk of the EH Table. Filter scan is performed when
                    // we are evaluating a series of filters to handle the exception until the first handler
                    // (filter's or otherwise) that will handle the exception.
                    foreach (var exceptionHandler in _methInfo2.Body.ExceptionHandlers)
                    {
                        var handlerOffset = 0;

                        // First, is the current offset in the try block?
                        if (exceptionHandler.TryStart.Offset <= curOffset && exceptionHandler.TryEnd.Offset >= curOffset)
                        {
                            // CORINFO_EH_CLAUSE_NONE represents 'catch' blocks
                            if (exceptionHandler.FilterStart == null)
                            {
                                // Now, does the catch block handle the thrown exception type?
                                var excType = _classLoader.LoadTypeFromTypeRef(exceptionHandler.CatchType);
                                if (ex.ExceptionObject.MethodTable == excType)
                                {
                                    _opStack.Clear();
                                    _opStack.Push(ex.ExceptionObject);
                                    handlerOffset = _instructions.Select((x, i) => new { Index = i, IsMatched = x == exceptionHandler.HandlerStart }).First(x => x.IsMatched).Index; // TODO: FIXME
                                    handleEx = true;
                                    //_filterNextScan = 0;
                                }
                            }
                            else
                            {
                                throw ThrowHelper.NotImplementedYet;
                            }

                            // Reset the interpreter loop in preparation of calling the handler.
                            if (handleEx)
                            {
                                // Set the IL offset of the handler.
                                _instructionPtr = handlerOffset;

                                // If an exception occurs while attempting to leave a protected scope,
                                // we empty the 'leave' info stack upon entering the handler.
                                // TODO: infoStack

                                // Some things are set up before a call, and must be cleared on an exception caught be the caller.
                                // A method that returns a struct allocates local space for the return value, and "registers" that
                                // space and the type so that it's scanned if a GC happens.  "Unregister" it if we throw an exception
                                // in the call, and handle it in the caller.  (If it's not handled by the caller, the Interpreter is
                                // deallocated, so it's value doesn't matter.)
                                _callThisArg = null;
                                _args.Clear();

                                break;
                            }
                        }
                    }
                }

                if (handleEx)
                {
                    goto EvalLoop;
                }
                else
                {
                    throw;
                }
            }
        }

        public static ObjectInstance ExecuteMethodWrapper(ClassLoader classLoader, MethodDefinition methDef, bool directCall, IList<ObjectInstance> args,
            object stubContext, out bool doJmpCall)
        {
            var interp = new PetitClrInterpreter(classLoader, methDef, directCall, args, stubContext);
            var interpFrame = new InterpreterFrame(interp);

            ObjectInstance retVal;
            MemberReference jmpCallToken;
            interp.ExecuteMethod(out retVal, out doJmpCall, out jmpCallToken);

            if (doJmpCall)
            {
                throw ThrowHelper.NotImplementedYet;
            }

            interpFrame.Pop();
            return retVal;
        }

        private void ExecuteMethodCore(out ObjectInstance retVal, out bool doJmpCall, out MemberReference jmpCallToken)
        {
            jmpCallToken = null;
            doJmpCall = false;
            retVal = null;

            for (; _instructions.Length > _instructionPtr; _instructionPtr++)
            {
                var inst = _instructions[_instructionPtr];

                if (inst.OpCode == OpCodes.Nop)
                {
                    continue;
                }
                else if (inst.OpCode == OpCodes.Break)
                {
                    continue;
                }

                else if (inst.OpCode == OpCodes.Ldarg_0)
                {
                    LdArg(0);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Ldarg_1)
                {
                    LdArg(1);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Ldarg_2)
                {
                    LdArg(2);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Ldarg_3)
                {
                    LdArg(3);
                    continue;
                }

                else if (inst.OpCode == OpCodes.Ldloc_0)
                {
                    LdLoc(0);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Ldloc_1)
                {
                    LdLoc(1);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Ldloc_2)
                {
                    LdLoc(2);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Ldloc_3)
                {
                    LdLoc(3);
                    continue;
                }

                else if (inst.OpCode == OpCodes.Stloc_0)
                {
                    StLoc(0);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Stloc_1)
                {
                    StLoc(1);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Stloc_2)
                {
                    StLoc(2);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Stloc_3)
                {
                    StLoc(3);
                    continue;
                }

                else if (inst.OpCode == OpCodes.Ldarg_S)
                {
                    // TODO: get index directly
                    var index = (inst.Operand is ParameterReference) ? ((ParameterReference)inst.Operand).Index : ((VariableReference)inst.Operand).Index;
                    LdArg(index);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Ldarga_S)
                {
                    // TODO: get index directly
                    var index = (inst.Operand is ParameterReference) ? ((ParameterReference)inst.Operand).Index : ((VariableReference)inst.Operand).Index;
                    LdArgA(index);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Starg_S)
                {
                    // TODO: get index directly
                    var index = (inst.Operand is ParameterReference) ? ((ParameterReference)inst.Operand).Index : ((VariableReference)inst.Operand).Index;
                    StArg(index);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Ldloc_S)
                {
                    // TODO: get index directly
                    var index = (inst.Operand is ParameterReference) ? ((ParameterReference)inst.Operand).Index : ((VariableReference)inst.Operand).Index;
                    LdLoc(index);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Ldloca_S)
                {
                    // TODO: get index directly
                    var index = (inst.Operand is ParameterReference) ? ((ParameterReference)inst.Operand).Index : ((VariableReference)inst.Operand).Index;
                    LdLocA(index);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Stloc_S)
                {
                    // TODO: get index directly
                    var index = (inst.Operand is ParameterReference) ? ((ParameterReference)inst.Operand).Index : ((VariableReference)inst.Operand).Index;
                    StLoc(index);
                    continue;
                }

                else if (inst.OpCode == OpCodes.Ldnull)
                {
                    Ldnull();
                    continue;
                }

                else if (inst.OpCode == OpCodes.Ldc_I4_M1)
                {
                    LdIcon(-1);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Ldc_I4_0)
                {
                    LdIcon(0);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Ldc_I4_1)
                {
                    LdIcon(1);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Ldc_I4_2)
                {
                    LdIcon(2);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Ldc_I4_3)
                {
                    LdIcon(3);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Ldc_I4_4)
                {
                    LdIcon(4);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Ldc_I4_5)
                {
                    LdIcon(5);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Ldc_I4_6)
                {
                    LdIcon(6);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Ldc_I4_7)
                {
                    LdIcon(7);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Ldc_I4_8)
                {
                    LdIcon(8);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Ldc_I4_S)
                {
                    LdIcon(inst.Operand);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Ldc_I4)
                {
                    LdIcon(inst.Operand);
                    continue;
                }

                else if (inst.OpCode == OpCodes.Ldstr)
                {
                    LdStr();
                    continue;
                }

                else if (inst.OpCode == OpCodes.Ldfld)
                {
                    LdFld();
                    continue;
                }
                else if (inst.OpCode == OpCodes.Ldflda)
                {
                    LdFldA();
                    continue;
                }
                else if (inst.OpCode == OpCodes.Stfld)
                {
                    StFld();
                    continue;
                }
                else if (inst.OpCode == OpCodes.Ldsfld)
                {
                    LdSFld();
                    continue;
                }
                else if (inst.OpCode == OpCodes.Ldsflda)
                {
                    LdSFldA();
                    continue;
                }
                else if (inst.OpCode == OpCodes.Stsfld)
                {
                    StSFld();
                    continue;
                }
                else if (inst.OpCode == OpCodes.Stobj)
                {
                    StObj();
                    continue;
                }

                // ...
                else if (inst.OpCode == OpCodes.Add)
                {
                    Add(checkOverflow: false, unsigned: false);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Add_Ovf)
                {
                    Add(checkOverflow: true, unsigned: false);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Add_Ovf_Un)
                {
                    Add(checkOverflow: true, unsigned: true);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Sub)
                {
                    Sub(checkOverflow: false, unsigned: false);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Sub_Ovf)
                {
                    Sub(checkOverflow: true, unsigned: false);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Sub_Ovf_Un)
                {
                    Sub(checkOverflow: true, unsigned: true);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Mul)
                {
                    Mul(checkOverflow: false, unsigned: false);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Mul_Ovf)
                {
                    Mul(checkOverflow: true, unsigned: false);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Mul_Ovf_Un)
                {
                    Mul(checkOverflow: true, unsigned: true);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Div)
                {
                    Div(checkOverflow: true, unsigned: false);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Div_Un)
                {
                    Div(checkOverflow: true, unsigned: true);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Rem)
                {
                    Rem(checkOverflow: true, unsigned: false);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Rem_Un)
                {
                    Rem(checkOverflow: true, unsigned: true);
                    continue;
                }

                else if (inst.OpCode == OpCodes.Newobj)
                {
                    NewObj();
                    continue;
                }

                else if (inst.OpCode == OpCodes.Box)
                {
                    // TODO: not implemented yet
                    continue;
                }

                else if (inst.OpCode == OpCodes.Call)
                {
                    Call(virtualCall: false);
                    continue;
                }
                else if (inst.OpCode == OpCodes.Callvirt)
                {
                    Call(virtualCall: true);
                    continue;
                }

                else if (inst.OpCode == OpCodes.Ret)
                {
                    // TODO: use reference or type
                    if (_methInfo2.ReturnType.Namespace == "System" && _methInfo2.ReturnType.Name == "Void")
                    {
                        Debug.Assert(_opStack.Count == 0);
                    }
                    else
                    {
                        Debug.Assert(_opStack.Count != 0);
                        retVal = _opStack.Pop();
                    }

                    return; // goto ExitEvalLoop;
                }

                else if (inst.OpCode == OpCodes.Br_S)
                {
                    // TODO: below code is too slow
                    _instructionPtr = _instructions.Select((x, i) => new { Index = i, IsMatched = x == inst.Operand }).First(x => x.IsMatched).Index - 1; // increments ops ptr after "continue;". ptr must be subtracted here.
                    continue;
                }
                else if (inst.OpCode == OpCodes.Leave_S)
                {
                    _opStack.Clear();
                    throw ThrowHelper.NotImplementedYet;
                }
                else if (inst.OpCode == OpCodes.Brfalse_S)
                {
                    // TODO: below code is too slow
                    var value = _opStack.Pop();
                    if ((value.IsReference && value.IsNull) || value.I == 0)
                    {
                        _instructionPtr = _instructions.Select((x, i) => new { Index = i, IsMatched = x == inst.Operand }).First(x => x.IsMatched).Index - 1; // increments ops ptr after "continue;". ptr must be subtracted here.
                    }
                    continue;
                }
                else if (inst.OpCode == OpCodes.Brtrue_S)
                {
                    // TODO: below code is too slow
                    var value = _opStack.Pop();
                    if ((value.IsReference && !value.IsNull) || value.I != 0)
                    {
                        _instructionPtr = _instructions.Select((x, i) => new { Index = i, IsMatched = x == inst.Operand }).First(x => x.IsMatched).Index - 1; // increments ops ptr after "continue;". ptr must be subtracted here.
                    }
                    continue;
                }

                // case CEE_PREFIX1:
                else if (inst.OpCode == OpCodes.Arglist)
                {
                    throw ThrowHelper.NotImplementedYet;
                }
                else if (inst.OpCode == OpCodes.Ceq)
                {
                    var value2 = _opStack.Pop();
                    var value1 = _opStack.Pop();

                    if (value1.Type == CorInfoType.Class)
                    {
                        _opStack.Push(ObjectInstance.FromClrObject((value1 == value2) ? 1 : 0));
                    }
                    else if (value1.Type == CorInfoType.String)
                    {
                        _opStack.Push(ObjectInstance.FromClrObject((value1.ObjectRef == value2.ObjectRef) ? 1 : 0));
                    }
                    else
                    {
                        _opStack.Push(ObjectInstance.FromClrObject((value1.I == value2.I) ? 1 : 0));
                    }
                    continue;
                }
                else if (inst.OpCode == OpCodes.Cgt)
                {
                    var value2 = _opStack.Pop().I;
                    var value1 = _opStack.Pop().I;
                    _opStack.Push(ObjectInstance.FromClrObject((value1 > value2) ? 1 : 0));
                    continue;
                }
                else if (inst.OpCode == OpCodes.Cgt_Un)
                {
                    var value2 = _opStack.Pop();
                    var value1 = _opStack.Pop();

                    if (value1.Type == CorInfoType.Class)
                    {
                        _opStack.Push(ObjectInstance.FromClrObject((value1 != value2) ? 1 : 0));
                    }
                    else if (value1.Type == CorInfoType.String)
                    {
                        _opStack.Push(ObjectInstance.FromClrObject((value1.ObjectRef != value2.ObjectRef) ? 1 : 0));
                    }
                    else
                    {
                        _opStack.Push(ObjectInstance.FromClrObject(((ulong)value1.I > (ulong)value2.I) ? 1 : 0));
                    }

                    continue;
                }
                else if (inst.OpCode == OpCodes.Clt)
                {
                    var value2 = _opStack.Pop().I;
                    var value1 = _opStack.Pop().I;
                    _opStack.Push(ObjectInstance.FromClrObject((value1 < value2) ? 1 : 0));
                    continue;
                }
                else if (inst.OpCode == OpCodes.Clt_Un)
                {
                    var value2 = (ulong)_opStack.Pop().I;
                    var value1 = (ulong)_opStack.Pop().I;
                    _opStack.Push(ObjectInstance.FromClrObject((value1 < value2) ? 1 : 0));
                    continue;
                }

                else if (inst.OpCode == OpCodes.Throw)
                {
                    Throw();
                    continue;
                }

                throw new InazumaExecutionException("Unknown OpCode: " + inst.OpCode.ToString());
            }
        }

        private void StObj()
        {
            throw ThrowHelper.NotImplementedYet;
        }

        private void StSFld()
        {
            var inst = _instructions[_instructionPtr];
            var fld = ((FieldDefinition)inst.Operand).Resolve();
            var mt = _classLoader.LoadTypeFromTypeRef(fld.DeclaringType);
            var fldDesc = _classLoader.LookupFieldDescFromFieldDef(fld);

            var value = _opStack.Pop();

            mt.StaticFields[fldDesc.Offset] = value.GetInstanceOrCopiedValue();
        }

        private void LdSFldA()
        {
            var inst = _instructions[_instructionPtr];
            var fld = ((FieldDefinition)inst.Operand).Resolve();
            var mt = _classLoader.LoadTypeFromTypeRef(fld.DeclaringType);
            var fldDesc = _classLoader.LookupFieldDescFromFieldDef(fld);

            _opStack.Push(mt.StaticFields[fldDesc.Offset]); // static field addr // TODO: as ref
        }

        private void LdSFld()
        {
            var inst = _instructions[_instructionPtr];
            var fld = ((FieldDefinition)inst.Operand).Resolve();
            var mt = _classLoader.LoadTypeFromTypeRef(fld.DeclaringType);
            var fldDesc = _classLoader.LookupFieldDescFromFieldDef(fld);

            _opStack.Push(mt.StaticFields[fldDesc.Offset].GetInstanceOrCopiedValue());
        }

        private void StFld()
        {
            var inst = _instructions[_instructionPtr];
            var fld = ((FieldDefinition) inst.Operand).Resolve();

            var value = _opStack.Pop();
            var target = _opStack.Pop(); // addrCit
            var fieldDesc = target.MethodTable.EEClass.LookupFieldDesc(fld);

            if (target.Type == CorInfoType.Class)
            {
                if (value.Type == CorInfoType.Class)
                {
                    // TODO: 
                    target.FieldInstances[fieldDesc.Offset] = value;
                }
                else if (value.Type == CorInfoType.ValueClass)
                {
                    target.FieldInstances[fieldDesc.Offset] = value.GetInstanceOrCopiedValue();
                    throw ThrowHelper.NotImplementedYet;
                }
                else
                {
                    target.FieldInstances[fieldDesc.Offset] = value.GetInstanceOrCopiedValue(); // copy
                }
            }
            else
            {
                throw ThrowHelper.NotImplementedYet;
            }
        }

        private void LdFldA()
        {
            throw ThrowHelper.NotImplementedYet;
        }

        private void LdFld()
        {
            var inst = _instructions[_instructionPtr];
            var fld = ((FieldDefinition)inst.Operand).Resolve();

            var target = _opStack.Pop();
            var fieldDesc = target.MethodTable.EEClass.LookupFieldDesc(fld);
            var value = target.FieldInstances[fieldDesc.Offset];
            _opStack.Push(value.GetInstanceOrCopiedValue());
        }

        private void NewObj()
        {
            var inst = _instructions[_instructionPtr];
            var memberRef = ((MethodReference)inst.Operand);
            var type = _classLoader.LoadTypeFromTypeRef(memberRef.DeclaringType);
            var resolvedCtor = memberRef.Resolve();

            if (resolvedCtor.IsStatic || resolvedCtor.IsAbstract)
            {
                ThrowHelper.VerificationError("newobj on static or abstract method");
            }

            // There are four cases:
            // 1) Value types (ordinary constructor, resulting VALUECLASS pushed)
            // 2) String (var-args constructor, result automatically pushed)
            // 3) MDArray (var-args constructor, resulting OBJECTREF pushed)
            // 4) Reference types (ordinary constructor, resulting OBJECTREF pushed)
            var classInfo = memberRef.DeclaringType;
            if (classInfo.IsValueType)
            {
                // Value type
                throw ThrowHelper.NotImplementedYet;
            }
            else if (classInfo.Namespace == "System" && classInfo.Name == "String") // TODO: move to method table
            {
                // For a VAROBJSIZE class (currently == String), pass NULL as this to "pseudo-constructor."
                throw ThrowHelper.NotImplementedYet;
            }
            else
            {
                ObjectInstance thisArgObj = null;
                if (classInfo.IsArray)
                {
                    // Array
                    throw ThrowHelper.NotImplementedYet;
                }
                else
                {
                    // Reference
                    thisArgObj = PetitClrRuntime.AllocateObject(type);
                    DoCallWork(false, thisArgObj, resolvedCtor);
                }

                _opStack.Push(thisArgObj);
            }
        }

        private void Add(bool checkOverflow, bool unsigned)
        {
            var val2 = _opStack.Pop();
            var val1 = _opStack.Pop();

            // TODO: float/double
            // TODO: overflow
            var type = CorInfoType.Int;
            if (unsigned)
            {
                var v1 = (uint)val1.I;
                var v2 = (uint)val2.I;
                _opStack.Push(new ObjectInstance { I = v1 + v2, Type = type });
            }
            else
            {
                var v1 = (int)val1.I;
                var v2 = (int)val2.I;
                _opStack.Push(new ObjectInstance { I = v1 + v2, Type = type });
            }
        }
        private void Sub(bool checkOverflow, bool unsigned)
        {
            var val2 = _opStack.Pop();
            var val1 = _opStack.Pop();

            // TODO: float/double
            // TODO: overflow
            var type = CorInfoType.Int;
            if (unsigned)
            {
                var v1 = (uint)val1.I;
                var v2 = (uint)val2.I;
                _opStack.Push(new ObjectInstance { I = v1 - v2, Type = type });
            }
            else
            {
                var v1 = (int)val1.I;
                var v2 = (int)val2.I;
                _opStack.Push(new ObjectInstance { I = v1 - v2, Type = type });
            }
        }
        private void Mul(bool checkOverflow, bool unsigned)
        {
            var val2 = _opStack.Pop();
            var val1 = _opStack.Pop();

            // TODO: float/double
            // TODO: overflow
            var type = CorInfoType.Int;
            if (unsigned)
            {
                var v1 = (int)val1.I;
                var v2 = (int)val2.I;
                _opStack.Push(new ObjectInstance { I = v1 * v2, Type = type });
            }
            else
            {
                var v1 = (uint)val1.I;
                var v2 = (uint)val2.I;
                _opStack.Push(new ObjectInstance { I = v1 * v2, Type = type });
            }
        }
        private void Div(bool checkOverflow, bool unsigned)
        {
            var val2 = _opStack.Pop();
            var val1 = _opStack.Pop();

            // TODO: float/double
            // TODO: overflow
            var type = CorInfoType.Int;
            if (unsigned)
            {
                var v1 = (uint)val1.I;
                var v2 = (uint)val2.I;
                _opStack.Push(new ObjectInstance { I = v1 / v2, Type = type });
            }
            else
            {
                var v1 = (int)val1.I;
                var v2 = (int)val2.I;
                _opStack.Push(new ObjectInstance { I = v1 / v2, Type = type });
            }
        }
        private void Rem(bool checkOverflow, bool unsigned)
        {
            var val2 = _opStack.Pop();
            var val1 = _opStack.Pop();

            // TODO: float/double
            // TODO: overflow
            var type = CorInfoType.Int;
            if (unsigned)
            {
                var v1 = (uint)val1.I;
                var v2 = (uint)val2.I;
                _opStack.Push(new ObjectInstance { I = v1 % v2, Type = type });
            }
            else
            {
                var v1 = (int)val1.I;
                var v2 = (int)val2.I;
                _opStack.Push(new ObjectInstance { I = v1 % v2, Type = type });
            }
        }
        private void LdArg(int argNum)
        {
            var tmpValue = _args[argNum].GetInstanceOrCopiedValue();
            _opStack.Push(tmpValue);
        }

        private void LdArgA(int argNum)
        {
            _opStack.Push(_args[argNum]);
        }

        private void LdLoc(int argNum)
        {
            _opStack.Push(_localSlot[argNum]);
        }

        private void LdLocA(int argNum)
        {
            throw ThrowHelper.NotImplementedYet;
        }

        private void StLoc(int argNum)
        {
            var value = _opStack.Peek();
            _localSlot[argNum] = value;
            _opStack.Pop();
        }

        private void StArg(int argNum)
        {
            throw ThrowHelper.NotImplementedYet;
        }

        private void Ldnull()
        {
            _opStack.Push(ObjectInstance.Null);
        }
        private void LdIcon(int c)
        {
            _opStack.Push(new ObjectInstance { I = c, Type = CorInfoType.Int });
        }
        private void LdIcon(object c)
        {
            if (c is Int32)
            {
                LdIcon((int)c);
            }
            if (c is SByte)
            {
                LdIcon((int)(SByte)c);
            }
        }

        private void LdStr()
        {
            _opStack.Push(new ObjectInstance { ObjectRef = _instructions[_instructionPtr].Operand as String, Type = CorInfoType.String });
        }

        private void Call(bool virtualCall)
        {
            DoCallWork(virtualCall);
        }

        private void DoCallWork(bool virtualCall, ObjectInstance thisArg = null, MethodReference methTok = null, object callInfo = null)
        {
            var op = (methTok ?? _instructions[_instructionPtr].Operand) as MethodReference;

            var methDef = op.Resolve();
            {
                if (methDef.IsInternalCall)
                {
                    InvokeInternalCall(methDef);
                    return;
                }
            }
            _callThisArg = thisArg; // for .ctor

            var totalSigArgs = 0;
            if (false)
            {
                
            }
            else
            {
                totalSigArgs = methDef.Parameters.Count + (methDef.HasThis ? 1 : 0);
            }

            // Note that "totalNativeArgs()" includes space for ret buff arg.
            var nSlots = totalSigArgs + 1;
            if (methDef.HasGenericParameters) nSlots++;
            if (methDef.IsVarArg()) nSlots++;

            // Make sure that the operand stack has the required number of arguments.
            // (Note that this is IL args, not native.)
            // 

            // The total number of arguments on the IL stack.  Initially we assume that all the IL arguments
            // the callee expects are on the stack, but may be adjusted downwards if the "this" argument
            // is provided by an allocation (the call is to a constructor).
            var totalArgsOnILStack = totalSigArgs;
            if (_callThisArg != null)
            {
                Debug.Assert(totalArgsOnILStack > 0);
                totalArgsOnILStack--;
            }

            var totalArgs = nSlots;
            var LOCAL_ARG_SLOTS = 8;
            var localArgs = new ObjectInstance[(totalArgs > LOCAL_ARG_SLOTS) ? totalArgs : LOCAL_ARG_SLOTS];
            // Current on-stack argument index.
            var arg = 0;

            // FIXME: stack (mayuki)
            var tmpArgsStack = _opStack.ToArray();
            var curArgSlot = 0;
            if (methDef.HasThis)
            {
                if (_callThisArg != null)
                {
                    localArgs[curArgSlot] = _callThisArg;
                }
                else
                {
                    localArgs[curArgSlot] = tmpArgsStack[tmpArgsStack.Length - (arg+1)];
                    arg++;
                }
                curArgSlot++;
            }

            // Now we do the non-this arguments.
            for (; arg < totalArgsOnILStack; arg++)
            {
                localArgs[curArgSlot] = tmpArgsStack[tmpArgsStack.Length - (arg + 1)];
                curArgSlot++;
            }

            if (methDef.HasThis)
            {
                if (thisArg == null)
                {
                    thisArg = tmpArgsStack[0];
                }
                else
                {
                    thisArg = _callThisArg;
                }
            }

            ObjectInstance retVal;
            MethodDefinition exactMethToCall = methDef;
            if (methDef.DeclaringType.IsInterface)
            {
                var slot = thisArg.MethodTable.InterfaceMethodSlotMap[methDef];
                exactMethToCall = thisArg.MethodTable.MethodSlots[slot].Definition;
            }
            else
            {
                if (virtualCall && methDef.IsVirtual)
                {
                    var methodDesc = _classLoader.LookupMethodDescFromMethodDef(methDef);
                    exactMethToCall = thisArg.MethodTable.MethodSlots[methodDesc.Slot].Definition;
                }
            }

            retVal = InterpretMethodBody(exactMethToCall, true, localArgs, null);

            // retval
            for (var i = 0; i < totalArgsOnILStack; i++)
            {
                _opStack.Pop();
            }

            if (methDef.ReturnType.FullName != "System.Void") // TODO: should refer typevalue enum
            {
                _opStack.Push(retVal);
            }
        }

        private void InvokeInternalCall(MethodDefinition methDef)
        {
            object retVal;
            var args = new object[methDef.Parameters.Count];
            for (var i = args.Length - 1; i >= 0; i--)
            {
                args[i] = _opStack.Pop().ToClrObject();
            }

            if (PetitClrRuntime.Current.InternalCallMethods.ContainsKey(methDef.ToString()))
            {
                var func = PetitClrRuntime.Current.InternalCallMethods[methDef.ToString()];
                retVal = func(_callThisArg, PetitClrThread.CurrentThread.Frame, args);
            }
            else
            {
                var realType = Type.GetType(methDef.DeclaringType.FullName.Replace("Inazuma.PetitClr.PetitCorlib.", ""));
                var realMethod = realType.GetMethod(methDef.Name, BindingFlags.Static | BindingFlags.Public, null, methDef.Parameters.Select(x => Type.GetType(x.ParameterType.FullName)).ToArray(), null);
                if (realMethod == null)
                {
                    throw new InazumaExecutionException("Could not find the internal method: " + methDef.FullName);
                }

                retVal = realMethod.Invoke(null, args);
            }

            if (methDef.ReturnType.FullName != "System.Void") // TODO: should refer typevalue
            {
                _opStack.Push(ObjectInstance.FromClrObject(retVal));
            }
        }

        private void Throw()
        {
            var ex = _opStack.Pop();
            throw new InazumaRuntimeException(ex);
        }
    }
}
