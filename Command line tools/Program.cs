using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using SD = SharpDX.Toolkit.Graphics;

namespace Command_line_tools
{
    internal class Program
    {
        private static void CommandTexturePack(string albedo, string normal, string bump, string discard, string specular, string roughness, string metalness, string albedoDiscardOutput)
        {
            var albedoBitmap = albedo == "" ? null : new Bitmap(Image.FromFile(albedo));
            var normalBitmap = normal == "" ? null : new Bitmap(Image.FromFile(normal));
            var bumpBitmap = bump == "" ? null : new Bitmap(Image.FromFile(bump));
            var discardBitmap = discard == "" ? null : new Bitmap(Image.FromFile(discard));
            var specularBitmap = specular == "" ? null : new Bitmap(Image.FromFile(specular));
            var roughnessBitmap = roughness == "" ? null : new Bitmap(Image.FromFile(roughness));
            var metalnessBitmap = metalness == "" ? null : new Bitmap(Image.FromFile(metalness));

            int ais = albedoBitmap.Width * albedoBitmap.Height;
            var albedoalphaBuffer = new Color[ais];
            for(int i = 0; i < ais; i++)
            {
                var apix = albedoBitmap.GetPixel(i % albedoBitmap.Width, (int)Math.Floor((double)i / albedoBitmap.Width));
                var dpix = discardBitmap.GetPixel(i % albedoBitmap.Width, (int)Math.Floor((double)i / albedoBitmap.Width));
                albedoalphaBuffer[ais] = Color.FromArgb(dpix.R * 255, apix.R * 255, apix.G * 255, apix.B * 255);
            }

            var img = SD.Image.New2D(albedoBitmap.Width, albedoBitmap.Height, 0, SD.PixelFormat.R8G8B8A8.UNorm);
            img.PixelBuffer[0].SetPixels(albedoalphaBuffer);
            img.Save(albedoDiscardOutput, SharpDX.Toolkit.Graphics.ImageFileType.Dds);
        }

        private static void Main(string[] args)
        {
        }
    }
}