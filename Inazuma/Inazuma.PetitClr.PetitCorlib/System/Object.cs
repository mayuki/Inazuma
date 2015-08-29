using System;
using System.Collections.Generic;

namespace System
{
    public class Object
    {
        public Object() { }

        public virtual string ToString()
        {
            // TODO: not implemented
            return "System.Object";
        }

        public virtual bool Equals(Object obj)
        {
            throw new NotImplementedException();
        }

        public virtual int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
