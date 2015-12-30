using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class ShaderStorageBuffer
    {
        public int last = 0;
        public BufferUsageHint Type = BufferUsageHint.DynamicDraw;
        public BufferTarget Target;

        private bool Generated;

        private int Handle = -1;

        public ShaderStorageBuffer(BufferTarget target = BufferTarget.ShaderStorageBuffer)
        {
            Target = target;
        }

        public static byte[] Serialize(dynamic structure)
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
                return buf.ToArray();
            }
            else
            {
                int size = Marshal.SizeOf(structure);
                byte[] arr = new byte[size];
                IntPtr ptr = Marshal.AllocHGlobal(size);

                Marshal.StructureToPtr(structure, ptr, true);
                Marshal.Copy(ptr, arr, 0, size);
                Marshal.FreeHGlobal(ptr);

                return arr;
            }
        }

        public void MapData(byte[] buffer)
        {
            if(!Generated)
            {
                Handle = GL.GenBuffer();
                Generated = true;
            }
            GL.BindBuffer(Target, Handle);
            GL.BufferData(Target, new IntPtr(buffer.Length), buffer, Type);
            last = buffer.Length;
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
            GL.BindBuffer(Target, Handle);
            GL.BufferSubData(Target, new IntPtr(start), new IntPtr(length), buffer);
        }

        public byte[] Read(int offset, int size)
        {
            GL.BindBuffer(Target, Handle);
            byte[] data = new byte[size];
            GL.GetBufferSubData<byte>(Target, new IntPtr(offset), new IntPtr(size), data);
            return data;
        }

        public void Release(uint point)
        {
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, point, 0);
        }

        public void Use(uint point)
        {
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, point, (uint)Handle);
        }
    }
}