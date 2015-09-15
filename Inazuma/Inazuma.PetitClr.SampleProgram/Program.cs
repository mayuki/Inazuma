using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inazuma.PetitClr.Core;
using Inazuma.Mono.Cecil;
using Inazuma.PetitClr.Core.Loader;
using Inazuma.PetitClr.Core.Structure;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Inazuma.PetitClr.SampleHost
{
    class Program
    {
        static void Main(string[] args)
        {

            // Setup InternalCallMethods
            PetitClrRuntime.Current.InternalCallMethods.Add("System.String System.Environment::GetStackTraceInternal(System.Exception,System.Boolean)", (thisArg, frame, methArgs) =>
            {
                return "HogeMoge";
            });
            PetitClrRuntime.Current.InternalCallMethods.Add("System.Void System.Console::Write(System.String)", (thisArg, frame, methArgs) =>
            {
                System.Console.Write(methArgs[0]);
                return null;
            });
            PetitClrRuntime.Current.InternalCallMethods.Add("System.Void System.Console::Write(System.String,System.Object)", (thisArg, frame, methArgs) =>
            {
                System.Console.Write(methArgs[0].ToString(), methArgs[1]);
                return null;
            });
            PetitClrRuntime.Current.InternalCallMethods.Add("System.Void System.Console::Write(System.Int32)", (thisArg, frame, methArgs) =>
            {
                System.Console.Write(methArgs[0]);
                return null;
            });
            PetitClrRuntime.Current.InternalCallMethods.Add("System.Void System.Console::WriteLine()", (thisArg, frame, methArgs) =>
            {
                System.Console.WriteLine();
                return null;
            });
            PetitClrRuntime.Current.InternalCallMethods.Add("System.Void System.Console::WriteLine(System.String)", (thisArg, frame, methArgs) =>
            {
                System.Console.WriteLine(methArgs[0]);
                return null;
            });
            PetitClrRuntime.Current.InternalCallMethods.Add("System.Void System.Console::WriteLine(System.String,System.Object)", (thisArg, frame, methArgs) =>
            {
                System.Console.WriteLine(methArgs[0].ToString(), methArgs[1]);
                return null;
            });
            PetitClrRuntime.Current.InternalCallMethods.Add("System.Void System.Console::WriteLine(System.Int32)", (thisArg, frame, methArgs) =>
            {
                System.Console.WriteLine(methArgs[0]);
                return null;
            });
            PetitClrRuntime.Current.InternalCallMethods.Add("System.Void System.Diagnostics.Debugger::Break()", (thisArg, frame, methArgs) =>
            {
                Debugger.Break();
                return null;
            });

            // Load Assembly & Get EntryPoint
            var assemblyResolver = new InazumaAssemblyResolver();
            var classLoader = new ClassLoader();
            var readerparameters = new ReaderParameters() { AssemblyResolver = assemblyResolver, MetadataResolver = new InazumaMetadataResolver(assemblyResolver) };
            InazumaAssemblyResolver.PetitCorLib = AssemblyDefinition.ReadAssembly("../../../Inazuma.PetitClr.PetitCorlib/Bin/Debug/Inazuma.PetitClr.PetitCorlib.dll", readerparameters); // = mscorlibAssembly
            var asm = AssemblyDefinition.ReadAssembly("Inazuma.PetitClr.SampleHost.exe", readerparameters);
            var entryPointType = asm.MainModule.GetType("Inazuma.PetitClr.SampleHost.SampleProgram");

            // Execute Program
            bool doJmpCall;
            PetitClrInterpreter.ExecuteMethodWrapper(classLoader, entryPointType.Methods[0], false, new List<ObjectInstance>(), null, out doJmpCall);

            System.Console.ReadLine();
        }
    }

    class InazumaMetadataResolver : MetadataResolver
    {
        public InazumaMetadataResolver(IAssemblyResolver assemblyResolver)
            : base(assemblyResolver)
        {
        }

        public override MethodDefinition Resolve(MethodReference method)
        {
            return base.Resolve(method);
        }
    }

    class InazumaAssemblyResolver : DefaultAssemblyResolver
    {
        public static AssemblyDefinition PetitCorLib;

        public override AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            if (name.Name == "mscorlib" || name.Name == "System.Core" || name.Name == "System")
            {
                return PetitCorLib;
            }
            else
            {
                return base.Resolve(name, parameters);
            }
        }

        public override AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
        {
            return base.Resolve(fullName, parameters);
        }
    }
}
