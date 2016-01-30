using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEngine
{
    public class ShaderPool
    {
        public class ShaderPack
        {
            public ShaderProgram
                Program,
                TesselatedProgram,
                Geometry1iTriangles,
                Geometry96iTriangles;

            public List<ShaderProgram> ProgramsList;

            public ShaderPack(string fs = null)
            {
                ProgramsList = new List<ShaderProgram>();
                if(Program == null)
                    Program = ShaderProgram.Compile("Generic.vertex.glsl",
                       fs);
                if(TesselatedProgram == null)
                    TesselatedProgram = ShaderProgram.Compile("Generic.vertex.glsl",
                        fs, null, "Generic.tesscontrol.glsl", "Generic.tesseval.glsl");
                if(Geometry1iTriangles == null)
                    Geometry1iTriangles = ShaderProgram.Compile("Generic.vertex.glsl",
                        fs, "Generic.geometry1iTriangles.geometry.glsl");
                if(Geometry96iTriangles == null)
                    Geometry96iTriangles = ShaderProgram.Compile("Generic.vertex.glsl",
                        fs, "Generic.geometry96iTriangles.geometry.glsl");
                ProgramsList.AddRange(new ShaderProgram[] {
                    Program,
                    TesselatedProgram,
                    Geometry1iTriangles,
                    Geometry96iTriangles,
                });
            }
            public void SetGlobal(string name, string value)
            {
                ProgramsList.ForEach((a) => a.SetGlobal(name, value));
            }
        }
        private ShaderPack DepthOnly, DepthOnlyMSAA, GenericMaterial, GenericMaterialMSAA, DistanceOnly, DistanceOnlyMSAA;

        public bool ForceSingleSample = false;

        public ShaderPack ChooseShaderGenericMaterial(bool forceSingleSample = false)
        {
            if(Game.MSAASamples > 1 && !forceSingleSample && !ForceSingleSample)
                return GenericMaterialMSAA;
            else
                return GenericMaterial;
        }
        public ShaderPack ChooseShaderDepth(bool forceSingleSample = false)
        {
            if(Game.MSAASamples > 1 && !forceSingleSample && !ForceSingleSample)
                return DepthOnlyMSAA;
            else
                return DepthOnly;
        }
        public ShaderPack ChooseShaderDistance(bool forceSingleSample = false)
        {
            if(Game.MSAASamples > 1 && !forceSingleSample && !ForceSingleSample)
                return DistanceOnlyMSAA;
            else
                return DistanceOnly;
        }

        public ShaderPool()
        {
            DepthOnly = new ShaderPack();

            DepthOnlyMSAA = new ShaderPack();
            DepthOnlyMSAA.SetGlobal("MSAA_SAMPLES", Game.MSAASamples.ToString());
            if(Game.MSAASamples > 1)
                DepthOnlyMSAA.SetGlobal("USE_MSAA", "");

            GenericMaterial = new ShaderPack("Generic.fragment.glsl");

            GenericMaterialMSAA = new ShaderPack("Generic.fragment.glsl");
            GenericMaterialMSAA.SetGlobal("MSAA_SAMPLES", Game.MSAASamples.ToString());
            if(Game.MSAASamples > 1)
                GenericMaterialMSAA.SetGlobal("USE_MSAA", "");

            DistanceOnly = new ShaderPack("DistanceOnly.fragment.glsl");

            DistanceOnlyMSAA = new ShaderPack("DistanceOnly.fragment.glsl");
            DistanceOnlyMSAA.SetGlobal("MSAA_SAMPLES", Game.MSAASamples.ToString());
            if(Game.MSAASamples > 1)
                DistanceOnlyMSAA.SetGlobal("USE_MSAA", "");
        }
    }
}
