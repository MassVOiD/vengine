using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class ShaderStorageBuffer
    {
        public BufferUsageHint Type = BufferUsageHint.DynamicDraw;

        private bool Generated;

        private int Handle = -1;

        public ShaderStorageBuffer()
        {
        }

        public void MapData(byte[] buffer)
        {
            if(!Generated)
            {
                Handle = GL.GenBuffer();
                Generated = true;
            }
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, Handle);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, new IntPtr(buffer.Length), buffer, Type);
        }

        public void MapData(dynamic structure)
        {
            if(structure.GetType().IsArray)
            {
                var objs = structure;
                var buf = new List<byte>();
                for(int i = 0; i < objs.Length; i++)
                {
                    int size = Marshal.SizeOf(structure[i]);
                    byte[] arr = new byte[size];
                    IntPtr ptr = Marshal.AllocHGlobal(size);

                    Marshal.StructureToPtr(structure[i], ptr, true);
                    Marshal.Copy(ptr, arr, 0, size);
                    Marshal.FreeHGlobal(ptr);

                    buf.AddRange(arr);
                }
                MapData(buf.ToArray());
            }
            else
            {
                int size = Marshal.SizeOf(structure);
                byte[] arr = new byte[size];
                IntPtr ptr = Marshal.AllocHGlobal(size);

                Marshal.StructureToPtr(structure, ptr, true);
                Marshal.Copy(ptr, arr, 0, size);
                Marshal.FreeHGlobal(ptr);

                MapData(arr);
            }
        }

        public void MapSubData(byte[] buffer, uint start, uint length)
        {
            if(!Generated)
            {
                Handle = GL.GenBuffer();
                Generated = true;
            }
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, Handle);
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, new IntPtr(start), new IntPtr(length), buffer);
        }

        public byte[] Read(int offset, int size)
        {
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, Handle);
            byte[] data = new byte[size];
            GL.GetBufferSubData<byte>(BufferTarget.ShaderStorageBuffer, new IntPtr(offset), new IntPtr(size), data);
            return data;
        }

        public void Use(uint point)
        {
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, point, (uint)Handle);
        }
    }
}