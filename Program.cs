using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Mono.Cecil;
using QuickGraph;

namespace Escan.Engine
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> aClassNames = new List<string>();
            aClassNames.Add("Test2");

            Engine analizador = new Engine(aClassNames, "Test2.dll");
            aClassNames.Clear();
            //analizador.busca_sources();

            /*
            foreach (Input inp in analizador.Inputs)
            {
                Console.WriteLine(inp.tree.EdgeCount + " edges - " + inp.tree.VertexCount + " vertex ");
                foreach (Input inpu in inp.tree.Vertices)
                {

                    //Console.WriteLine(inpu.method.Name + " - " + inpu.sClassName);
                    foreach (MarkedEdge<Input, int> e in inp.tree.OutEdges(inpu))
                    {
                        //Console.WriteLine(e.Source.inst.Operand.ToString() + "->" + e.Target.inst.Operand.ToString());
                        

                    }
                }
            }

            foreach (Input inp in analizador.Inputs)
            {
                
            }
            */
            foreach (Warning myWarn in analizador.Warnings)
            {
                Console.WriteLine("<<<<" + myWarn.wInp.Instr.Operand.ToString());
                Console.WriteLine("\t>>>>" + myWarn.wOut.Instr.Operand.ToString());

            }

            Console.WriteLine(analizador.Warnings.Count);
            Console.Read();

        }
    }
}
