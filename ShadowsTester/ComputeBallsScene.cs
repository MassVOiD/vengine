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
            int instancesAxis = 4;
            int instances = instancesAxis * instancesAxis * instancesAxis;
            var ballInfo = Object3dInfo.LoadFromObjSingle(Media.Get("star3d.obj"));
           // var cb = Object3dGenerator.CreateCube(new Vector3(1000, 1000, 1000), new Vector2(1, 1));
           // cb.FlipFaces();
           // var sky = new Mesh3d(cb, new GenericMaterial(Color.Black));
           // Add(sky);
            ballInfo.Normalize();
            var instanced = new InstancedMesh3d(ballInfo, new GenericMaterial(Color.White));
            var instanced2 = new InstancedMesh3d(ballInfo, new GenericMaterial(Color.Green));
            var model = Object3dInfo.LoadFromObjSingle(Media.Get("monkey.obj"));
            var vrts = model.GetOrderedVertices();
            vrts = vrts.Distinct().ToList();
            var bts3 = new List<Vector4>();
            for(int i = 0; i < vrts.Count; i++)
            {
                bts3.Add(new Vector4(vrts[i] * 40, 1));
            }
            instanced2.UpdateMatrix();
            Add(instanced2);

            instanced.Instances = instances;
            var SBuffer = new ShaderStorageBuffer();
            var VBuffer = new ShaderStorageBuffer();
            var PBuffer = new ShaderStorageBuffer();
            GLThread.Invoke(() =>
            {
                var cshader = new ComputeShader("AIPathFollower.compute.glsl");
                var bts = new List<Vector4>();
                var bts2 = new List<Vector4>();
                var rand = new Random();
                PBuffer.MapData(bts3.ToArray());
                for(int x = 0; x < instancesAxis; x++)
                {
                    for(int y = 0; y < instancesAxis; y++)
                    {
                        for(int z = 0; z < instancesAxis; z++)
                        {
                            instanced.Transformations.Add(new TransformationManager(new Vector3(x - instancesAxis / 2, y + 1 - instancesAxis / 2, z - instancesAxis / 2) * 4, Quaternion.Identity, 5));
                            bts.Add(new Vector4(x - instancesAxis / 2, y + 1 - instancesAxis / 2, z - instancesAxis / 2, 1) * 4);
                            bts2.Add((new Vector4((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1) - new Vector4(0.5f)) * 60);
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
                    PBuffer.Use(3);
                    cshader.SetUniform("BallsCount", instances);
                    cshader.SetUniform("PathPointsCount", bts3.Count);
                    cshader.Dispatch(instancesAxis, instancesAxis, instancesAxis);
                };
            });
            Add(instanced);
        }

    }
}
