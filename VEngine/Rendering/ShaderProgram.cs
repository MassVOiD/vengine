using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class ShaderProgram
    {
        public static ShaderProgram Current = null;
        static public bool Lock = false;
        public int Handle = -1;
        public bool UsingTesselation = false;
        private static List<ShaderProgram> AllPrograms = new List<ShaderProgram>();
        public bool Compiled;
        private string FragmentFile;
        private string GeometryFile;
        private Dictionary<string, string> Globals = new Dictionary<string, string>();
        private string lastCombinedSourcesHash;

        private string TessControlFile;
        private string TessEvalFile;
        private Dictionary<string, int> UniformLocationsCache;
        private Dictionary<string, object> ValuesMap;

        private string VertexFile;
        private string VertexSource, FragmentSource, GeometrySource = null, TessControlSource = null, TessEvaluationSource = null;

        private ShaderProgram(string vertexFile, string fragmentFile, string geometryFile = null, string tesscontrolFile = null, string tessevalFile = null)
        {
            ValuesMap = new Dictionary<string, object>();
            VertexFile = vertexFile;
            FragmentFile = fragmentFile;
            GeometryFile = geometryFile;
            TessControlFile = tesscontrolFile;
            TessEvalFile = tessevalFile;
            lastCombinedSourcesHash = "";
            Recompile();
            AllPrograms.Add(this);
        }

        public enum SwitchResult
        {
            Switched,
            AlreadyInUse,
            Locked
        }

        public static ShaderProgram Compile(string vertex, string fragment, string geometry = null, string tesscontrol = null, string tesseval = null)
        {
            string concatedNames = vertex + fragment + geometry + (tesscontrol != null ? tesscontrol : "notess") + (tesseval != null ? tesseval : "notessev");

            var cached = ShaderCache.GetShaderProgramOrNull(concatedNames);
            if(cached != null)
                return cached;
            var output = new ShaderProgram(vertex, fragment, geometry, tesscontrol, tesseval);
            ShaderCache.CacheShaderProgram(concatedNames, output);
            return output;
        }

        public static void RecompileAll()
        {
            AllPrograms.ForEach(a => a.Recompile());
        }

        public void BindAttributeLocation(int index, string name)
        {
            GL.BindAttribLocation(Handle, index, name);
        }

        public void Recompile()
        {
            UniformLocationsCache = new Dictionary<string, int>();

            VertexSource = ShaderPreparser.Preparse(VertexFile, Media.ReadAllText(VertexFile));
            FragmentSource = ShaderPreparser.Preparse(FragmentFile, Media.ReadAllText(FragmentFile));
            if(GeometryFile != null)
            {
                GeometrySource = ShaderPreparser.Preparse(GeometryFile, Media.ReadAllText(GeometryFile));
            }
            if(TessControlFile != null && TessEvalFile != null)
            {
                TessControlSource = ShaderPreparser.Preparse(TessControlFile, Media.ReadAllText(TessControlFile));
                TessEvaluationSource = ShaderPreparser.Preparse(TessEvalFile, Media.ReadAllText(TessEvalFile));
                UsingTesselation = true;
            }
            var sb = new StringBuilder();
            sb.AppendLine(VertexSource);
            sb.AppendLine(FragmentSource);
            sb.AppendLine(GeometrySource);
            sb.AppendLine(TessControlSource);
            sb.AppendLine(TessEvaluationSource);
            string hash = Hash(sb.ToString());
            if(lastCombinedSourcesHash != hash)
            {
                Compiled = false;
                lastCombinedSourcesHash = hash;
            }
        }

        public void RemoveGlobal(string key)
        {
            if(Globals.ContainsKey(key))
                Globals.Remove(key);
        }

        public void SetGlobal(string key, string value)
        {
            if(Globals.ContainsKey(key))
                Globals[key] = value;
            else
                Globals.Add(key, value);
        }

        public void SetUniform(string name, Matrix4 data)
        {
            int location = GetUniformLocation(name);
            if(location >= 0 && CheckCache(name, data))
                GL.UniformMatrix4(location, false, ref data);
        }

        public void SetUniform(string name, bool data)
        {
            int location = GetUniformLocation(name);
            if(location >= 0 && CheckCache(name, data))
                GL.Uniform1(location, data ? 1 : 0);
        }

        public void SetUniform(string name, float data)
        {
            int location = GetUniformLocation(name);
            if(location >= 0 && CheckCache(name, data))
                GL.Uniform1(location, data);
        }

        public void SetUniform(string name, uint data)
        {
            //if(name == "Instances")
            //     Console.WriteLine(data);
            int location = GetUniformLocation(name);
            if(location >= 0 && CheckCache(name, data))
                GL.Uniform1(location, data);
        }
        public void SetUniform(string name, int data)
        {
            //if(name == "Instances")
            //     Console.WriteLine(data);
            int location = GetUniformLocation(name);
            if(location >= 0 && CheckCache(name, data))
                GL.Uniform1(location, data);
        }

        public void SetUniform(string name, Vector2 data)
        {
            int location = GetUniformLocation(name);
            if(location >= 0 && CheckCache(name, data))
                GL.Uniform2(location, data);
        }

        public void SetUniform(string name, Vector3 data)
        {
            int location = GetUniformLocation(name);
            if(location >= 0 && CheckCache(name, data))
                GL.Uniform3(location, data);
        }

        public void SetUniform(string name, Color4 data)
        {
            int location = GetUniformLocation(name);
            if(location >= 0 && CheckCache(name, data))
                GL.Uniform4(location, data);
        }

        public void SetUniform(string name, Vector4 data)
        {
            int location = GetUniformLocation(name);
            if(location >= 0 && CheckCache(name, data))
                GL.Uniform4(location, data);
        }

        public void SetUniformArray(string name, Matrix4[] data)
        {
            int location = GetUniformLocation(name);
            List<float> floats = new List<float>();
            foreach(var v in data)
            {
                floats.Add(v.Row0.X);
                floats.Add(v.Row0.Y);
                floats.Add(v.Row0.Z);
                floats.Add(v.Row0.W);

                floats.Add(v.Row1.X);
                floats.Add(v.Row1.Y);
                floats.Add(v.Row1.Z);
                floats.Add(v.Row1.W);

                floats.Add(v.Row2.X);
                floats.Add(v.Row2.Y);
                floats.Add(v.Row2.Z);
                floats.Add(v.Row2.W);

                floats.Add(v.Row3.X);
                floats.Add(v.Row3.Y);
                floats.Add(v.Row3.Z);
                floats.Add(v.Row3.W);
            }
            if(location >= 0 && CheckCache(name, data))
            {
                GL.UniformMatrix4(location, data.Length, false, floats.ToArray());
                GLThread.CheckErrors(name);
            }
        }

        public void SetUniformArray(string name, Vector3[] data)
        {
            int location = GetUniformLocation(name);
            List<float> floats = new List<float>();
            foreach(var v in data)
            {
                floats.Add(v.X);
                floats.Add(v.Y);
                floats.Add(v.Z);
            }
            if(location >= 0 && CheckCache(name, data))
            {
                GL.Uniform3(location, data.Length, floats.ToArray());
                GLThread.CheckErrors(name);
            }
        }

        public void SetUniformArray(string name, Vector2[] data)
        {
            int location = GetUniformLocation(name);
            List<float> floats = new List<float>();
            foreach(var v in data)
            {
                floats.Add(v.X);
                floats.Add(v.Y);
            }
            if(location >= 0 && CheckCache(name, data))
            {
                GL.Uniform2(location, data.Length, floats.ToArray());
                GLThread.CheckErrors(name);
            }
        }

        public void SetUniformArray(string name, Vector4[] data)
        {
            int location = GetUniformLocation(name);
            List<float> floats = new List<float>();
            foreach(var v in data)
            {
                floats.Add(v.X);
                floats.Add(v.Y);
                floats.Add(v.Z);
                floats.Add(v.W);
            }
            if(location >= 0 && CheckCache(name, data))
            {
                GL.Uniform4(location, data.Length, floats.ToArray());
                GLThread.CheckErrors(name);
            }
        }

        public void SetUniformArray(string name, float[] data)
        {
            int location = GetUniformLocation(name);
            if(location >= 0 && CheckCache(name, data))
            {
                GL.Uniform1(location, data.Length, data);
                GLThread.CheckErrors(name);
            }
        }

        public void SetUniformArray(string name, int[] data)
        {
            int location = GetUniformLocation(name);
            if(location >= 0 && CheckCache(name, data))
            {
                GL.Uniform1(location, data.Length, data);
                GLThread.CheckErrors(name);
            }
        }

        public SwitchResult Use()
        {
            if(!Lock)
            {
                if(!Compiled)
                    Compile();
                if(Current == this)
                    return SwitchResult.AlreadyInUse;
                GL.UseProgram(Handle);
                Current = this;
                return SwitchResult.Switched;
            }
            return SwitchResult.Locked;
        }

        private static int GetUniformLocation(string name)
        {
            if(Current.Handle == -1)
                return -1;
            if(Current.UniformLocationsCache.ContainsKey(name))
                return Current.UniformLocationsCache[name];
            int location = GL.GetUniformLocation(Current.Handle, name);
            GLThread.CheckErrors("Locating " + name);
            //if(!Lock)
            Current.UniformLocationsCache.Add(name, location);
            //if(Lock && name == "Time")
            //    return -1;
            return location;
        }

        private static string Hash(string input)
        {
            using(SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach(byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        private bool CheckCache(string key, object value)
        {
            if(!ValuesMap.ContainsKey(key))
            {
                ValuesMap.Add(key, value);
                return true;
            }
            else
            {
                if(ValuesMap[key] == value)
                    return false;
                else
                    return true;
            }
        }

        private void Compile()
        {
            Handle = GL.CreateProgram();

            Console.WriteLine("Compiling vertex shader {0}", VertexFile);
            int vertexShaderHandle = CompileSingleShader(ShaderType.VertexShader, VertexSource);
            GL.AttachShader(Handle, vertexShaderHandle);

            Console.WriteLine("Compiling fragment shader {0}", FragmentFile);
            int fragmentShaderHandle = CompileSingleShader(ShaderType.FragmentShader, FragmentSource);
            GL.AttachShader(Handle, fragmentShaderHandle);

            if(TessControlSource != null && TessEvaluationSource != null)
            {
                Console.WriteLine("Compiling tesselation control shader {0}", TessControlFile);
                int tessCShaderHandle = CompileSingleShader(ShaderType.TessControlShader, TessControlSource);
                GL.AttachShader(Handle, tessCShaderHandle);

                Console.WriteLine("Compiling tesselation evaluation shader {0}", TessEvalFile);
                int tessEShaderHandle = CompileSingleShader(ShaderType.TessEvaluationShader, TessEvaluationSource);
                GL.AttachShader(Handle, tessEShaderHandle);
                GL.PatchParameter(PatchParameterInt.PatchVertices, 3);
            }

            if(GeometrySource != null)
            {
                Console.WriteLine("Compiling geometry shader {0}", GeometryFile);
                int geometryShaderHandle = CompileSingleShader(ShaderType.GeometryShader, GeometrySource);
                GL.AttachShader(Handle, geometryShaderHandle);
            }

            GL.LinkProgram((uint)Handle);
            var ostr = GL.GetProgramInfoLog(Handle).Trim();
            if(ostr.Length > 0)
                Console.WriteLine(ostr);

            int status_code;
            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out status_code);
            if(status_code != 1)
                throw new ApplicationException("Linking error");

            GL.UseProgram(Handle);

            ostr = GL.GetProgramInfoLog(Handle).Trim();
            if(ostr.Length > 0)
                Console.WriteLine(ostr);

            Compiled = true;
        }

        private int CompileSingleShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);

            StringBuilder globalsString = new StringBuilder();
            foreach(var g in Globals)
            {
                globalsString.AppendLine("#define " + g.Key + " " + g.Value);
            }

            string fullsrc = Regex.Replace(source, @"\#version (.+)\r\n", "#version $1\r\n" + globalsString);

            GL.ShaderSource(shader, fullsrc);

            GL.CompileShader(shader);

            var ostr = GL.GetShaderInfoLog(shader).Trim();
            if(ostr.Length > 0)
                Console.WriteLine(ostr);
            int status_code;
            GL.GetShader(shader, ShaderParameter.CompileStatus, out status_code);
            if(status_code != 1)
                throw new ApplicationException("Compilation error");
            return shader;
        }
    }
}