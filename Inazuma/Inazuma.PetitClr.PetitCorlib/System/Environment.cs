using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System
{
    public static class Environment
    {
        public static string GetResourceString(string sr, string arg0 = null)
        {
            // TODO: not implemented yet
            return sr;
        }

        /*==================================StackTrace==================================
         **Action:
         **Returns:
         **Arguments:
         **Exceptions:
         ==============================================================================*/
        public static String StackTrace
        {
            get
            {
                return GetStackTrace(null, true);
            }
        }

        internal static String GetStackTrace(Exception e, bool needFileInfo)
        {
            return GetStackTraceInternal(e, needFileInfo);
        }
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static string GetStackTraceInternal(Exception e, bool needFileInfo);

        public const string NewLine = "\r\n";
    }
}
