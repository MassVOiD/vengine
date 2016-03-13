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

            public ShaderProgram[] ProgramsList;

            public ShaderPack(string fs = null)
            {
                ProgramsList = new ShaderProgram[4];
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

                ProgramsList[0] = Program;
                ProgramsList[1] = TesselatedProgram;
                ProgramsList[2] = Geometry1iTriangles;
                ProgramsList[3] = Geometry96iTriangles;

            }
        }
        private ShaderPack DepthOnly, GenericMaterial, ForwardMaterial, DistanceOnly;

        private ShaderPack[] Packs;

        public ShaderPack[] GetPacks()
        {
            return Packs;
        }

        public ShaderPack ChooseShaderGenericMaterial(bool isForward)
        {
            return isForward ? ForwardMaterial : GenericMaterial;
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
            DepthOnly = new ShaderPack("ConeLight.fragment.glsl");

            GenericMaterial = new ShaderPack("Generic.fragment.glsl");

            ForwardMaterial = new ShaderPack("Forward.fragment.glsl");

            DistanceOnly = new ShaderPack("DistanceOnly.fragment.glsl");

            Packs = new ShaderPack[] { DepthOnly, GenericMaterial, ForwardMaterial, DistanceOnly };
        }
    }
}
