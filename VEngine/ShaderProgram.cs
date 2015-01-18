using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace VDGTech
{
    public class ShaderProgram
    {
        public static ShaderProgram Current = null;
        int Handle = -1;
        Dictionary<string, int> UniformLocationsCache;
        string VertexSource, FragmentSource;
        bool Compiled;
        static public bool Lock = false;

        public ShaderProgram(string vertex, string fragment)
        {
            UniformLocationsCache = new Dictionary<string, int>();

            VertexSource = vertex;
            FragmentSource = fragment;
            Compiled = false;
        }

        void Compile()
        {
            int vertexShaderHandle = CompileSingleShader(ShaderType.VertexShader, VertexSource);
            int fragmentShaderHandle = CompileSingleShader(ShaderType.FragmentShader, FragmentSource);

            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, vertexShaderHandle);
            GL.AttachShader(Handle, fragmentShaderHandle);

            GL.LinkProgram(Handle);

            int status_code;
            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out status_code);
            if (status_code != 1)
                throw new ApplicationException("Linking error");

            GL.UseProgram(Handle);

            Console.WriteLine(GL.GetProgramInfoLog(Handle));

            Compiled = true;
        }

        public void BindAttributeLocation(int index, string name)
        {
            GL.BindAttribLocation(Handle, index, name);
        }

        static int GetUniformLocation(string name)
        {
            if (Current.Handle == -1) return -1;
            if (Current.UniformLocationsCache.ContainsKey(name) && !Lock) return Current.UniformLocationsCache[name];
            int location = GL.GetUniformLocation(Current.Handle, name);
            GLThread.CheckErrors();
            if (!Lock) Current.UniformLocationsCache.Add(name, location);
            if (Lock && name == "Time") return -1;
            return location;
        }

        public void SetUniform(string name, Matrix4 data)
        {
            int location = GetUniformLocation(name);
            if(location >= 0) GL.UniformMatrix4(location, false, ref data);
        }

        public void SetUniform(string name, float data)
        {
            int location = GetUniformLocation(name);
            if (location >= 0) GL.Uniform1(location, data);
        }
        public void SetUniform(string name, int data)
        {
            int location = GetUniformLocation(name);
            if (location >= 0) GL.Uniform1(location, data);
        }

        public void SetUniform(string name, Vector2 data)
        {
            int location = GetUniformLocation(name);
            if (location >= 0) GL.Uniform2(location, data);
        }

        public void SetUniform(string name, Vector3 data)
        {
            int location = GetUniformLocation(name);
            if (location >= 0) GL.Uniform3(location, data);
        }

        public void SetUniform(string name, Color4 data)
        {
            int location = GetUniformLocation(name);
            if (location >= 0) GL.Uniform4(location, data);
        }

        public void SetUniform(string name, Vector4 data)
        {
            int location = GetUniformLocation(name);
            if (location >= 0) GL.Uniform4(location, data);
        }

        public void Use()
        {
            //if (Current == this) return;
            if (!Compiled) Compile();
            if(!Lock) GL.UseProgram(Handle);
            Current = this;
        }

        private int CompileSingleShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);

            GL.ShaderSource(shader, source);

            GL.CompileShader(shader);

            Console.WriteLine(GL.GetShaderInfoLog(shader));
            int status_code;
            GL.GetShader(shader, ShaderParameter.CompileStatus, out status_code);
            if (status_code != 1)
                throw new ApplicationException("Compilation error");
            return shader;
        }
    }
}