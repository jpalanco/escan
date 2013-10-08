using System;
using System.Collections.Generic;
using System.Text;

namespace Escan.Engine
{
    public class Sink
    {
        public string sNamespace;
        public string sName;
        public string sMethod;


        public Sink(string sNamespace, string sName, string sMethod)
        {
            this.sNamespace = sNamespace;
            this.sName = sName;
            this.sMethod = sMethod;
        }

        public override bool Equals(object o)
        {

            if (o == null)
            {
                return this == null;
            }
            else
            {
                return (o is Sink) &&
                       (this.sName == ((Sink)o).sName) &&
                       (this.sNamespace == ((Sink)o).sNamespace) &&
                       (this.sMethod == ((Sink)o).sMethod);
            }
        }


    }
}
