using System;
using System.Collections.Generic;
using System.Text;

using Mono.Cecil.Cil;

namespace Escan.Engine
{
    internal class OpCode
    {
        internal static bool IsACallInstruction(Instruction oInstr)
        {
            return oInstr.OpCode == OpCodes.Call ||
                   oInstr.OpCode == OpCodes.Calli ||
                   oInstr.OpCode == OpCodes.Callvirt ||
                   oInstr.OpCode == OpCodes.Newobj;
            //FIXME: we should study this case:
            // || anInstruction.OpCode == OpCodes.Newarr;
        }

        internal static bool IsAPopInstruction(Instruction oInstr)
        {
            return oInstr.OpCode == OpCodes.Pop;
        }

        //for arrays/vectors
        internal static bool IsAStelemRef(Instruction oInstr)
        {
            return oInstr.OpCode == OpCodes.Stelem_Ref;
            //TODO: perhaps there are more
        }

        //Para saber si una instruccion es un ldarg
        internal static bool IsALdargInstruction(Instruction oInstr)
        {
            return oInstr.OpCode == OpCodes.Ldarg ||
                   oInstr.OpCode == OpCodes.Ldarg_0 ||
                   oInstr.OpCode == OpCodes.Ldarg_1 ||
                   oInstr.OpCode == OpCodes.Ldarg_2 ||
                   oInstr.OpCode == OpCodes.Ldarg_3 ||
                   oInstr.OpCode == OpCodes.Ldarg_S;
            //FIXME: what is OpCodes.Ldarga?
        }

        internal static bool IsAStlocInstruction(Instruction oInstr)
        {
            return oInstr.OpCode == OpCodes.Stloc ||
                   oInstr.OpCode == OpCodes.Stloc_0 ||
                   oInstr.OpCode == OpCodes.Stloc_1 ||
                   oInstr.OpCode == OpCodes.Stloc_2 ||
                   oInstr.OpCode == OpCodes.Stloc_3 ||
                   oInstr.OpCode == OpCodes.Stloc_S;
        }

        internal static bool IsARet(Instruction oInstr)
        {
            return oInstr.Next.OpCode == OpCodes.Ret;
        }
    }
}
