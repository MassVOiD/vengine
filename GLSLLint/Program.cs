using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDGTech;
using OpenTK.Graphics.OpenGL4;

namespace GLSLLint
{
    class Program
    {
        static void Main(string[] args)
        {
            string typestr = args[0];
            ShaderType type = ShaderType.VertexShader;
            if (typestr == "vertex" || typestr == "v") type = ShaderType.VertexShader;
            if (typestr == "fragment" || typestr == "f") type = ShaderType.FragmentShader;
            if (typestr == "compute" || typestr == "c") type = ShaderType.ComputeShader;
            if (typestr == "geometry" || typestr == "g") type = ShaderType.GeometryShader;
            if (typestr == "tesscontrol" || typestr == "tc") type = ShaderType.TessControlShader;
            if (typestr == "tesseval" || typestr == "te") type = ShaderType.TessEvaluationShader;
            string source = System.IO.File.ReadAllText(args[1]);

            var window = new VEngineInvisibleAdapter();

            int shader = GL.CreateShader(type);
            System.IO.Directory.SetCurrentDirectory(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
            var Config = SharpScript.CreateClass(System.IO.File.ReadAllText("Config.css"));
            Media.SearchPath = Config.MediaPath;
            Console.WriteLine(ShaderPreparser.Preparse(source));
            GL.ShaderSource(shader, ShaderPreparser.Preparse(source));

            GL.CompileShader(shader);

            string compilationResult = GL.GetShaderInfoLog(shader);
            int status_code;
            GL.GetShader(shader, ShaderParameter.CompileStatus, out status_code);
            if (status_code != 1)
            {
                Console.WriteLine("Compilation FAILED");
                Console.WriteLine(compilationResult);
            }
            else Console.WriteLine("OK");
            window.Close();
            window.Dispose();

        }
    }
}
