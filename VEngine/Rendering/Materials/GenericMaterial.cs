using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class GenericMaterial
    {
        public Texture Tex;
        public Vector4 Color;
        public float SpecularSize = 1.0f, SpecularComponent = 1.0f, DiffuseComponent = 1.0f;
        public float ReflectionStrength = 0;
        public float RefractionStrength = 0;

        public bool CastShadows = true;
        public bool ReceiveShadows = true;
        public bool IgnoreLighting = false;

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
            if(AlphaMask != null)
            {
                prg.SetUniform("UseAlphaMask", 1);
                AlphaMask.Use(TextureUnit.Texture2);
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
            prg.SetUniform("SpecularSize", SpecularSize);
            prg.SetUniform("ReflectionStrength", ReflectionStrength);
            prg.SetUniform("RefractionStrength", RefractionStrength);
            prg.SetUniform("IgnoreLighting", IgnoreLighting);

            return res;
        }

        public ShaderProgram Program, TesselatedProgram;
        public Texture NormalMap, BumpMap, AlphaMask;
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
        public void SetTextureFromMedia(string textKey)
        {
            Tex = new Texture(Media.Get(textKey));
        }
        public void SetNormalMapFromMedia(string mapKey)
        {
            NormalMap = new Texture(Media.Get(mapKey));
        }
        public void SetAlphaMaskFromMedia(string key)
        {
            AlphaMask = new Texture(Media.Get(key));
        }

        public ShaderProgram GetShaderProgram()
        {
            return Type == MaterialType.Water || Type == MaterialType.PlanetSurface ? TesselatedProgram : Program;
        }

    }
}