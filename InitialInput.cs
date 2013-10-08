using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using QuickGraph;

namespace Escan.Engine
{
    public class InitialInput : Input, ICilNode
    {
        private AdjacencyGraph<ICilNode, Edge<ICilNode>> oTree;

        internal InitialInput(MethodDefinition method, Instruction inst) : base(inst, method)
        {
            this.oTree = new AdjacencyGraph<ICilNode, Edge<ICilNode>>();
        }


        internal void FillTree()
        {
            this.FillTree(null, null, null);
        }

        //Metodo recursivo para seguir el flujo de las variables a traves de los metodos
        private void FillTree(ICilNode oCurrentNode, MethodDefinition oCurrentMethod, Instruction oInstr)
        {
            if (oCurrentNode == null)
            {
                this.oTree.AddVertex(this);
                oCurrentNode = this;
                oCurrentMethod = this.oMethod;
                oInstr = this.oInstr;
            }

            if (OpCode.IsAStlocInstruction(oInstr.Next) || (OpCode.IsALdargInstruction(oInstr)))
            {
                Console.WriteLine("es variable");
                List<Instruction> aCallers = oCurrentNode.callBy(oCurrentMethod);

                if (aCallers != null)
                {
                    //Console.WriteLine("Hemos entrado");
                    foreach (Instruction newinst in callBy(oCurrentMethod))
                    {
                        Console.WriteLine("Se trata de " + Engine.getVar(oInstr).OpCode.Name);
                        Console.WriteLine("NEWINST es " + newinst.OpCode.Name + " " + newinst.Operand.ToString());
                        if (Engine.detectModifications(Engine.getVar(oInstr), newinst))
                        {
                            Console.WriteLine("*******DETECTADA MODIFICACION*****");
                            continue;
                        }
                        //Console.WriteLine(newinst.Operand.ToString());
                        Input newVertex = new Input(newinst, oCurrentMethod);

                        //   INode<Input> leaf = node.AddChild(tmpInput);
                        this.oTree.AddVertex(newVertex);
                        Edge<ICilNode> e1 = new Edge<ICilNode>(oCurrentNode, newVertex);
                        this.oTree.AddEdge(e1);

                        //Parcheado para detectar el final del archivo que siempre da null
                        //hAY QUE MIRAR PORQUE NO FUNCIONA CON ALGUNAS DEL FINAL

                        if (Engine.isNative(newinst))
                        {
                            Engine.getCallArgs(newinst);
                            Console.WriteLine("estamos en " + newinst.OpCode.Name);
                            Console.WriteLine("ESTE METODO NOS TENEMOS QUE METER " + newinst.Operand.ToString());
                            Console.WriteLine("NUMERO DE ARGUMENTO " + Engine.getNumberArgument(newinst));
                            Console.WriteLine("CON LA VARIABLE " + Engine.getVar(oCurrentNode.Instr).OpCode.Name);
                            Console.WriteLine("nos metemos en el metodo");
                            // MethodDefinition me = 
                            //   oAssembly.MainModule.Types[0].Methods.
                            string ldarg = Engine.getLdarg(Engine.getNumberArgument(newinst));
                            Console.WriteLine("LDARG es " + ldarg);
                            MethodDefinition newMethod = Engine.getMethodByCaller(newinst);
                            List<Instruction> lCallers = Engine.detectLdarg(newMethod, ldarg);
                            if (lCallers != null)
                            {
                                foreach (Instruction inside in lCallers)
                                {
                                    Console.WriteLine("Estamos dentro, la variable es " + ldarg + " dentro del metodo " + newMethod.Name + " se llama a ldard " + lCallers.Count + " veces");

                                    //Input tmp1 = makeInputFromInstruction(inside, newMethod);
                                    //INode<Input> leaf1 = leaf.AddChild(tmp1);
                                    //Input insideVertex = makeInputFromInstruction(inside, newMethod);
                                    //if (insideVertex != null)
                                    //{
                                    //   INode<Input> leaf = node.AddChild(tmpInput);
                                    //this.oTree.AddVertex(insideVertex);
                                    //MarkedEdge<Input, int> e2 = new MarkedEdge<Input, int>(newVertex, insideVertex, count);
                                    //this.oTree.AddEdge(e2);
                                    //count++;                                        
                                    this.FillTree(newVertex, newMethod, inside);
                                }
                            }
                        }

                    }
                }
                else
                {
                    Console.WriteLine("Es return");
                    Dictionary<Instruction, MethodDefinition> hCallers = Engine.searchReturnCallers(oCurrentMethod);

                    if (hCallers != null)
                    {
                        foreach (Instruction myInst in hCallers.Keys)
                        {
                            Console.WriteLine("El hastable es ");
                            FillTree(oCurrentNode, hCallers[oInstr], oInstr);
                        }
                    }
                }

            }
            //llena_arbol(newinst, method, newVertex);
            //Comprobar que el metodo no sea nativo de java

            if (Engine.isInstance(oInstr))
            {
                Console.WriteLine("Es instancia");

                if (Engine.detectCaller(oInstr) != null)
                {
                    Console.WriteLine("Hemos detectado el caller");
                    Instruction myCaller = Engine.detectCaller(oInstr);
                    //Console.WriteLine("Se llama desde " + inst.Next.OpCode.Name + " a " + inst.OpCode.Name);
                    Input newVertex = new Input(myCaller, oCurrentMethod);
                    this.oTree.AddVertex(newVertex);
                    Edge<ICilNode> oNewEdge = new Edge<ICilNode>(oCurrentNode, newVertex);
                    this.oTree.AddEdge(oNewEdge);
                    if (Engine.isNative(myCaller))
                    {
                        Console.WriteLine("Es nativo hay que analizar");
                    }
                }
                //getCallArgs(inst);

            }




            /*if (OpCode.IsALdargInstruction(inst))
             {
                 Console.WriteLine("ES ldarg");
                 foreach (Instruction newinst in callBy(inst, method))
                 {
                     Console.WriteLine("Se trata de " + getVar(inst).OpCode.Name);
                     Console.WriteLine("NEWINST es " + newinst.OpCode.Name);
                     //Console.WriteLine(newinst.Operand.ToString());
                     Input newVertex = makeInputFromInstruction(newinst, method);

                     //   INode<Input> leaf = node.AddChild(tmpInput);
                     this.oTree.AddVertex(newVertex);
                     MarkedEdge<Input, int> e1 = new MarkedEdge<Input, int>(vertex, newVertex, count);
                     this.oTree.AddEdge(e1);
                     count++;
                 }*/



            if (OpCode.IsARet(oInstr))
            {

                Console.WriteLine("Es valor de retorno");
                Dictionary<Instruction, MethodDefinition> hCallers = Engine.searchReturnCallers(oCurrentMethod);

                if (hCallers != null)
                {
                    foreach (Instruction myInst in hCallers.Keys)
                    {
                        Console.WriteLine("El hastable es ");
                        FillTree(oCurrentNode, hCallers[oInstr], oInstr);
                    }
                }
                //getCallArgs(inst);


            }

            /*
            if (isArgument(inst))
            {
                Console.WriteLine("Es argumento");

            }*/

        }

