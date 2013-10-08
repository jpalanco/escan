using System;
using System.Collections.Generic;
using System.Text;

using Mono.Cecil.Cil;
using Mono.Cecil;

namespace Escan.Engine
{
    public interface ICilNode
    {
        Instruction Instr { get; }
        MethodDefinition Method { get; }
        string ClassName { get; }

        //FIXME: do we need this in the interface?
        List<Instruction> callBy(MethodDefinition method);
    }
}
