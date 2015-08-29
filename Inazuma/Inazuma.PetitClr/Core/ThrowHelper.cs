using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inazuma.PetitClr.Core
{
    internal class ThrowHelper
    {
        public static NotImplementedException NotImplementedYet = new NotImplementedException("Not implemented yet");
        public static NotImplementedException NotImplementedByDesign = new NotImplementedException("Not implemented by design");

        public static void ComPlusThrow(Exception exception)
        {
            throw (exception);
        }

        public static void VerificationError(string message)
        {
            throw new InazumaExecutionException("VerificationError: " + message);
        }
    }
}
