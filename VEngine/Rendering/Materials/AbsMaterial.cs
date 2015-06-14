using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public abstract class AbsMaterial : IMaterial
    {
        public ShaderProgram Program, TesselatedProgram;
        public Texture NormalMap, BumpMap;
        public float NormalMapScale = 1.0f;
        public float TesselationMultiplier = 1.0f;

        public enum MaterialType
        {
            Solid,
            RandomlyDisplaced,
            Water,
            Sky,
            WetDrops,
            Grass,
            PlanetSurface
        }
        public MaterialType Type;

        public void SetBumpMapFromMedia(string bumpmapKey)
        {
            BumpMap = new Texture(Media.Get(bumpmapKey));
        }
        public void SetNormalMapFromMedia(string mapKey)
        {
            NormalMap = new Texture(Media.Get(mapKey));
        }

        public ShaderProgram GetShaderProgram()
        {
            return Type == MaterialType.Water || Type == MaterialType.PlanetSurface? TesselatedProgram : Program;
        }

        public virtual bool Use()
        {
            var prg = GetShaderProgram();
            bool res = prg.Use();
            if(res == false)
                prg = ShaderProgram.Current;
            prg.SetUniform("TesselationMultiplier", TesselationMultiplier);
            if(NormalMap != null)
            {
                prg.SetUniform("UseNormalMap", 1);
                prg.SetUniform("NormalMapScale", NormalMapScale);
                NormalMap.Use(TextureUnit.Texture1);
            }
            else
                prg.SetUniform("UseNormalMap", 0);

            if(BumpMap != null)
            {
                prg.SetUniform("UseBumpMap", 1);
                BumpMap.Use(TextureUnit.Texture31);
            }
            else
                prg.SetUniform("UseBumpMap", 0);
            return res;
        }
    }
}
