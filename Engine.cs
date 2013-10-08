using System;
using System.Collections.Generic;
using System.Text;

using System.Collections;
using System.Xml;
using System.IO;

using Mono.Cecil;
using Mono.Cecil.Cil;
using QuickGraph;

namespace Escan.Engine
{

    public class Engine
    {
        private string sFile;
        private List<string> aClassNamesToAnalyze;
        AssemblyDefinition oAssembly;
        List<Source> aSources;
        List<Sink> aSinks;
        
        List<InitialInput> aInitialInputs;
        List<Warning> aWarnings;

        private static Engine oCurrentEngine;
        public static Engine Instance
        {
            get 
            {
                return Engine.oCurrentEngine;
            }
        }

        public AssemblyDefinition Assembly
        {
            get { return this.oAssembly; }
        }
        //AdjacencyGraph<ICilNode, Edge<ICilNode>> tmp;

        public IList<InitialInput> InitialInputs
        {
            get { return this.aInitialInputs; }
        }

        public IList<Warning> Warnings
        {
            get { return this.aWarnings; }
        }

        public List<string> ClassNamesToAnalyze
        {
            get { return this.aClassNamesToAnalyze; }
        }
        
        public Engine(List<string> aClassNamesToAnalyze, string sFile)
        {
            this.sFile = sFile;
            this.aClassNamesToAnalyze = aClassNamesToAnalyze;
            this.oAssembly = AssemblyFactory.GetAssembly(sFile); 

            LoadSourcesXml();
            LoadSinksXml();
            aInitialInputs = new List<InitialInput>();
            aWarnings = new List<Warning>();

            Engine.oCurrentEngine = this;

            //the result of this is new Inputs
            this.SearchSources();
            this.BuildInputTrees();
        }

        //Carga el fichero XML de sources
        private void LoadSourcesXml()
        {
            FileStream oStream = new FileStream("sources.xml", FileMode.Open, FileAccess.Read);
            XmlTextReader oReader = new XmlTextReader(oStream);
            aSources = new List<Source>();
			
            while (oReader.Read())
            {
                if (oReader.NodeType == XmlNodeType.Element)
                {
                    for (int i = 0; i < oReader.AttributeCount; i++)
                    {
                        aSources.Add(MakeSource(oReader.GetAttribute(i)));
                    }
                }
            }
        }

        private void LoadSinksXml()
        {
            FileStream oStream = new FileStream("sinks.xml", FileMode.Open, FileAccess.Read);
            XmlTextReader oReader = new XmlTextReader(oStream);
            this.aSinks = new List<Sink>();

            while (oReader.Read())
            {
                if (oReader.NodeType == XmlNodeType.Element)
                {
                    for (int i = 0; i < oReader.AttributeCount; i++)
                    {
                        aSinks.Add(MakeSink(oReader.GetAttribute(i)));
                    }
                }
            }
        }

        //To parse the XML line and convert it to a Sink class
        private Sink MakeSink(string line)
        {
            //FIXME: mazo de cutre, usar ReadableRegex
            string[] parts = line.Split('.');
            string function = parts[parts.Length - 1];
            string[] function_split = function.Split('(');
            function = function_split[0];
            string name = parts[parts.Length - 2];
            string ns = "";

            for (int i = 0; i < parts.Length - 2; i++)
            {
                ns += parts[i] + ".";
            }
            ns = ns.Substring(0, ns.Length - 1);
            Sink tmp = new Sink(ns, name, function);
            return tmp;
        }

        //To parse the XML line and convert it to a Source class
        private Source MakeSource(string sLine)
        {
            //FIXME: mazo de cutre, usar ReadableRegex
            string[] parts = sLine.Split('.');
            string function = parts[parts.Length - 1];
            string[] function_split = function.Split('(');
            function = function_split[0];
            string name = parts[parts.Length - 2];
            string ns = "";
            
            for (int i = 0; i < parts.Length - 2; i++)
            {
                ns += parts[i] + ".";
            }
            ns = ns.Substring(0, ns.Length - 1);
            Source tmp = new Source(ns, name, function);
            return tmp;
        }


        //Busca todas las posibles inputs comparandolo con los sources
        //FIXME: Hay que tratar con los namespaces, etc
        public void SearchSources()
        {
            foreach (TypeDefinition oType in oAssembly.MainModule.Types)
            {
                //Console.WriteLine(type.FullName);
                if (this.aClassNamesToAnalyze.Contains(oType.FullName))
                {
                    this.Analyze(oType);
                }
            }
        }

        private void Analyze(TypeDefinition oType)
        {
            //Console.WriteLine(type.FullName);
            foreach (MethodDefinition oMethod in oType.Methods)
            {
                this.Analyze(oMethod);
            }
        }

