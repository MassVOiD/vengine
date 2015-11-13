using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp;

namespace VEngine
{
    public class ExpressionEvaluator
    {
        public static void Eval(List<Tuple<string, object>> localVariables, string expression)
        {
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            string cname = "EvaluationD"+ (uint)(DateTime.Now - GLThread.StartTime).TotalMilliseconds;
            StringBuilder varssb = new StringBuilder();
            StringBuilder varssb2 = new StringBuilder();
            StringBuilder varssb3 = new StringBuilder();
            int vnum = 0;
            foreach(var v in localVariables)
            {
                vnum++;
                varssb.Append(v.Item2.GetType().GetTypeInfo().FullName);
                varssb.Append(" ");
                varssb.Append(v.Item1);
                varssb.AppendLine(";");

                varssb2.Append(v.Item2.GetType().GetTypeInfo().FullName);
                varssb2.Append(" a");
                varssb2.Append(vnum.ToString());
                varssb2.Append(",");

                varssb3.Append(v.Item1);
                varssb3.Append(" = a");
                varssb3.Append(vnum.ToString());
                varssb3.AppendLine(";");
            }
            string argsstr = varssb2.ToString().TrimEnd(',');
            string setstr = varssb3.ToString();
            string src = string.Format(@"
                    namespace " + cname + @"
                    {{
                        using System;
                        using System.Collections.Generic;
                        using System.Drawing;
                        using OpenTK;
                        using VEngine;
                        using VEngine.FileFormats;
                        using VEngine.Generators;
                        public class Evaluator
                        {{
                            " + varssb.ToString() + @"
                            public void Eval()
                            {{
                                {0}
                            }}
                            public void SetVal(" + argsstr + @")
                            {{
                                " + setstr + @"
                            }}

                            
                        }}
                    }}

                ", expression);
            CompilerParameters paramsa = new CompilerParameters();
            paramsa.GenerateInMemory = true;
            paramsa.GenerateExecutable = false;
            //paramsa.ReferencedAssemblies.Add("System.dll");
            var referencedAssemblies =
                AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.FullName.StartsWith("mscorlib", StringComparison.InvariantCultureIgnoreCase))
                .Where(a => !a.IsDynamic) 
                .Select(a => a.Location)
                .ToArray();
            paramsa.ReferencedAssemblies.AddRange(referencedAssemblies);

            CompilerResults results =
                codeProvider
                .CompileAssemblyFromSource(paramsa, new string[]
                {
                src
                });
            if(results.Errors.Count > 0)
            {
                var lines = src.Split('\n');
                foreach(var e in results.Errors)
                {
                    dynamic d = e;
                    Console.WriteLine(lines[d.Line - 1]);
                    Console.WriteLine(d.ErrorText);
                }
            }
            Assembly assembly = results.CompiledAssembly;
            dynamic evaluator =
                Activator.CreateInstance(assembly.GetType(cname+".Evaluator"));
            (evaluator.GetType() as Type).GetMethod("SetVal").Invoke(evaluator, localVariables.Select<Tuple<string, object>, object>((a) => a.Item2).ToArray());
            evaluator.Eval();
        }
    }
}
