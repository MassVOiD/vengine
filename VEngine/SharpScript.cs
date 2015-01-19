using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VDGTech
{
    class SharpScript
    {
        private static CSharpCodeProvider compiler = new CSharpCodeProvider(new Dictionary<String, String> { { "CompilerVersion", "v3.5" } });
        public static dynamic CreateClass(string script)
        {
            var name = "CLS" + Guid.NewGuid().ToString().Substring(0, 8).Replace("-", string.Empty);
            string code = string.Format(@"
            namespace VDGTech.SharpScriptNS{{ 
                public class {0} 
                {{ 
                    {1} 
                }} 
            }}", name, script);

            var parameters = new CompilerParameters();

            var compilerResult = compiler.CompileAssemblyFromSource(parameters, code); 
            string errorText = String.Empty;
            foreach (CompilerError compilerError in compilerResult.Errors)
                errorText += compilerError + "\n";
            var compiledAssembly = compilerResult.CompiledAssembly;
            var type = compiledAssembly.GetType("VDGTech.SharpScriptNS." + name).GetConstructor(new Type[0]).Invoke(new object[0]);
            return type;
        }
    }
}
