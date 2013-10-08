using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil.Cil;
using Mono.Cecil;

namespace Escan.Engine
{
    public class Input : ICilNode
    {
        private void InitializeInput(string sClassName, MethodDefinition method, Instruction inst) {
            this.sClassName = sClassName;
            this.oMethod = method;
            this.oInstr = inst;
        }

        
        /* Se le pasa una instruccion que esta llamando a un metodo con una de nuestras inputs
         *  y devuelve una instancia de MethodDefinition para poder trabajar dentro del nuevo
         * metodo */
        internal Input(Instruction inst, MethodDefinition method)
        {
            if (!OpCode.IsACallInstruction(inst) || !(inst.Operand is MethodReference))
            {
                throw new NotSupportedException("This case is not supported");
            }
            this.InitializeInput(((MethodReference)inst.Operand).DeclaringType.Name, method, inst);
        }

        protected Instruction oInstr;
        protected MethodDefinition oMethod;
        protected string sClassName;

        public Instruction Instr
        {
            get { return this.oInstr; }
            set { this.oInstr = value; }
        }

        public MethodDefinition Method
        {
            get { return this.oMethod; }
            set { this.oMethod = value; }
        }

        public string ClassName
        {
            get { return this.sClassName; }
            set { this.sClassName = value; }
        }

        //internal Input(string sClassName, MethodDefinition method, Instruction inst) {
        //}


        //Se le pasa un methoddefinition y un opcode y devuelve las instrucciones donde se guardan
        public List<Instruction> callBy(MethodDefinition method)
        {
            List<Instruction> aCallers = new List<Instruction>();
            if (Engine.getVar(this.Instr).OpCode.Name == "stloc.s")
            {
                string V_ = Convert.ToString(Engine.getVar(this.Instr).Operand);
                aCallers = Engine.getldlocS(V_, method);
            }

            if (OpCode.IsALdargInstruction(this.Instr))
            {
                foreach (Instruction instruccion in method.Body.Instructions)
                {
                    if (instruccion.OpCode == this.Instr.OpCode)
                    {
                        Console.WriteLine("Se llama desde " + Engine.detectCaller(instruccion).Operand.ToString() + " a " + Engine.getVar(this.Instr).OpCode.Name);
                        aCallers.Add(Engine.detectCaller(instruccion));
                        return aCallers;
                    }
                }
            }
            else
            {
                foreach (Instruction instruccion in method.Body.Instructions)
                {
                    try
                    {
                        if (instruccion.OpCode.Name == Engine.stTold(Engine.getVar(this.Instr).OpCode.Name))
                        {
                            Console.WriteLine("Se llama desde " + Engine.detectCaller(instruccion).Operand.ToString() + " a " + Engine.getVar(this.Instr).OpCode.Name);
                            //isNative(detectCaller(instruccion));
                            aCallers.Add(Engine.detectCaller(instruccion));
                        }
                    }
                    catch (Exception e)
                    {
                        return null;
                    }
                }
            }
            return aCallers;
        }

    }
}
