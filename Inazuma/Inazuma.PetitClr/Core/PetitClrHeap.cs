using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inazuma.PetitClr.Core.Structure;

namespace Inazuma.PetitClr.Core
{
    public class PetitClrHeap
    {
        private Dictionary<int, ObjectInstance> _heapMemory;
        private int _ptr= 0x0;

        public const int InvalidPointer = 0x0;

        public Dictionary<int, ObjectInstance> HeapMemory { get { return _heapMemory; } }

        public PetitClrHeap()
        {
            _heapMemory = new Dictionary<int, ObjectInstance>();
        }

        public ObjectInstance Alloc()
        {
            _heapMemory[++_ptr] = new ObjectInstance();

            return _heapMemory[_ptr];
        }

        public void Free(int ptr)
        {
            _heapMemory.Remove(ptr);
        }
    }
}
