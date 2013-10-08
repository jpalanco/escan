using System;
using System.Collections.Generic;
using System.Text;

namespace Escan.Engine
{
    class Source
    {
        public string sNamespace;
        public string sName;
        public string sMethod;
        //public string category;

        public Source(string sNamespace, string sName, string sMethod)
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
                return (o is Source) && 
					   (this.sName == ((Source)o).sName) &&
					   (this.sNamespace == ((Source)o).sNamespace) &&
					   (this.sMethod == ((Source)o).sMethod);
			}
        }
    }
}
    
    
