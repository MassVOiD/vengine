using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class ComputeShader
    {
        public static ComputeShader Current = null;

        static public bool Lock = false;

        private static List<ComputeShader> AllComputeShaders = new List<ComputeShader>();

        private bool Compiled;

        private string ComputeFile;

        private int Handle = -1;

        private Dictionary<string, int> UniformLocationsCache;

        public ComputeShader(string file)
        {
            UniformLocationsCache = new Dictionary<string, int>();
            ComputeFile = file;
            AllComputeShaders.Add(this);
            Compiled = false;
        }

        public static void RecompileAll()
        {
            AllComputeShaders.ForEach((a) => a.Compile());
        }

        public void BindAttributeLocation(int index, string name)
        {
            GL.BindAttribLocation(Handle, index, name);
        }

        public void Dispatch(int x, int y = 1, int z = 1)
        {
            GL.DispatchCompute(x, y, z);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
        }

        public void SetUniform(string name, Matrix4 data)
        {
            int location = GetUniformLocation(name);
            if(location >= 0)
                GL.UniformMatrix4(location, false, ref data);
        }

        public void SetUniform(string name, bool data)
        {
            int location = GetUniformLocation(name);
            if(location >= 0)
                GL.Uniform1(location, data ? 1 : 0);
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
                Game.CheckErrors(name);
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
                Game.CheckErrors(name);
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
            if(location >= 0)
            {
                GL.Uniform2(location, data.Length, floats.ToArray());
                Game.CheckErrors(name);
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
                Game.CheckErrors(name);
            }
        }

        public void SetUniformArray(string name, float[] data)
        {
            int location = GetUniformLocation(name);
            if(location >= 0)
            {
                GL.Uniform1(location, data.Length, data);
                Game.CheckErrors(name);
            }
        }

        public void SetUniformArray(string name, int[] data)
        {
            int location = GetUniformLocation(name);
            if(location >= 0)
            {
                GL.Uniform1(location, data.Length, data);
                Game.CheckErrors(name);
            }
        }

        public void Use()
        {
            if(!Compiled)
                Compile();
            GL.UseProgram(Handle);
            Current = this;
        }

        private static int GetUniformLocation(string name)
        {
            if(Current.Handle == -1)
                return -1;
            int location = GL.GetUniformLocation(Current.Handle, name);
            Game.CheckErrors();
            return location;
        }

        private void Compile()
        {
            int shaderHandle = CompileSingleShader(ShaderType.ComputeShader, ShaderPreparser.Preparse(ComputeFile, Media.ReadAllText(ComputeFile)));

            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, shaderHandle);

            GL.LinkProgram(Handle);

            int status_code;
            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out status_code);
            Console.WriteLine(GL.GetProgramInfoLog(Handle));
            if(status_code != 1)
                throw new ApplicationException("Linking error");

            GL.UseProgram(Handle);

            Console.WriteLine(GL.GetProgramInfoLog(Handle));

            Compiled = true;
        }

        private int CompileSingleShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);

            Console.WriteLine("Compiling compute shader {0}", ComputeFile);
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