using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class GenericMaterial
    {
        public static ShaderPool.ShaderPack OverrideShaderPack = null;

        public string Name;

        public Texture DiffuseTexture, SpecularTexture, NormalsTexture, BumpTexture, RoughnessTexture, AlphaTexture;

        public float ParallaxHeightMultiplier = 1.0f;

        public int BufferOffset = -1;

        public float Roughness = 0.5f;

        public Vector3 SpecularColor = new Vector3(0.7f), DiffuseColor = new Vector3(0.3f);

        public float Alpha = 1.0f;

        public bool UseForwardRenderer = false;

        public bool InvertUVy = false;

        public bool IsBillboard = false;

        public float TessellationMultiplier = 1.0f;

        public MaterialType Type;

        private ShaderProgram lastUserProgram = null;

        private static List<GenericMaterial> AllMaterialsPool = new List<GenericMaterial>();
        //private static MaterialsBuffer Buffer = new MaterialsBuffer();

        public ShaderProgram CustomShaderProgram = null;

        public enum BlendingEnum
        {
            AdditiveNormalized,
            Alpha
        }

        public BlendingEnum Blending = GenericMaterial.BlendingEnum.Alpha;

        public GenericMaterial()
        {
            AllMaterialsPool.Add(this);
            //  Buffer.Update(AllMaterialsPool);
        }

        ~GenericMaterial()
        {
            AllMaterialsPool.Remove(this);
            //Buffer.Update(AllMaterialsPool);
        }

        public GenericMaterial(Vector3 color)
        {
            AllMaterialsPool.Add(this);
            //  Buffer.Update(AllMaterialsPool);
        }

        public GenericMaterial(Color color)
        {
            AllMaterialsPool.Add(this);
            // Buffer.Update(AllMaterialsPool);
        }

        public enum DrawMode
        {
            TextureOnly,
            ColorOnly,
            TextureMultipleColor,
            OneMinusColorOverTexture
        }

        public enum MaterialType
        {
            Generic,
            Water,
            Grass,
            PlanetSurface,
            TessellatedTerrain,
            AdvancedParallax,
            Flag
        }

        public ShaderProgram GetShaderProgram()
        {
            if(CustomShaderProgram != null)
                return CustomShaderProgram;
            var pack = OverrideShaderPack != null ? OverrideShaderPack : Game.ShaderPool.ChooseShaderGenericMaterial(UseForwardRenderer);
            if(Type == MaterialType.Grass || Type == MaterialType.Flag)
                return pack.Geometry96iTriangles;
            if(Type == MaterialType.TessellatedTerrain)
                return pack.TesselatedProgram;
            if(Type == MaterialType.AdvancedParallax)
                return pack.Geometry96iTriangles;
            return Type == MaterialType.Water || Type == MaterialType.PlanetSurface ||
               Type == MaterialType.TessellatedTerrain || Type == MaterialType.Grass ? pack.TesselatedProgram : pack.Program;
        }

        public void SetBumpTexture(string key)
        {
            BumpTexture = new Texture(Media.Get(key));
        }

        public void SetNormalsTexture(string key)
        {
            NormalsTexture = new Texture(Media.Get(key));
        }

        public void SetDiffuseTexture(string key)
        {
            DiffuseTexture = new Texture(Media.Get(key));
        }

        public void SetSpecularTexture(string key)
        {
            SpecularTexture = new Texture(Media.Get(key));
        }

        public void SetAlphaTexture(string key)
        {
            AlphaTexture = new Texture(Media.Get(key));
        }

        public void SetRoughnessTexture(string key)
        {
            RoughnessTexture = new Texture(Media.Get(key));
        }

        public bool Use()
        {
            var prg = GetShaderProgram();

            if(lastUserProgram == null)
                lastUserProgram = ShaderProgram.Current;
            ShaderProgram.SwitchResult res = prg.Use();

            //prg.SetUniform("MaterialIndex", BufferOffset);
            prg.SetUniform("IsTessellatedTerrain", Type == MaterialType.TessellatedTerrain);
            prg.SetUniform("TessellationMultiplier", TessellationMultiplier);
            prg.SetUniform("InvertUVy", InvertUVy);
            prg.SetUniform("IsBillboard", IsBillboard);

            prg.SetUniform("SpecularColor", SpecularColor);
            prg.SetUniform("DiffuseColor", DiffuseColor);
            prg.SetUniform("ParallaxHeightMultiplier", ParallaxHeightMultiplier);
            prg.SetUniform("Roughness", Roughness);
            prg.SetUniform("Alpha", Alpha);

            prg.SetUniform("NormalTexEnabled", NormalsTexture != null);
            prg.SetUniform("BumpTexEnabled", BumpTexture != null);
            prg.SetUniform("AlphaTexEnabled", AlphaTexture != null);
            prg.SetUniform("RoughnessTexEnabled", RoughnessTexture != null);
            prg.SetUniform("DiffuseTexEnabled", DiffuseTexture != null);
            prg.SetUniform("SpecularTexEnabled", SpecularTexture != null);

            if(NormalsTexture != null)
                NormalsTexture.Use(prg.getConstInt("normalsTexBind"));

            if(BumpTexture != null)
                BumpTexture.Use(prg.getConstInt("bumpTexBind"));

            if(AlphaTexture != null)
                AlphaTexture.Use(prg.getConstInt("alphaTexBind"));

            if(RoughnessTexture != null)
                RoughnessTexture.Use(prg.getConstInt("roughnessTexBind"));

            if(DiffuseTexture != null)
                DiffuseTexture.Use(prg.getConstInt("diffuseTexBind"));

            if(SpecularTexture != null)
                SpecularTexture.Use(prg.getConstInt("specularTexBind"));

            return true;
        }
    }
}