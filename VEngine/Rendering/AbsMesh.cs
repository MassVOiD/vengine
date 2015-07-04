using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace VEngine
{
    public abstract class AbsMesh
    {
        class LodLevelData
        {
            public Object3dInfo Info3d;
            public GenericMaterial Material;
            public float Distance;
        }

        private List<LodLevelData> LodLevels;

        public int Instances;
        public GenericMaterial Material;
        public Object3dInfo ObjectInfo;
        public Random Randomizer;
        public float SpecularSize = 1.0f, SpecularComponent = 1.0f, DiffuseComponent = 1.0f;
        public bool IgnoreLighting = false;
        public bool DisableDepthWrite = false;


        public void AddLodLevel(float distance, Object3dInfo info, GenericMaterial material)
        {
            if(LodLevels == null)
                LodLevels = new List<LodLevelData>();
            LodLevels.Add(new LodLevelData()
            {
                Info3d = info,
                Material = material,
                Distance = distance
            });
            LodLevels.Sort((a, b) => (int)((a.Distance - b.Distance) * 100.0)); // *100 to preserve precision
        }

        public void SetUniforms(GenericMaterial material)
        {
            ShaderProgram shader = material.GetShaderProgram();
            bool shaderSwitchResult = Material.Use();

            // if(Sun.Current != null) Sun.Current.BindToShader(shader); per mesh

            shader.SetUniform("SpecularComponent", SpecularComponent);
            shader.SetUniform("DiffuseComponent", DiffuseComponent);
            shader.SetUniform("SpecularSize", SpecularSize);
            shader.SetUniform("IgnoreLighting", false);

            shader.SetUniform("RandomSeed1", (float)Randomizer.NextDouble());
            shader.SetUniform("RandomSeed2", (float)Randomizer.NextDouble());
            shader.SetUniform("RandomSeed3", (float)Randomizer.NextDouble());
            shader.SetUniform("RandomSeed4", (float)Randomizer.NextDouble());
            shader.SetUniform("RandomSeed5", (float)Randomizer.NextDouble());
            shader.SetUniform("RandomSeed6", (float)Randomizer.NextDouble());
            shader.SetUniform("RandomSeed7", (float)Randomizer.NextDouble());
            shader.SetUniform("RandomSeed8", (float)Randomizer.NextDouble());
            shader.SetUniform("RandomSeed9", (float)Randomizer.NextDouble());
            shader.SetUniform("RandomSeed10", (float)Randomizer.NextDouble());

            shader.SetUniform("Time", (float)(DateTime.Now - GLThread.StartTime).TotalMilliseconds / 1000);

            shader.SetUniform("UseAlphaMask", 0);

            //LastMaterialHash = Material.GetShaderProgram().GetHashCode();
            // per world
            shader.SetUniform("Instances", Instances);
            shader.SetUniform("ViewMatrix", Camera.Current.ViewMatrix);
            shader.SetUniform("ProjectionMatrix", Camera.Current.ProjectionMatrix);
            shader.SetUniform("LogEnchacer", 0.01f);

            shader.SetUniform("CameraPosition", Camera.Current.Transformation.GetPosition());
            shader.SetUniform("FarPlane", Camera.Current.Far);
            shader.SetUniform("resolution", new Vector2(GLThread.Resolution.Width, GLThread.Resolution.Height));

        }
    }
}
