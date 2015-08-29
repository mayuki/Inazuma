using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inazuma.PetitClr.Core.Structure;

namespace Inazuma.PetitClr.Core
{
    public class PetitClrThread
    {
        private static PetitClrThread _mainThread = new PetitClrThread();

        // TODO: multi-thread
        public static PetitClrThread CurrentThread
        {
            get { return _mainThread; }
        }

        public Frame Frame { get; set; }
    }
}
