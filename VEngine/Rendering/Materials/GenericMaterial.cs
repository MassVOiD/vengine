using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class GenericMaterial
    {
        public Texture Tex;
        public Vector4 Color;
        public float SpecularComponent = 1.0f, DiffuseComponent = 1.0f;
        public float ReflectionStrength = 0;
        public float RefractionStrength = 0;
        public float Roughness = 0.5f;
        public float Metalness = 0.5f;

        public bool CastShadows = true;
        public bool ReceiveShadows = true;
        public bool IgnoreLighting = false;

        public ShaderProgram Program, TesselatedProgram;
        public Texture NormalMap, BumpMap, AlphaMask, RoughnessMap, MetalnessMap, SpecularMap;
        public float NormalMapScale = 1.0f;
        public float TesselationMultiplier = 1.0f;
        public string Name;

        public enum DrawMode
        {
            TextureOnly,
            ColorOnly,
            TextureMultipleColor,
            OneMinusColorOverTexture
        }


        public DrawMode Mode;

        void CompileShaders()
        {
            if(Program == null)
                Program = ShaderProgram.Compile("Generic.vertex.glsl",
                    "Generic.fragment.glsl");
            if(TesselatedProgram == null)
                TesselatedProgram = ShaderProgram.Compile("Tesselation.vertex.glsl",
                    "Generic.fragment.glsl", null, "Generic.tesscontrol.glsl", "Generic.tesseval.glsl");
        }
        public static GenericMaterial FromMedia(string vertex, string fragment, string geometry = null, string tesscontrol = null, string tesseval = null)
        {
            return new GenericMaterial(Vector4.Zero){
                Program = ShaderProgram.Compile(vertex, fragment, geometry, tesscontrol, tesseval)
            };
        }
        public static GenericMaterial FromName(string name)
        {
            return GenericMaterial.FromMedia(name + ".vertex.glsl", name + ".fragment.glsl");
        }

        public GenericMaterial(Texture tex, Texture normalMap = null, Texture bumpMap = null)
        {
            CompileShaders();
            Tex = tex;
            NormalMap = normalMap;
            bumpMap = bumpMap;
            Color = Vector4.One;
            Mode = GenericMaterial.DrawMode.TextureOnly;
        }
        public GenericMaterial(Vector4 color)
        {
            CompileShaders();
            Color = color;
            Mode = GenericMaterial.DrawMode.ColorOnly;
        }
        public GenericMaterial(Color color)
        {
            CompileShaders();
            Color = new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
            Mode = GenericMaterial.DrawMode.ColorOnly;
        }

        public static GenericMaterial FromMedia(string key)
        {
            return new GenericMaterial(new Texture(Media.Get(key)));
        }

        public static GenericMaterial FromMedia(string key, string normalmap_key, string bump_map)
        {
            return new GenericMaterial(new Texture(Media.Get(key)), new Texture(Media.Get(normalmap_key)), new Texture(Media.Get(bump_map)));
        }


        public bool Use()
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
                NormalMap.Use(TextureUnit.Texture27);
            }
            else
                prg.SetUniform("UseNormalMap", 0);

            if(BumpMap != null)
            {
                prg.SetUniform("UseBumpMap", 1);
                BumpMap.Use(TextureUnit.Texture29);
            }
            else
                prg.SetUniform("UseBumpMap", 0);

            if(SpecularMap != null)
            {
                prg.SetUniform("UseSpecularMap", 1);
                SpecularMap.Use(TextureUnit.Texture31 + 1);
            }
            else
                prg.SetUniform("UseSpecularMap", 0);
            if(RoughnessMap != null)
            {
                prg.SetUniform("UseRoughnessMap", 1);
                RoughnessMap.Use(TextureUnit.Texture30);
            }
            else
                prg.SetUniform("UseRoughnessMap", 0);
            if(MetalnessMap != null)
            {
                prg.SetUniform("UseMetalnessMap", 1);
                MetalnessMap.Use(TextureUnit.Texture31);
            }
            else
                prg.SetUniform("UseMetalnessMap", 0);


            if(AlphaMask != null)
            {
                prg.SetUniform("UseAlphaMask", 1);
                AlphaMask.Use(TextureUnit.Texture28);
                //GL.DepthFunc(DepthFunction.Always);
                GL.Disable(EnableCap.CullFace);
            }
            else
            {
                prg.SetUniform("UseAlphaMask", 0);
                GL.Enable(EnableCap.CullFace);
                //GL.DepthFunc(DepthFunction.Lequal);
            }

            if(Tex != null)
                Tex.Use(TextureUnit.Texture0);

            prg.SetUniform("input_Color", Color);
            prg.SetUniform("DrawMode", (int)Mode);
            prg.SetUniform("MaterialType", (int)Type);

            prg.SetUniform("SpecularComponent", SpecularComponent);
            prg.SetUniform("DiffuseComponent", DiffuseComponent);
            prg.SetUniform("Roughness", Roughness);
            prg.SetUniform("Metalness", Metalness);
            prg.SetUniform("ReflectionStrength", ReflectionStrength);
            prg.SetUniform("RefractionStrength", RefractionStrength);
            prg.SetUniform("IgnoreLighting", IgnoreLighting);
           // prg.SetUniform("FrameINT", (int)PostProcessing.RandomIntFrame);

         //   GL.BindImageTexture(22u, (uint)PostProcessing.FullScene3DTexture.Handle, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);

            return res;
        }


        public enum MaterialType
        {
            Solid,
            RandomlyDisplaced,
            Water,
            Sky,
            WetDrops,
            Grass,
            PlanetSurface, 
            TessellatedTerrain
        }
        public MaterialType Type;


        public void SetRoughnessMapFromMedia(string key)
        {
            RoughnessMap = new Texture(Media.Get(key));
        }

        public void SetMetalnessMapFromMedia(string key)
        {
            MetalnessMap = new Texture(Media.Get(key));
        }

        public void SetSpecularMapFromMedia(string key)
        {
            SpecularMap = new Texture(Media.Get(key));
        }

        public void SetBumpMapFromMedia(string key)
        {
            BumpMap = new Texture(Media.Get(key));
        }
        public void SetTextureFromMedia(string key)
        {
            Tex = new Texture(Media.Get(key));
        }
        public void SetNormalMapFromMedia(string key)
        {
            NormalMap = new Texture(Media.Get(key));
        }
        public void SetAlphaMaskFromMedia(string key)
        {
            AlphaMask = new Texture(Media.Get(key));
        }

        public ShaderProgram GetShaderProgram()
        {
            return Type == MaterialType.Water || Type == MaterialType.PlanetSurface ||
               Type == MaterialType.TessellatedTerrain || Type == MaterialType.Grass ? TesselatedProgram : Program;
        }

    }
}