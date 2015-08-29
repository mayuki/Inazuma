using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace System
{
    public class Exception
    {
        public virtual string Message { get { return _message; } } // TODO: not implemented

        public Exception()
        {
            // TODO: not implemented
        }

        public Exception(String message)
        {
            // TODO: not implemented
            _message = message;
        }

        // Creates a new Exception.  All derived classes should 
        // provide this constructor.
        // Note: the stack trace is not started until the exception 
        // is thrown
        // 
        public Exception(String message, Exception innerException)
        {
            // TODO: not implemented
        }

        protected Exception(SerializationInfo info, StreamingContext context)
        {
            // TODO: not implemented
        }

        internal static string GetMessageFromNativeResources(ExceptionMessageKind kind)
        {
            return kind.ToString();
        }

        internal void SetErrorCode(int hr)
        {
            // TODO: not implemented
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // TODO: not implemented
        }

        public override String ToString()
        {
            return ToString(true, true);
        }

        private String ToString(bool needFileLineInfo, bool needMessage)
        {
            Debugger.Break();

            String message = (needMessage ? Message : null);
            String s;

            if (message == null || message.Length <= 0)
            {
                s = GetClassName();
            }
            else
            {
                s = GetClassName() + ": " + message;
            }

            if (_innerException != null)
            {
                s = s + " ---> " + _innerException.ToString(needFileLineInfo, needMessage) + Environment.NewLine +
                "   " + Environment.GetResourceString("Exception_EndOfInnerExceptionStack");

            }

            string stackTrace = GetStackTrace(needFileLineInfo);
            if (stackTrace != null)
            {
                s += Environment.NewLine + stackTrace;
            }

            return s;
        }

        // Computes and returns the stack trace as a string
        // Attempts to get source file and line number information if needFileInfo
        // is true.  Note that this requires FileIOPermission(PathDiscovery), and so
        // will usually fail in CoreCLR.  To avoid the demand and resulting
        // SecurityException we can explicitly not even try to get fileinfo.
        private string GetStackTrace(bool needFileInfo)
        {
            string stackTraceString = _stackTraceString;
            string remoteStackTraceString = _remoteStackTraceString;

            // if no stack trace, try to get one
            if (stackTraceString != null)
            {
                return remoteStackTraceString + stackTraceString;
            }
            if (_stackTrace == null)
            {
                return remoteStackTraceString;
            }

            // Obtain the stack trace string. Note that since Environment.GetStackTrace
            // will add the path to the source file if the PDB is present and a demand
            // for FileIOPermission(PathDiscovery) succeeds, we need to make sure we 
            // don't store the stack trace string in the _stackTraceString member variable.
            System.Diagnostics.Debugger.Break();
            String tempStackTraceString = Environment.GetStackTrace(this, needFileInfo);
            return remoteStackTraceString + tempStackTraceString;
        }

        private string GetClassName()
        {
            return "[NOT_IMPLEMENTED]"; // TODO: not implemented yet
        }


        private String _className;  //Needed for serialization.  
        //private MethodBase _exceptionMethod;  //Needed for serialization.  
        private String _exceptionMethodString; //Needed for serialization. 
        internal String _message;
        //private IDictionary _data;
        private Exception _innerException;
        private String _helpURL;
        private Object _stackTrace;
        //[OptionalField] // This isnt present in pre-V4 exception objects that would be serialized.
        private Object _watsonBuckets;
        private String _stackTraceString; //Needed for serialization.  
        private String _remoteStackTraceString;
        private int _remoteStackIndex;
#pragma warning disable 414  // Field is not used from managed.
        // _dynamicMethods is an array of System.Resolver objects, used to keep
        // DynamicMethodDescs alive for the lifetime of the exception. We do this because
        // the _stackTrace field holds MethodDescs, and a DynamicMethodDesc can be destroyed
        // unless a System.Resolver object roots it.
        private Object _dynamicMethods;
#pragma warning restore 414

        // @MANAGED: HResult is used from within the EE!  Rename with care - check VM directory
        internal int _HResult;     // HResult

        public int HResult
        {
            get
            {
                return _HResult;
            }
            protected set
            {
                _HResult = value;
            }
        }

        private String _source;         // Mainly used by VB. 
        // WARNING: Don't delete/rename _xptrs and _xcode - used by functions
        // on Marshal class.  Native functions are in COMUtilNative.cpp & AppDomain
        private IntPtr _xptrs;             // Internal EE stuff 
#pragma warning disable 414  // Field is not used from managed.
        private int _xcode;             // Internal EE stuff 
#pragma warning restore 414
        //[OptionalField]
        private UIntPtr _ipForWatsonBuckets; // Used to persist the IP for Watson Bucketing

#if FEATURE_SERIALIZATION
        [OptionalField(VersionAdded = 4)]
        private SafeSerializationManager _safeSerializationManager;
#endif // FEATURE_SERIALIZATION

        // See clr\src\vm\excep.h's EXCEPTION_COMPLUS definition:
        private const int _COMPlusExceptionCode = unchecked((int)0xe0434352);   // Win32 exception code for COM+ exceptions
    }

    // This piece of infrastructure exists to help avoid deadlocks 
    // between parts of mscorlib that might throw an exception while 
    // holding a lock that are also used by mscorlib's ResourceManager
    // instance.  As a special case of code that may throw while holding
    // a lock, we also need to fix our asynchronous exceptions to use
    // Win32 resources as well (assuming we ever call a managed 
    // constructor on instances of them).  We should grow this set of
    // exception messages as we discover problems, then move the resources
    // involved to native code.
    internal enum ExceptionMessageKind
    {
        ThreadAbort = 1,
        ThreadInterrupted = 2,
        OutOfMemory = 3
    }
}
