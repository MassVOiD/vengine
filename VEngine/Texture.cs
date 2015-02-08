﻿using System;
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
    public class Texture
    {
        int Handle = -1;
        bool Generated;
        byte[] Bitmap;
        Size Size;
        private Texture()
        {
        }

        public Texture(string file)
        {
            Update(file);
        }
        public Texture(int handle)
        {
            Handle = handle;
            Generated = true;
        }

        public Texture(Bitmap bitmap)
        {
            Update(bitmap);
        }

        public void Update(string file)
        {
            var bitmap = new Bitmap(Image.FromFile(file));
            Generated = false;
            if(Handle >= 0)
            {
                GL.DeleteTexture(Handle);
            }
            Bitmap = BitmapToByteArray(bitmap);
            Size = bitmap.Size;
        }

        public void Update(Bitmap bitmap)
        {
            Generated = false;
            if(Handle >= 0)
            {
                GL.DeleteTexture(Handle);
            }
            Bitmap = BitmapToByteArray(bitmap);
            Size = bitmap.Size;
        }

        public void Use(TextureUnit unit)
        {
            if(!Generated)
            {
                Handle = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, Handle);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Size.Width, Size.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, Bitmap);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.LinearSharpenColorSgis);
                //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.MirroredRepeat);
                //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.MirroredRepeat);
                //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, (int)TextureMagFilter.Linear);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                float maxAniso;
                GL.GetFloat((GetPName)OldGL.ExtTextureFilterAnisotropic.MaxTextureMaxAnisotropyExt, out maxAniso);
                GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)OldGL.ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, maxAniso);
                Generated = true;
            }
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }

        private static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            BitmapData bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            int numbytes = bmpdata.Stride * bitmap.Height;
            byte[] bytedata = new byte[numbytes];
            IntPtr ptr = bmpdata.Scan0;
            Marshal.Copy(ptr, bytedata, 0, numbytes);
            bitmap.UnlockBits(bmpdata);
            return bytedata;

        }
        static int nlpo2(int x)
        {
            x--; // comment out to always take the next biggest power of two, even if x is already a power of two
            x |= (x >> 1);
            x |= (x >> 2);
            x |= (x >> 4);
            x |= (x >> 8);
            x |= (x >> 16);
            return (x + 1);
        }
        public static Texture FromText(string text, string font, float size, Color textColor, Color background)
        {
            var tex = new Texture();
            tex.UpdateFromText(text, font, size, textColor, background);
            return tex;

        }
        public void UpdateFromText(string text, string font, float size, Color textColor, Color background)
        {
            Bitmap bmp = new Bitmap(1, 1);
            var textSize = Graphics.FromImage(bmp).MeasureString(text, new Font(font, size), new PointF(0, 0), StringFormat.GenericDefault);
            bmp = new Bitmap((int)textSize.Width, (int)textSize.Height);

            RectangleF rectf = new RectangleF(0, 0, nlpo2((int)textSize.Width), nlpo2((int)textSize.Height));

            Graphics g = Graphics.FromImage(bmp);
            g.FillRectangle(new SolidBrush(background), rectf);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.DrawString(text, new Font(font, size), new SolidBrush(textColor), rectf);

            g.Flush();
            Update(bmp);

        }
    }
}
