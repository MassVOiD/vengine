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
        private ShaderPack DepthOnly, GenericMaterial, DistanceOnly;

        public ShaderPack[] GetPacks()
        {
            return new ShaderPack[] { DepthOnly, GenericMaterial, DistanceOnly };
        }
        
        public ShaderPack ChooseShaderGenericMaterial()
        {
        return GenericMaterial;
        }
        public ShaderPack ChooseShaderDepth()
        {
            return DepthOnly;
        }
        public ShaderPack ChooseShaderDistance()
        {
            return DistanceOnly;
        }

        public ShaderPool()
        {
            DepthOnly = new ShaderPack();

            GenericMaterial = new ShaderPack("Generic.fragment.glsl");

            DistanceOnly = new ShaderPack("DistanceOnly.fragment.glsl");
        }
    }
}