        private void Analyze(MethodDefinition oMethod)
        {
            foreach (Instruction oInstr in oMethod.Body.Instructions)
            {
                if (OpCode.IsACallInstruction(oInstr))
                {
                    //Console.WriteLine(instruccion.ToString() + " in " + instruccion.Operand.GetType().Name);
                    if (oInstr.Operand is MethodReference)
                    {
                        MethodReference oMethodCalled = (MethodReference)oInstr.Operand;
                        //Console.WriteLine(reference.DeclaringType.Name);
                        //Console.WriteLine(reference.DeclaringType.Namespace);

                        if (IsDangerous(oMethodCalled))
                        {
                            Console.WriteLine("Metido " + oMethodCalled.ToString() + "como input");
                            InitialInput oNewInput = new InitialInput(oMethod, oInstr);
                            aInitialInputs.Add(oNewInput);
                            //aPotentialSources.Add(instruccion);
                        }

                    }
                    //I think this could only be found when using delegates (FIXME: add a test for a delegate)
                    else if (oInstr.Operand is MethodDefinition)
                    {
                        MethodDefinition definition = (MethodDefinition)oInstr.Operand;

                        //ITree<Input> tree = NodeTree<Input>.NewTree();
                    }
                }

            }
        }


        //Sin soporte para ldarg de momento
        internal static bool detectModifications(Instruction myInst, Instruction caller)
        {
            Console.WriteLine("****************** Miramos " + myInst.OpCode.Name);
            if (OpCode.IsAStlocInstruction(myInst))
            {
                Instruction inst = caller.Previous;
                while (inst != myInst)
                {
                    if ((AreEquivalent(inst, myInst)))
                    {
                        return true;
                    }
                    inst = inst.Previous;
                }
                return false;
            }
            if (OpCode.IsALdargInstruction(myInst))
            {
                Instruction inst = caller.Previous;
                Console.WriteLine("Miramos ******* " + inst.OpCode.Name);
                while (inst != myInst)
                {
                    if ((AreEquivalent(inst, myInst)))
                    {
                        return true;
                    }
                    inst = inst.Previous;
                }
                return false; 
            }
            return false;
        }


        public static bool AreEquivalent(Instruction first, Instruction last) {
            if (OpCode.IsAStlocInstruction(last))
            {
                if (first.OpCode.Name != "stloc.s")
                {
                    if (first.OpCode.Name == last.OpCode.Name) {
                        return true;
                    }
                }
                else 
                {
                    string one = Convert.ToString(first.Operand);
                    string two = Convert.ToString(last.Operand);
                    if (one == two) {
                        return true;
                    }
                }
            }
            else if (OpCode.IsALdargInstruction(last))
            {
                if (first.OpCode.Name != "ldarg.s")
                {
                    if (first.OpCode.Name == last.OpCode.Name)
                    {
                        return true;
                    }
                }
                else
                {
                    string one = Convert.ToString(first.Operand);
                    string two = Convert.ToString(last.Operand);
                    if (one == two)
                    {
                        return true;
                    }
                }
           }
           return false;
        }

        internal static Dictionary<Instruction, MethodDefinition> searchReturnCallers(MethodDefinition method)
        {
            Dictionary<Instruction, MethodDefinition> hCallers = new Dictionary<Instruction,MethodDefinition>() ;
            foreach (TypeDefinition oType in Engine.Instance.Assembly.MainModule.Types)
            {
                Console.WriteLine("Buscamos quien llama a la funcion con return" + " - " + method.Name);
                if (Engine.Instance.ClassNamesToAnalyze.Contains(oType.FullName))
                {
                    foreach (MethodDefinition oMethod in oType.Methods)
                    {
                        foreach (Instruction oInstr in oMethod.Body.Instructions)
                        {
                            if (OpCode.IsACallInstruction(oInstr))
                            {
                                if (oInstr.Operand is MethodReference)
                                {
                                    MethodReference oMethodCalled = (MethodReference)oInstr.Operand;
                                    if (oMethodCalled.Name == method.Name)
                                    {
                                        
                                        hCallers.Add(oInstr, oMethod);
                                        
                                        
                                    }
                                    Console.WriteLine(oMethodCalled.Name);

                                }
                            }

                        }
                    }

                }
            }
            return hCallers;
        }

