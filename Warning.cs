using System;
using System.Collections.Generic;
using System.Text;

namespace Escan.Engine
{
    public class Warning
    {
        public InitialInput wInp;
        public Input wOut;

        public Warning(InitialInput wInp, Input wOut) {
            this.wInp = wInp;
            this.wOut = wOut;
        }
    }
}
