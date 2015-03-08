using System;
using System.Text.RegularExpressions;
using OpenTK.Graphics.OpenGL4;
using VDGTech;

namespace GLSLLint
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string source = null;
            ShaderType type = ShaderType.VertexShader;
            if(args.Length == 2)
            {
                string typestr = args[0];
                type = ShaderType.VertexShader;
                if(typestr == "vertex" || typestr == "v")
                    type = ShaderType.VertexShader;
                if(typestr == "fragment" || typestr == "f")
                    type = ShaderType.FragmentShader;
                if(typestr == "compute" || typestr == "c")
                    type = ShaderType.ComputeShader;
                if(typestr == "geometry" || typestr == "g")
                    type = ShaderType.GeometryShader;
                if(typestr == "tesscontrol" || typestr == "tc")
                    type = ShaderType.TessControlShader;
                if(typestr == "tesseval" || typestr == "te")
                    type = ShaderType.TessEvaluationShader;
                source = System.IO.File.ReadAllText(args[1]);
            }
            else if(args.Length == 1)
            {
                string path = args[0];
                type = ShaderType.VertexShader;
                if(path.EndsWith(".vertex.glsl"))
                    type = ShaderType.VertexShader;
                if(path.EndsWith(".fragment.glsl"))
                    type = ShaderType.FragmentShader;
                if(path.EndsWith(".compute.glsl"))
                    type = ShaderType.ComputeShader;
                if(path.EndsWith(".geometry.glsl"))
                    type = ShaderType.GeometryShader;
                if(path.EndsWith(".tesscontrol.glsl"))
                    type = ShaderType.TessControlShader;
                if(path.EndsWith(".tesseval.glsl"))
                    type = ShaderType.TessEvaluationShader;
                source = System.IO.File.ReadAllText(path);
            }

            var window = new VEngineInvisibleAdapter();

            int shader = GL.CreateShader(type);
            System.IO.Directory.SetCurrentDirectory(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
            var Config = SharpScript.CreateClass(System.IO.File.ReadAllText("Config.css"));
            Media.SearchPath = Config.MediaPath;
            string src = ShaderPreparser.Preparse(source);
            GL.ShaderSource(shader, src);
            int i = 0;
            //string split = String.Join("\r\n", src.Split('\n').Select(a => (i++).ToString() + ": " + a).ToArray());
            //Console.WriteLine(split);

            GL.CompileShader(shader);

            string compilationResult = GL.GetShaderInfoLog(shader);
            int status_code;
            GL.GetShader(shader, ShaderParameter.CompileStatus, out status_code);
            if(status_code != 1)
            {
                Console.WriteLine("Compilation FAILED");
                Console.WriteLine(compilationResult.Trim());
                string[] errors = compilationResult.Split('\n');
                var codes = src.Split('\n');
                foreach(var line in errors)
                {
                    if(line.StartsWith("ERROR"))
                    {
                        //ERROR: 0:169: error(#143) Undeclared identifier: distance1
                        Match match = Regex.Match(line, @"ERROR: [0-9]+\:([0-9]+)\:");
                        if(match.Success)
                        {
                            Console.WriteLine("in line: " + codes[int.Parse(match.Groups[1].Value) - 1].Trim());
                        }
                    }
                }
            }
            else
                Console.WriteLine("OK");
            window.Close();
            window.Dispose();
        }
    }
}