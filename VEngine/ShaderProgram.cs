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
        private int handle;
        Dictionary<string, int> UniformLocationsCache;

        public ShaderProgram(string vertex, string fragment)
        {
            UniformLocationsCache = new Dictionary<string, int>();

            int vertexShaderHandle = CompileSingleShader(ShaderType.VertexShader, vertex);
            int fragmentShaderHandle = CompileSingleShader(ShaderType.FragmentShader, fragment);

            handle = GL.CreateProgram();

            GL.AttachShader(handle, vertexShaderHandle);
            GL.AttachShader(handle, fragmentShaderHandle);

            GL.LinkProgram(handle);

            int status_code;
            GL.GetProgram(handle, GetProgramParameterName.LinkStatus, out status_code);
            if (status_code != 1)
                throw new ApplicationException("Linking error");

            GL.UseProgram(handle);

            Console.WriteLine(GL.GetProgramInfoLog(handle));
        }

        public void BindAttributeLocation(int index, string name)
        {
            GL.BindAttribLocation(handle, index, name);
        }

        int GetUniformLocation(string name)
        {
            if (UniformLocationsCache.ContainsKey(name)) return UniformLocationsCache[name];
            int location = GL.GetUniformLocation(handle, name);
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
            GL.UseProgram(handle);
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