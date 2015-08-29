using Inazuma.PetitClr.Core.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inazuma.PetitClr.Core
{
    public class InazumaRuntimeException : Exception
    {
        public ObjectInstance ExceptionObject;

        public InazumaRuntimeException(ObjectInstance exception)
            : base("Runtime Exception")
        {
            ExceptionObject = exception;
        }
    }
}
