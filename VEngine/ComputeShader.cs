using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    internal class ComputeShader
    {
        public ComputeShader(string source, string fragment)
        {
            UniformLocationsCache = new Dictionary<string, int>();

            ComputeSource = source;
            Compiled = false;
        }

        public static ComputeShader Current = null;
        static public bool Lock = false;
        private bool Compiled;
        private string ComputeSource;
        private int Handle = -1;
        private Dictionary<string, int> UniformLocationsCache;

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
            for(int i = 0; i < data.Length; i++)
            {
                int location = GetUniformLocation(name + "_" + i);
                if(location >= 0)
                {
                    GL.UniformMatrix4(location, false, ref data[i]);
                    GLThread.CheckErrors();
                }
            }
        }

        public void SetUniformArray(string name, Vector3[] data)
        {
            for(int i = 0; i < data.Length; i++)
            {
                int location = GetUniformLocation(name + "_" + i);
                if(location >= 0)
                {
                    GL.Uniform3(location, data[i]);
                    GLThread.CheckErrors();
                }
            }
        }

        public void Use()
        {
            if(!Lock)
            {
                if(Current == this)
                    return;
                if(!Compiled)
                    Compile();
                if(!Lock)
                    GL.UseProgram(Handle);
                Current = this;
            }
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
            int shaderHandle = CompileSingleShader(ShaderType.ComputeShader, ComputeSource);

            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, shaderHandle);

            GL.LinkProgram(Handle);

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

            GL.ShaderSource(shader, source);

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