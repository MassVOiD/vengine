using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OldGL = OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;

namespace VDGTech
{
    public class ShaderStorageBuffer
    {
        uint Handle = -1;
        bool Generated;
        byte[] data;
        public ShaderStorageBuffer()
        {
        }

        public void MapData(byte[] buffer)
        {
            Handle = (uint)GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, Handle);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, new IntPtr(buffer.Length), buffer, BufferUsageHint.DynamicCopy);
        }

        public void Use(uint point)
        {
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, point, Handle);
        }

    }
}
