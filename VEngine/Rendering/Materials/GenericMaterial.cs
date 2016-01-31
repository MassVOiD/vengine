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

        public bool CastShadows = true;

        public bool SupportTransparency = false;

        public bool InvertUVYAxis = false;

        public bool IgnoreLighting = false;

        public bool InvertNormalMap = false;
        
        public DrawMode Mode;

        public string Name;

        public Texture DiffuseTexture, SpecularTexture, NormalsTexture, BumpTexture, RoughnessTexture, AlphaTexture;

        public float NormalMapScale = 1.0f;

        public float ParallaxHeightMultiplier = 1.0f;

        public int ParallaxInstances = 12;
        
        public float ReflectionStrength = 0;

        public float RefractionStrength = 0;

        public float Roughness = 0.5f;

        public Vector3 SpecularColor = new Vector3(0.7f), DiffuseColor = new Vector3(0.3f);

        public float Alpha = 1.0f;

        public float TesselationMultiplier = 1.0f;

        public MaterialType Type;

        private ShaderProgram lastUserProgram = null;

        private RainSystem RainsDropSystem = null;

        public GenericMaterial()
        {
        }

        public GenericMaterial(Vector3 color)
        {
            DiffuseColor = color;
            Mode = GenericMaterial.DrawMode.ColorOnly;
        }

        public GenericMaterial(Color color)
        {
            DiffuseColor = new Vector3(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);
            Mode = GenericMaterial.DrawMode.ColorOnly;
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
            Diffuse,
            RandomlyDisplaced,
            Water,
            Sky,
            WetDrops,
            Grass,
            PlanetSurface,
            TessellatedTerrain,
            Flag,

            Plastic,
            Metal,

            Parallax,

            DropsSystem,
            OptimizedSpheres
        }
        
        public ShaderProgram GetShaderProgram()
        {
            var pack = OverrideShaderPack != null ? OverrideShaderPack : Game.ShaderPool.ChooseShaderGenericMaterial();
            if(Type == MaterialType.Grass || Type == MaterialType.Flag)
                return pack.Geometry96iTriangles;
            if(Type == MaterialType.TessellatedTerrain)
                return pack.TesselatedProgram;
            if(Type == MaterialType.Parallax)
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

        public void SetRainSystem(RainSystem rs)
        {
            RainsDropSystem = rs;
        }
        
        public bool Use()
        {
            var prg = GetShaderProgram();
            // if(prg.) {
            //GL.Disable(EnableCap.CullFace);
            // }
            if(lastUserProgram == null)
                lastUserProgram = ShaderProgram.Current;
            ShaderProgram.SwitchResult res = prg.Use();
            //  if(res == ShaderProgram.SwitchResult.Locked)
            //  {
            //      prg = ShaderProgram.Current;
            // }
            // if(lastUserProgram == ShaderProgram.Current)
            //      return true;
            //lastUserProgram = ShaderProgram.Current;
            prg.SetUniform("TesselationMultiplier", TesselationMultiplier);
            prg.SetUniform("InvertUVy", InvertUVYAxis);
            if(NormalsTexture != null)
            {
                prg.SetUniform("UseNormalsTex", 1);
                prg.SetUniform("InvertNormalMap", InvertNormalMap);
                prg.SetUniform("NormalMapScale", NormalMapScale);
                //NormalsTexture.Use(TextureUnit.Texture2);
                NormalsTexture.UseBindlessHandle("normalsTexAddr");
            }
            else
                prg.SetUniform("UseNormalsTex", 0);

            if(BumpTexture != null)
            {
                prg.SetUniform("UseBumpTex", 1);
                BumpTexture.Use(TextureUnit.Texture3);
            }
            else
                prg.SetUniform("UseBumpTex", 0);
            
            if(RoughnessTexture != null)
            {
                prg.SetUniform("UseRoughnessTex", 1);
               // RoughnessTexture.Use(TextureUnit.Texture7);
                RoughnessTexture.UseBindlessHandle("roughnessTexAddr");
            }
            else
                prg.SetUniform("UseRoughnessTex", 0);

            if(AlphaTexture != null)
            {
                prg.SetUniform("UseAlphaTex", 1);
                AlphaTexture.Use(TextureUnit.Texture4);
            }
            else
                prg.SetUniform("UseAlphaTex", 0);

            if(DiffuseTexture != null)
            {
                prg.SetUniform("UseDiffuseTex", 1);
                //DiffuseTexture.Use(TextureUnit.Texture5);
                DiffuseTexture.UseBindlessHandle("diffuseTexAddr");
            }
            else
                prg.SetUniform("UseDiffuseTex", 0);

            if(SpecularTexture != null)
            {
                prg.SetUniform("UseSpecularTex", 1);
                SpecularTexture.UseBindlessHandle("specularTexAddr");
            }
            else
                prg.SetUniform("UseSpecularTex", 0);

            if(Type == MaterialType.Grass)
            {
                GL.Disable(EnableCap.CullFace);
            }
            
            //prg.SetUniform("input_Color", DiffuseColor);
            prg.SetUniform("DrawMode", (int)Mode);
            prg.SetUniform("MaterialType", (int)Type);

            if(Type == MaterialType.DropsSystem && RainsDropSystem != null)
                RainsDropSystem.MapToCurrentShader();


            // pbr stuff
            prg.SetUniform("SpecularColor", SpecularColor);
            prg.SetUniform("DiffuseColor", DiffuseColor);
            prg.SetUniform("Alpha", Alpha);


            prg.SetUniform("Roughness", Roughness);
            prg.SetUniform("ReflectionStrength", ReflectionStrength);
            prg.SetUniform("RefractionStrength", RefractionStrength);
            prg.SetUniform("IgnoreLighting", IgnoreLighting);
            prg.SetUniform("ParallaxHeightMultiplier", ParallaxHeightMultiplier);
            prg.SetUniform("ParallaxInstances", ParallaxInstances);

        //    if(Color.W < 1.0f)
         //       GL.DepthMask(false);
         //   else
        //        GL.DepthMask(true);

            // prg.SetUniform("AORange", AORange);
            // prg.SetUniform("AOStrength", AOStrength);
            // prg.SetUniform("AOAngleCutoff", AOAngleCutoff);
            //  prg.SetUniform("VDAOMultiplier", VDAOMultiplier);
            // prg.SetUniform("VDAOSamplingMultiplier", VDAOSamplingMultiplier);
            // prg.SetUniform("VDAORefreactionMultiplier", VDAORefreactionMultiplier);
            //prg.SetUniform("SubsurfaceScatteringMultiplier", SubsurfaceScatteringMultiplier);
            // prg.SetUniform("FrameINT", (int)PostProcessing.RandomIntFrame);

            // GL.BindImageTexture(22u, (uint)PostProcessing.FullScene3DTexture.Handle, 0, false, 0,
            // TextureAccess.ReadWrite, SizedInternalFormat.R32ui);

            return true;
        }
    }
}