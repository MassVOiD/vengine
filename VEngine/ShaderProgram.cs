using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace VDGTech
{
    public class ShaderProgram
    {
        private ShaderProgram(string vertex, string fragment, string geometry = null, string tesscontrol = null, string tesseval = null)
        {
            UniformLocationsCache = new Dictionary<string, int>();

            VertexSource = ShaderPreparser.Preparse(vertex);
            FragmentSource = ShaderPreparser.Preparse(fragment);
            if(geometry != null)
            {
                GeometrySource = ShaderPreparser.Preparse(geometry);
            }
            if(tesscontrol != null && tesseval != null)
            {
                TessControlSource = ShaderPreparser.Preparse(tesscontrol);
                TessEvaluationSource = ShaderPreparser.Preparse(tesseval);
                UsingTesselation = true;
            }
            Compiled = false;
        }

        public static ShaderProgram Current = null;
        static public bool Lock = false;
        public int Handle = -1;
        public bool UsingTesselation = false;
        private bool Compiled;
        private Dictionary<string, int> UniformLocationsCache;
        private string VertexSource, FragmentSource, GeometrySource = null, TessControlSource = null, TessEvaluationSource = null;
        private static Dictionary<string, string> Globals = new Dictionary<string,string>();

        public static void SetGlobal(string key, string value)
        {
            if(Globals.ContainsKey(key))
                Globals[key] = value;
            else
                Globals.Add(key, value);
        }
        public static void RemoveGlobal(string key)
        {
            if(Globals.ContainsKey(key))
                Globals.Remove(key);
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

        public void BindAttributeLocation(int index, string name)
        {
            GL.BindAttribLocation(Handle, index, name);
        }

        public void SetUniform(string name, Matrix4 data)
        {
            int location = GetUniformLocation(name);
            if(location >= 0)
                GL.UniformMatrix4(location, false, ref data);
        }

        public void SetUniform(string name, float data)
        {
            int location = GetUniformLocation(name);
            if(location >= 0)
                GL.Uniform1(location, data);
        }

        public void SetUniform(string name, int data)
        {
            int location = GetUniformLocation(name);
            if(location >= 0)
                GL.Uniform1(location, data);
        }

        public void SetUniform(string name, Vector2 data)
        {
            int location = GetUniformLocation(name);
            if(location >= 0)
                GL.Uniform2(location, data);
        }

        public void SetUniform(string name, Vector3 data)
        {
            int location = GetUniformLocation(name);
            if(location >= 0)
                GL.Uniform3(location, data);
        }

        public void SetUniform(string name, Color4 data)
        {
            int location = GetUniformLocation(name);
            if(location >= 0)
                GL.Uniform4(location, data);
        }

        public void SetUniform(string name, Vector4 data)
        {
            int location = GetUniformLocation(name);
            if(location >= 0)
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
            if(location >= 0)
            {
                GL.UniformMatrix4(location, data.Length, false, floats.ToArray());
                GLThread.CheckErrors();
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
            if(location >= 0)
            {
                GL.Uniform3(location, data.Length, floats.ToArray());
                GLThread.CheckErrors();
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
            if(location >= 0)
            {
                GL.Uniform4(location, data.Length, floats.ToArray());
                GLThread.CheckErrors();
            }
        }

        public void SetUniformArray(string name, float[] data)
        {
            int location = GetUniformLocation(name);
            if(location >= 0)
            {
                GL.Uniform1(location, data.Length, data);
                GLThread.CheckErrors();
            }
        }

        public bool Use()
        {
            if(!Lock)
            {
                if(Current == this)
                    return false;
                if(!Compiled)
                    Compile();
                GL.UseProgram(Handle);
                Current = this;
                return true;
            }
            return false;
        }

        private static int GetUniformLocation(string name)
        {
            if(Current.Handle == -1)
                return -1;
            if(Current.UniformLocationsCache.ContainsKey(name) && !Lock)
                return Current.UniformLocationsCache[name];
            int location = GL.GetUniformLocation(Current.Handle, name);
            GLThread.CheckErrors();
            if(!Lock)
                Current.UniformLocationsCache.Add(name, location);
            if(Lock && name == "Time")
                return -1;
            return location;
        }

        private void Compile()
        {
            Handle = GL.CreateProgram();

            int vertexShaderHandle = CompileSingleShader(ShaderType.VertexShader, VertexSource);
            GL.AttachShader(Handle, vertexShaderHandle);

            int fragmentShaderHandle = CompileSingleShader(ShaderType.FragmentShader, FragmentSource);
            GL.AttachShader(Handle, fragmentShaderHandle);

            if(TessControlSource != null && TessEvaluationSource != null)
            {
                int tessCShaderHandle = CompileSingleShader(ShaderType.TessControlShader, TessControlSource);
                GL.AttachShader(Handle, tessCShaderHandle);

                int tessEShaderHandle = CompileSingleShader(ShaderType.TessEvaluationShader, TessEvaluationSource);
                GL.AttachShader(Handle, tessEShaderHandle);
                GL.PatchParameter(PatchParameterInt.PatchVertices, 3);
            }

            if(GeometrySource != null)
            {
                int geometryShaderHandle = CompileSingleShader(ShaderType.GeometryShader, GeometrySource);
                GL.AttachShader(Handle, geometryShaderHandle);
            }

            GL.LinkProgram((uint)Handle);
            Console.WriteLine(GL.GetProgramInfoLog(Handle));

            int status_code;
            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out status_code);
            if(status_code != 1)
                throw new ApplicationException("Linking error");

            GL.UseProgram(Handle);

            Console.WriteLine(GL.GetProgramInfoLog(Handle));

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

            Console.WriteLine(GL.GetShaderInfoLog(shader));
            int status_code;
            GL.GetShader(shader, ShaderParameter.CompileStatus, out status_code);
            if(status_code != 1)
                throw new ApplicationException("Compilation error");
            return shader;
        }
    }
}