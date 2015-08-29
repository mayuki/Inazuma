using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inazuma.Mono.Cecil;

namespace Inazuma.PetitClr.Core.Structure
{
    public class InterpreterFrame : Frame
    {
        private PetitClrInterpreter _interp;

        public InterpreterFrame(PetitClrInterpreter interp)
        {
            _interp = interp;

            Push();
        }

        public override MethodDefinition /* MethodDesc */ Method
        {
            get { return _interp.MethodInfo; }
        }
    }

    public abstract class Frame
    {
        public Frame Next { get; private set; }

        public PetitClrThread Thread
        {
            get { return PetitClrThread.CurrentThread; }
        }
        public abstract MethodDefinition /* MethodDesc */ Method { get; }

        public void Push(PetitClrThread thread)
        {
            Next = thread.Frame;

            thread.Frame = this;
        }

        public void Pop(PetitClrThread thread)
        {
            thread.Frame = Next;
        }

        public void Push()
        {
            Push(Thread);
        }

        public void Pop()
        {
            Pop(Thread);
        }

    }
}
