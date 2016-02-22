using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class ShadowMapsArrayTexture
    {
        private int HandleDepths = -1;
        private int HandleColors = -1;
        private int Levels;
        private int Width, Height; 

        public ShadowMapsArrayTexture(int width, int height)
        {
            Width = width;
            Height = height;
            Levels = 0;
        }

        private void FreeGPU()
        {
            if(HandleDepths != -1)
                GL.DeleteTexture(HandleDepths);
            if(HandleColors != -1)
                GL.DeleteTexture(HandleColors);
        }

        private void Reallocate(int levels)
        {
            FreeGPU();
            HandleDepths = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DArray, HandleDepths);
            //GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.R32f, Width, Height, levels);
            GL.TexImage3D(TextureTarget.Texture2DArray, 0, PixelInternalFormat.DepthComponent32f, Width, Height, levels, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRefToTexture);

            HandleColors = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DArray, HandleColors);
            //GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.R32f, Width, Height, levels);
            GL.TexImage3D(TextureTarget.Texture2DArray, 0, PixelInternalFormat.Rgba16f, Width, Height, levels, 0, PixelFormat.Rgba, PixelType.HalfFloat, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            Levels = levels;
        }

        private void AttachLayer(int level)
        {
            GL.FramebufferTextureLayer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, HandleColors, 0, level);
            GL.FramebufferTextureLayer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, HandleDepths, 0, level);
        }

        public void UpdateFromLightsList(List<ProjectionLight> lights)
        {
            if(lights.Count != Levels) Reallocate(lights.Count);
            for(int i = 0; i < lights.Count; i++)
            {
                lights[i].FBO.Use();
                lights[i].FBO.Width = Width;
                lights[i].FBO.Height = Height;
                AttachLayer(i);
                lights[i].ShadowMapArrayIndex = i;
            }
        }

        public void Bind(int colorsloc, int depthloc)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + colorsloc);
            GL.BindTexture(TextureTarget.Texture2DArray, HandleColors);
            GL.ActiveTexture(TextureUnit.Texture0 + depthloc);
            GL.BindTexture(TextureTarget.Texture2DArray, HandleDepths);
        }
    }
}
