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
        public int Handle = -1;
        Dictionary<string, int> UniformLocationsCache;

        public ShaderProgram(string vertex, string fragment)
        {
            UniformLocationsCache = new Dictionary<string, int>();

            int vertexShaderHandle = CompileSingleShader(ShaderType.VertexShader, vertex);
            int fragmentShaderHandle = CompileSingleShader(ShaderType.FragmentShader, fragment);

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

        }

        public void BindAttributeLocation(int index, string name)
        {
            GL.BindAttribLocation(Handle, index, name);
        }

        int GetUniformLocation(string name)
        {
            if (Handle == -1) return 0;
            if (UniformLocationsCache.ContainsKey(name)) return UniformLocationsCache[name];
            int location = GL.GetUniformLocation(Handle, name);
            UniformLocationsCache.Add(name, location);
            return location;
        }

        public void SetUniform(string name, Matrix4 data)
        {
            int location = GetUniformLocation(name);
            GL.UniformMatrix4(location, false, ref data);
        }

        public void SetUniform(string name, float data)
        {
            int location = GetUniformLocation(name);
            GL.Uniform1(location, data);
        }

        public void SetUniform(string name, Vector2 data)
        {
            int location = GetUniformLocation(name);
            GL.Uniform2(location, data);
        }

        public void SetUniform(string name, Vector3 data)
        {
            int location = GetUniformLocation(name);
            GL.Uniform3(location, data);
        }

        public void SetUniform(string name, Color4 data)
        {
            int location = GetUniformLocation(name);
            GL.Uniform4(location, data);
        }

        public void SetUniform(string name, Vector4 data)
        {
            int location = GetUniformLocation(name);
            GL.Uniform4(location, data);
        }

        public void Use()
        {
            //if (Current == this) return;
            GL.UseProgram(Handle);
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