        internal static bool isInstance(Instruction inst)
        {
            if (!Engine.isVar(inst) && !OpCode.IsALdargInstruction(inst) && !OpCode.IsAPopInstruction(inst.Next))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public static MethodDefinition getMethodByCaller(Instruction inst)
        {
            if (inst.Operand is MethodReference)
            {
                MethodReference reference = (MethodReference)inst.Operand;
                Console.WriteLine("XXXXXXXXXXXXXXXXXXXXx " + reference.DeclaringType.FullName);
                Console.WriteLine(reference.Name);

                //FIXME tiene que haber una forma mas LIMPIA de hacerlo
                foreach (TypeDefinition type in Engine.Instance.Assembly.MainModule.Types)
                {
                    Console.WriteLine("XX" + type.Name);
                    Console.WriteLine("XX" + reference.DeclaringType.FullName);
                    if ((type.Name == reference.DeclaringType.FullName) || (type.FullName == reference.DeclaringType.FullName))
                    {
                        foreach (MethodDefinition method in type.Methods)
                        {
                            if ((method.Name == reference.Name) && (reference.Parameters == method.Parameters))
                            {
                                return method;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public void BuildInputTrees()
        {
            foreach (InitialInput oInput in aInitialInputs)
            {
                oInput.FillTree();

                List<Warning> aInputWarnings = oInput.DetectSink(this.aSinks);
                foreach(Warning oNewWarning in aInputWarnings)
                {
                    this.aWarnings.Add(oNewWarning);
                }
            }
        }


        //No se va a utilizar
        //Nos devuelve el ldarg para seguir en el proximo metodo
        internal static List<Instruction> detectLdarg(MethodDefinition method, String opc) {
            List<Instruction> callers = new List<Instruction>();
            foreach (Instruction inst in method.Body.Instructions)
            {
                //Console.WriteLine("La variable que nos da null es " + opc);
                if (inst.OpCode.Name == opc)
                {
                    callers.Add(inst);
                }
            }
            return callers;
        }

        //Devuelve el ldarg correspondiente según el int
        //FIXME para soportar los ldarg.S
        internal static string getLdarg(int number)
        {
            return "ldarg." + Convert.ToString(number);
        }


        //Nos devuelve el numero de argumento de nuestra variable cuando se pasa a un metodo
        internal static int getNumberArgument(Instruction inst)
        {
            Console.WriteLine("-------------------------------------------------");
            int i = 1;
            while (inst.Previous != null)
            {
                Console.WriteLine(inst.OpCode.Name);
                if (isLdlocInstruction(inst.Previous))
                {
                    return i;
                }
                if (OpCode.IsALdargInstruction(inst.Previous))
                {
                    return i;
                }
                i++;
                inst = inst.Previous;
            }
            return 0;
           

        }


        //Habria que buscar algo mas limpio para detectarlo ya que nos podemos estar dejando 
        //algún caso.
        private bool isArgument(Instruction inst)
        {
            if (!isVar(inst) && !OpCode.IsARet(inst))
            {
                return true;
            }
            return false;
        }

        //Para saber si es variable
        private static bool isVar(Instruction inst)
        {
            if (OpCode.IsAStlocInstruction(inst.Next))
            {
                return true;
            }
            return false;
        }

        //Nos devuelve la variable
        internal static Instruction getVar(Instruction inst)
        {
            if (OpCode.IsALdargInstruction(inst))
            {
                return inst;
            }
            else
            {
                return inst.Next;
            }
        }

        private bool IsDangerous(MethodReference oMethodCall)
        {
            Source oTempSource = new Source(
               oMethodCall.DeclaringType.Namespace,
               oMethodCall.DeclaringType.Name,
               oMethodCall.Name);

            foreach (Source oSource in aSources)
            {
                if (oTempSource.Equals(oSource))
                {
                    //Console.WriteLine("ojo");
                    return true;
                }
            }
            return false;
        }

        public static bool isDangerSink(Sink sSink, List<Sink> aSinks)
        {
            foreach (Sink objSink in aSinks)
            {
                if (sSink.Equals(objSink)) {
                    return true;
                }
            }
            return false;
        }

        //Para saber si una instruccion es un ldloc
        public static bool isLdlocInstruction(Instruction ldInstruction)
        {
            return ldInstruction.OpCode == OpCodes.Ldloc ||
                   ldInstruction.OpCode == OpCodes.Ldloc_0 ||
                   ldInstruction.OpCode == OpCodes.Ldloc_1 ||
                   ldInstruction.OpCode == OpCodes.Ldloc_2 ||
                   ldInstruction.OpCode == OpCodes.Ldloc_3 ||
                   ldInstruction.OpCode == OpCodes.Ldloc_S;

        }

        //FIXME: to OpCode
        //Se le pasa un stloc y devuelve el ldloc correspondiente
        internal static string stTold(string opcode)
        {
            switch (opcode)
            {
                case "stloc.0":
                    return "ldloc.0";
                case "stloc.1":
                    return "ldloc.1";
                case "stloc.2":
                    return "ldloc.2";
                case "stloc.3":
                    return "ldloc.3";
                case "stloc.S":
                    return "ldloc.S";

            }
            return null;
        }

        private string ldTost(string opcode)
        {
            switch (opcode)
            {
                case "ldloc.0":
                    return "stloc.0";
                case "ldloc.1":
                    return "stloc.1";
                case "ldloc.2":
                    return "stloc.2";
                case "ldloc.3":
                    return "stloc.3";
                case "ldloc.S":
                    return "stloc.S";

            }
            return null;
        }


        //PAra soportar los ldloc.S
        internal static List<Instruction> getldlocS(string V_, MethodDefinition method)
        {
            List<Instruction> aInst = new List<Instruction>();
            foreach (Instruction inst in method.Body.Instructions)
            {
                if (inst.OpCode == OpCodes.Ldloc_S)
                {
                    if (Convert.ToString(inst.Operand) == V_)
                    {
                        Console.WriteLine("Se llama desde " + detectCaller(inst).Operand.ToString() + " a " + V_);
                        aInst.Add(detectCaller(inst));
                        //isNative(detectCaller(inst));
                    }
                }
            }
            return aInst;

        }


        //Nos tiene que decir si uno de los metodos en los que nos estamos metiendo pertenencen al codigo que se tiene
        //que auditar del cliente o por otro lado es nativo de java.
        internal static bool isNative(Instruction inst)
        {
            if (OpCode.IsACallInstruction(inst))
            {
                if (inst.Operand is MethodReference)
                {
                    MethodReference reference = (MethodReference)inst.Operand;
                    Console.WriteLine("Pertenece a la clase " + reference.DeclaringType.Name);
                    Console.WriteLine(reference.DeclaringType.Namespace);
                    if ((Engine.Instance.aClassNamesToAnalyze.Contains(reference.DeclaringType.Namespace)) ||
                         Engine.Instance.aClassNamesToAnalyze.Contains(reference.DeclaringType.Name) || 
                         Engine.Instance.aClassNamesToAnalyze.Contains(reference.DeclaringType.Namespace + "." + reference.DeclaringType.Name))
                    {
                        //Console.WriteLine("MIRAAAAAAAAAAAAAAAAA ");
                        Console.WriteLine("ENE STA CLASE NOS TENEMOS QUE METER A ANALIZARLA");
                        return true;
                    }
                }
                else if (inst.Operand is MethodDefinition)
                {
                    MethodDefinition definition = (MethodDefinition)inst.Operand;
                    Console.WriteLine("Pertenece a la clase " + definition.DeclaringType.Name);
                    //ITree<Input> tree = NodeTree<Input>.NewTree();
                    return true;
                }
                else
                {
                    Console.WriteLine("NO SABEMOS QUE ES " + inst.ToString());
                }
            }
            return false;
        }

        private static Instruction getNextCaller(Instruction inst)
        {
            
            while (inst.Next != null)
            {
                if (OpCode.IsACallInstruction(inst.Next))
                {
                    return inst.Next;
                }
                else
                {
                    inst = inst.Next;
                }
            }
            return null;
        }

        private static bool isThisCaller(Instruction myInst, Instruction callerInst)
        {
            //FIXME: implement recursive comparation
            return true;
        }
        
        // FIXME: WTF? This is called but the values returned are not stored or used
        public static List<ParameterDefinition> getCallArgs(Instruction inst)
        {
            List<ParameterDefinition> parameters = new List<ParameterDefinition>();
            if (OpCode.IsACallInstruction(inst))
            {
                MethodReference reference = (MethodReference)inst.Operand;
                Console.WriteLine("Estoy dentro de los parametros " + reference.Name);
                foreach (ParameterDefinition param in reference.Parameters)
                {
                    parameters.Add(param);
                    //Console.WriteLine("El parametro es " + param.ParameterType.Name);
                }
            }
            return parameters;
        }

        //Devuelve la proxima llamada a un stloc (Usado para detectar en que array se ha metido nuestro valor)
        private Instruction getNextStloc(Instruction inst)
        {
            Instruction myInst;
            while (inst.Next != null)
            {
                myInst = inst.Next;
                if (OpCode.IsAStlocInstruction(myInst))
                {
                    return myInst;
                }
                myInst = myInst.Next;
            }
            return null;
        }


        //Para detectar quien esta usando nuestra input
        //Defecated sustituida por detectNewCaller
        internal static Instruction detectCaller(Instruction inst)
        {
            Instruction caller = getNextCaller(inst);
            while (caller != null)
            {
                Console.WriteLine("Probando si se llama desde " + caller.Operand.ToString());
                if (isThisCaller(inst, caller))
                {
                    return caller;
                }
                caller = getNextCaller(caller);
            }
            return null;

        }

    }

}

