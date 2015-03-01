using System;
using OpenTK.Graphics.OpenGL4;

namespace VDGTech
{
    public class ShaderStorageBuffer
    {
        public ShaderStorageBuffer()
        {
        }

        private byte[] data;
        private bool Generated;
        private int Handle = -1;

        public void MapData(byte[] buffer)
        {
            Handle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, Handle);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, new IntPtr(buffer.Length), buffer, BufferUsageHint.DynamicCopy);
        }

        public void Use(uint point)
        {
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, point, (uint)Handle);
        }
    }
}