        internal List<Warning> DetectSink(List<Sink> aSinks)
        {
            List<Warning> aWarnings = new List<Warning>();
            foreach (ICilNode oInput in this.oTree.Vertices)
            {
                //reference.DeclaringType.Namespace, reference.DeclaringType.Name, reference.Name
                if (oInput.Instr.Operand is MethodReference)
                {
                    MethodReference reference = (MethodReference)oInput.Instr.Operand;
                    //Console.WriteLine(reference.DeclaringType.Namespace + " " + reference.DeclaringType.Name + " " + reference.Name);
                    Sink tmpSink = new Sink(reference.DeclaringType.Namespace, reference.DeclaringType.Name, reference.Name);
                    if (Engine.isDangerSink(tmpSink, aSinks))
                    {
                        //Console.WriteLine("DANGER >>>> " + reference.DeclaringType.Namespace + " " + reference.DeclaringType.Name + " " + reference.Name);
                        aWarnings.Add(new Warning(this, (Input)oInput));
                        Console.WriteLine("Metida SINK");
                    }
                    else
                    {
                        //Console.WriteLine("<<<<<<<<<<< " + reference.DeclaringType.Namespace + " " + reference.DeclaringType.Name + " " + reference.Name);
                    }
                }

            }
            return aWarnings;
        }

    }
}
