using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using VEngine;
using VEngine.Generators;
using OpenTK;

namespace ShadowsTester
{
    public class ComputeBallsScene : Scene
    {
        private ShaderStorageBuffer SBuffer;

        public ComputeBallsScene()
        {
            int instancesAxis = 9;
            int instances = instancesAxis * instancesAxis * instancesAxis;
            var ballInfo = Object3dInfo.LoadFromObjSingle(Media.Get("lightsphere.obj"));
            ballInfo.Normalize();
            var instanced = new InstancedMesh3d(ballInfo, new GenericMaterial(Color.LightCyan));

            instanced.Instances = instances;
            var SBuffer = new ShaderStorageBuffer();
            var VBuffer = new ShaderStorageBuffer();
            GLThread.Invoke(() =>
            {
                var cshader = new ComputeShader("ComputeTest.compute.glsl");
                var bts = new List<Vector3>();
                var bts2 = new List<Vector3>();
                var rand = new Random();
                for(int x = 0; x < instancesAxis; x++)
                {
                    for(int y = 0; y < instancesAxis; y++)
                    {
                        for(int z = 0; z < instancesAxis; z++)
                        {
                            instanced.Transformations.Add(new TransformationManager(new Vector3(x, y, z)*4));
                            bts.Add(new Vector3(x, y, z) * 4);
                            bts2.Add(new Vector3((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble()));
                        }
                    }
                }
                SBuffer.MapData(bts.ToArray());
                VBuffer.MapData(bts2.ToArray());
                instanced.UpdateMatrix();
                GLThread.OnBeforeDraw += (o, e) =>
                {
                    cshader.Use();
                    instanced.ModelMatricesBuffer.Use(0);
                    SBuffer.Use(1);
                    VBuffer.Use(2);
                    cshader.SetUniform("BallsCount", instances);
                    cshader.Dispatch(instancesAxis, instancesAxis, instancesAxis);
                };
            });
            Add(instanced);
        }

    }
}
