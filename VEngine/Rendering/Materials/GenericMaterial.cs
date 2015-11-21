using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class GenericMaterial
    {
        public class ShaderPack
        {
            public ShaderProgram
                Program,
                TesselatedProgram,
                //Geometry1iLines,
                Geometry1iTriangles,
                //Geometry1iPoints,
                //Geometry32iLines,
                //Geometry32iTriangles,
                // Geometry32iPoints,
                //Geometry96iLines,
                Geometry96iTriangles;
            //Geometry96iPoints;
            /* TesselatedGeometry1iLines,
             TesselatedGeometry1iTriangles,
             TesselatedGeometry1iPoints,
             TesselatedGeometry32iLines,
             TesselatedGeometry32iTriangles,
             TesselatedGeometry32iPoints,
             TesselatedGeometry96iLines,
             TesselatedGeometry96iTriangles,
             TesselatedGeometry96iPoints;*/

            public List<ShaderProgram> ProgramsList;

            public ShaderPack(string fs)
            {
                ProgramsList = new List<ShaderProgram>();
                if(Program == null)
                    Program = ShaderProgram.Compile("Generic.vertex.glsl",
                       fs);
                if(TesselatedProgram == null)
                    TesselatedProgram = ShaderProgram.Compile("Generic.vertex.glsl",
                        fs, null, "Generic.tesscontrol.glsl", "Generic.tesseval.glsl");
                /*
                if(Geometry1iLines == null)
                    Geometry1iLines = ShaderProgram.Compile("Generic.vertex.glsl",
                        fs, "Generic.geometry1iLines.glsl");*/
                if(Geometry1iTriangles == null)
                    Geometry1iTriangles = ShaderProgram.Compile("Generic.vertex.glsl",
                        fs, "Generic.geometry1iTriangles.glsl");
                /*if(Geometry1iPoints == null)
                    Geometry1iPoints = ShaderProgram.Compile("Generic.vertex.glsl",
                        fs, "Generic.geometry1iPoints.glsl");
                if(Geometry32iLines == null)
                    Geometry32iLines = ShaderProgram.Compile("Generic.vertex.glsl",
                        fs, "Generic.geometry32iLines.glsl");
                if(Geometry32iTriangles == null)
                    Geometry32iTriangles = ShaderProgram.Compile("Generic.vertex.glsl",
                        fs, "Generic.geometry32iTriangles.glsl");
                if(Geometry32iPoints == null)
                    Geometry32iPoints = ShaderProgram.Compile("Generic.vertex.glsl",
                        fs, "Generic.geometry32iPoints.glsl");
                if(Geometry96iLines == null)
                    Geometry96iLines = ShaderProgram.Compile("Generic.vertex.glsl",
                        fs, "Generic.geometry96iLines.glsl");*/
                if(Geometry96iTriangles == null)
                    Geometry96iTriangles = ShaderProgram.Compile("Generic.vertex.glsl",
                        fs, "Generic.geometry96iTriangles.glsl");
                /*if(Geometry96iPoints == null)
                    Geometry96iPoints = ShaderProgram.Compile("Generic.vertex.glsl",
                        fs, "Generic.geometry96iPoints.glsl");
                
                if(TesselatedGeometry1iLines == null)
                    TesselatedGeometry1iLines = ShaderProgram.Compile("Generic.vertex.glsl",
                        fs, "Generic.geometry1iLines.glsl", "Generic.tesscontrol.glsl", "Generic.tesseval.glsl");
                if(TesselatedGeometry1iTriangles == null)
                    TesselatedGeometry1iTriangles = ShaderProgram.Compile("Generic.vertex.glsl",
                        fs, "Generic.geometry1iTriangles.glsl", "Generic.tesscontrol.glsl", "Generic.tesseval.glsl");
                if(TesselatedGeometry1iPoints == null)
                    TesselatedGeometry1iPoints = ShaderProgram.Compile("Generic.vertex.glsl",
                        fs, "Generic.geometry1iPoints.glsl", "Generic.tesscontrol.glsl", "Generic.tesseval.glsl");
                if(TesselatedGeometry32iLines == null)
                    TesselatedGeometry32iLines = ShaderProgram.Compile("Generic.vertex.glsl",
                        fs, "Generic.geometry32iLines.glsl", "Generic.tesscontrol.glsl", "Generic.tesseval.glsl");
                if(TesselatedGeometry32iTriangles == null)
                    TesselatedGeometry32iTriangles = ShaderProgram.Compile("Generic.vertex.glsl",
                        fs, "Generic.geometry32iTriangles.glsl", "Generic.tesscontrol.glsl", "Generic.tesseval.glsl");
                if(TesselatedGeometry32iPoints == null)
                    TesselatedGeometry32iPoints = ShaderProgram.Compile("Generic.vertex.glsl",
                        fs, "Generic.geometry32iPoints.glsl", "Generic.tesscontrol.glsl", "Generic.tesseval.glsl");
                if(TesselatedGeometry96iLines == null)
                    TesselatedGeometry96iLines = ShaderProgram.Compile("Generic.vertex.glsl",
                        fs, "Generic.geometry96iLines.glsl", "Generic.tesscontrol.glsl", "Generic.tesseval.glsl");
                if(TesselatedGeometry96iTriangles == null)
                    TesselatedGeometry96iTriangles = ShaderProgram.Compile("Generic.vertex.glsl",
                        fs, "Generic.geometry96iTriangles.glsl", "Generic.tesscontrol.glsl", "Generic.tesseval.glsl");
                if(TesselatedGeometry96iPoints == null)
                    TesselatedGeometry96iPoints = ShaderProgram.Compile("Generic.vertex.glsl",
                        fs, "Generic.geometry96iPoints.glsl", "Generic.tesscontrol.glsl", "Generic.tesseval.glsl");*/
                ProgramsList.AddRange(new ShaderProgram[] {
                    Program,
                    TesselatedProgram,
                   // Geometry1iLines,
                    Geometry1iTriangles,
                    //Geometry1iPoints,
                   // Geometry32iLines,
                   // Geometry32iTriangles,
                   // Geometry32iPoints,
                   // Geometry96iLines,
                    Geometry96iTriangles,
                   // Geometry96iPoints,
                   // TesselatedGeometry1iLines,
                   // TesselatedGeometry1iTriangles,
                   // TesselatedGeometry1iPoints,
                  //  TesselatedGeometry32iLines,
                  //  TesselatedGeometry32iTriangles,
                   // TesselatedGeometry32iPoints,
                   // TesselatedGeometry96iLines,
                   // TesselatedGeometry96iTriangles,
                   // TesselatedGeometry96iPoints
                });
            }
        }

        public static ShaderPack MainShaderPack = new ShaderPack("Generic.fragment.glsl");
        public static ShaderPack OverrideShaderPack = null;
        public bool CastShadows = true;
        public Vector4 Color;
        public bool IgnoreLighting = false;
        public float Metalness = 0.5f;
        public DrawMode Mode;
        public string Name;
        public Texture NormalMap, BumpMap, AlphaMask, RoughnessMap, MetalnessMap, SpecularMap;
        public float NormalMapScale = 1.0f;
        public bool ReceiveShadows = true;
        public float ReflectionStrength = 0;
        public float RefractionStrength = 0;
        public float Roughness = 0.5f;
        public float SpecularComponent = 0.0f, DiffuseComponent = 1.0f;
        public float TesselationMultiplier = 1.0f;
        public float ParallaxHeightMultiplier = 1.0f;
        public int ParallaxInstances = 12;

        public float
            AORange = 0.5f,
            AOStrength = 0.5f,
            AOAngleCutoff = 0.0f,
            VDAOMultiplier = 0.5f,
            VDAOSamplingMultiplier = 0.5f,
            VDAORefreactionMultiplier = 0.0f,
            SubsurfaceScatteringMultiplier = 0.0f;

        public Texture Tex;
        public MaterialType Type;

        private ShaderProgram lastUserProgram = null;

        private RainSystem RainsDropSystem = null;

        public ShaderStorageBuffer BallsBuffer;

        public void SetRainSystem(RainSystem rs)
        {
            RainsDropSystem = rs;
        }

        public GenericMaterial(Texture tex, Texture normalMap = null, Texture bumpMap = null)
        {
            Tex = tex;
            BallsBuffer = new ShaderStorageBuffer();
            BallsBuffer.Type = BufferUsageHint.DynamicDraw;
            NormalMap = normalMap;
            BumpMap = bumpMap;
            Color = Vector4.One;
            Mode = GenericMaterial.DrawMode.TextureOnly;
        }

        public GenericMaterial(Vector4 color)
        {
            Color = color;
            BallsBuffer = new ShaderStorageBuffer();
            BallsBuffer.Type = BufferUsageHint.DynamicDraw;
            Mode = GenericMaterial.DrawMode.ColorOnly;
        }

        public GenericMaterial(Color color)
        {
            Color = new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
            BallsBuffer = new ShaderStorageBuffer();
            BallsBuffer.Type = BufferUsageHint.DynamicDraw;
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

        public static GenericMaterial FromMedia(string key)
        {
            return new GenericMaterial(new Texture(Media.Get(key)));
        }
        public static GenericMaterial FromColor(Color color)
        {
            return new GenericMaterial(color);
        }

        public static GenericMaterial FromMedia(string key, string normalmap_key)
        {
            return new GenericMaterial(new Texture(Media.Get(key)), new Texture(Media.Get(normalmap_key)));
        }

        public static GenericMaterial FromMedia(string key, string normalmap_key, string bump_map)
        {
            return new GenericMaterial(new Texture(Media.Get(key)), new Texture(Media.Get(normalmap_key)), new Texture(Media.Get(bump_map)));
        }

        public ShaderProgram GetShaderProgram()
        {
            ShaderPack pack = OverrideShaderPack != null ? OverrideShaderPack : MainShaderPack;
            if(Type == MaterialType.Grass || Type == MaterialType.Flag)
                return pack.Geometry96iTriangles;
            if(Type == MaterialType.TessellatedTerrain)
                return pack.TesselatedProgram;
            if(Type == MaterialType.Parallax)
                return pack.Geometry96iTriangles;
            return Type == MaterialType.Water || Type == MaterialType.PlanetSurface ||
               Type == MaterialType.TessellatedTerrain || Type == MaterialType.Grass ? pack.TesselatedProgram : pack.Program;
        }

        public void SetAlphaMaskFromMedia(string key)
        {
            AlphaMask = new Texture(Media.Get(key));
        }

        public void SetBumpMapFromMedia(string key)
        {
            BumpMap = new Texture(Media.Get(key));
        }

        public void SetMetalnessMapFromMedia(string key)
        {
            MetalnessMap = new Texture(Media.Get(key));
        }

        public void SetNormalMapFromMedia(string key)
        {
            NormalMap = new Texture(Media.Get(key));
        }

        public void SetRoughnessMapFromMedia(string key)
        {
            RoughnessMap = new Texture(Media.Get(key));
        }

        public void SetSpecularMapFromMedia(string key)
        {
            SpecularMap = new Texture(Media.Get(key));
        }

        public void SetTextureFromMedia(string key)
        {
            Tex = new Texture(Media.Get(key));
            Mode = GenericMaterial.DrawMode.TextureOnly;
        }

        public void SetOptimizedBalls(List<Vector4> PositionsAndScales)
        {
            var bytes = new List<byte>();
            foreach(var p in PositionsAndScales)
            {
                bytes.AddRange(BitConverter.GetBytes(p.X));
                bytes.AddRange(BitConverter.GetBytes(p.Y));
                bytes.AddRange(BitConverter.GetBytes(p.Z));
                bytes.AddRange(BitConverter.GetBytes(p.W));
            }
            GLThread.Invoke(() => BallsBuffer.MapData(bytes.ToArray()));
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
            if(res == ShaderProgram.SwitchResult.Locked)
            {
                prg = ShaderProgram.Current;
            }
            if(lastUserProgram == ShaderProgram.Current)
                return true;
            lastUserProgram = ShaderProgram.Current;
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

            if(Type == MaterialType.OptimizedSpheres)
            {
                BallsBuffer.Use(4);
            }

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
                GL.Disable(EnableCap.CullFace);
                // GL.Enable(EnableCap.CullFace);
                //GL.DepthFunc(DepthFunction.Lequal);
            }

            if(Tex != null)
                Tex.Use(TextureUnit.Texture0);

            prg.SetUniform("input_Color", Color);
            prg.SetUniform("DrawMode", (int)Mode);
            prg.SetUniform("MaterialType", (int)Type);

            if(Type == MaterialType.DropsSystem && RainsDropSystem != null)
                RainsDropSystem.MapToCurrentShader();

            prg.SetUniform("SpecularComponent", SpecularComponent);
            prg.SetUniform("DiffuseComponent", DiffuseComponent);
            prg.SetUniform("Roughness", Roughness);
            prg.SetUniform("Metalness", Metalness);
            prg.SetUniform("ReflectionStrength", ReflectionStrength);
            prg.SetUniform("RefractionStrength", RefractionStrength);
            prg.SetUniform("IgnoreLighting", IgnoreLighting);
            prg.SetUniform("ParallaxHeightMultiplier", ParallaxHeightMultiplier);
            prg.SetUniform("ParallaxInstances", ParallaxInstances);

            prg.SetUniform("AORange", AORange);
            prg.SetUniform("AOStrength", AOStrength);
            prg.SetUniform("AOAngleCutoff", AOAngleCutoff);
            prg.SetUniform("VDAOMultiplier", VDAOMultiplier);
            prg.SetUniform("VDAOSamplingMultiplier", VDAOSamplingMultiplier);
            prg.SetUniform("VDAORefreactionMultiplier", VDAORefreactionMultiplier);
            prg.SetUniform("SubsurfaceScatteringMultiplier", SubsurfaceScatteringMultiplier);
            // prg.SetUniform("FrameINT", (int)PostProcessing.RandomIntFrame);

            // GL.BindImageTexture(22u, (uint)PostProcessing.FullScene3DTexture.Handle, 0, false, 0,
            // TextureAccess.ReadWrite, SizedInternalFormat.R32ui);

            return true;
        }
    }
}