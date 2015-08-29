using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inazuma.PetitClr
{
    public class InazumaExecutionException : Exception
    {
        public InazumaExecutionException()
            : base()
        {
        }
        public InazumaExecutionException(string message)
            : base(message)
        {
        }
        public InazumaExecutionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
