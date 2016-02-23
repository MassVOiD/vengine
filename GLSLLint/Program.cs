using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using OpenTK.Graphics.OpenGL4;
using VEngine;

namespace GLSLLint
{
    internal class Program
    {
        private class Linter
        {
            private VEngineInvisibleAdapter window;

            public Linter()
            {
                window = new VEngineInvisibleAdapter();
            }

            public bool IsEntryPoint(string file)
            {
                if(file.EndsWith(".vertex.glsl"))
                    return true;
                if(file.EndsWith(".fragment.glsl"))
                    return true;
                if(file.EndsWith(".compute.glsl"))
                    return true;
                if(file.EndsWith(".geometry.glsl"))
                    return true;
                if(file.EndsWith(".tesscontrol.glsl"))
                    return true;
                if(file.EndsWith(".tesseval.glsl"))
                    return true;
                if(file.EndsWith(".geometry.glsl"))
                    return true;
                return false;
            }

            public string Lint(string file)
            {
                var type = ShaderType.VertexShader;
                if(file.EndsWith(".vertex.glsl"))
                    type = ShaderType.VertexShader;
                if(file.EndsWith(".fragment.glsl"))
                    type = ShaderType.FragmentShader;
                if(file.EndsWith(".compute.glsl"))
                    type = ShaderType.ComputeShader;
                if(file.EndsWith(".geometry.glsl"))
                    type = ShaderType.GeometryShader;
                if(file.EndsWith(".tesscontrol.glsl"))
                    type = ShaderType.TessControlShader;
                if(file.EndsWith(".tesseval.glsl"))
                    type = ShaderType.TessEvaluationShader;
                if(file.EndsWith(".geometry.glsl"))
                    type = ShaderType.GeometryShader;
                int shader = GL.CreateShader(type);
                string src = ShaderPreparser.Preparse(file, System.IO.File.ReadAllText(file));
                StringBuilder globalsString = new StringBuilder();
                globalsString.AppendLine("#define MSAA_SAMPLES " + Game.MSAASamples);
                if(Game.MSAASamples > 1)
                    globalsString.AppendLine("#define USE_MSAA");


                string fullsrc = Regex.Replace(src, @"\#version (.+)\r\n", "#version $1\r\n" + globalsString);
               // File.WriteAllText(file + ".out.txt", fullsrc);
                GL.ShaderSource(shader, fullsrc);

                GL.CompileShader(shader);

                string compilationResult = GL.GetShaderInfoLog(shader);
                int status_code;
                GL.GetShader(shader, ShaderParameter.CompileStatus, out status_code);
                if(status_code != 1)
                {
                    Console.WriteLine("Shader compilation FAILED");
                    Console.WriteLine("File: " + file);
                    Console.WriteLine(compilationResult.Trim());
                    string[] errors = compilationResult.Split('\n');
                    var codes = src.Split('\n');
                    foreach(var line in errors)
                    {
                        // if(line.StartsWith("ERROR"))
                        //{
                        Match match = Regex.Match(line, @"ERROR: [0-9]+\:([0-9]+)\:");
                        Match match2 = Regex.Match(line, @"0\(([0-9]+)\)");
                        try
                        {
                            if(match.Success)
                            {
                                Console.WriteLine(match.Value);
                                int id = int.Parse(match.Groups[1].Value) - 1;
                                if(id >= codes.Length)
                                    id = codes.Length - 1;
                                Console.WriteLine("in line AMD: " + codes[id].Trim());
                            }
                            if(match2.Success)
                            {
                                Console.WriteLine(match2.Value);
                                int id = int.Parse(match2.Groups[1].Value) - 1;
                                if(id >= codes.Length)
                                    id = codes.Length - 1;
                                Console.WriteLine("in line NV: " + codes[id].Trim());
                            }
                        }
                        catch { }
                        // }
                    }
                }
                return "";
            }
        }

        private static void CompileRecurse(Linter linter, string directory)
        {
            var files = Directory.GetFiles(directory);
            foreach(var f in files)
            {
                if(linter.IsEntryPoint(f))
                    linter.Lint(f);
            }
            var dirs = Directory.GetDirectories(directory);
            foreach(var d in dirs)
                CompileRecurse(linter, d);
        }

        private static void Main(string[] args)
        {
            Media.SearchPath = args[0];
            string directory = args[0];
            var linter = new Linter();
            CompileRecurse(linter, directory);
        }
    }
}