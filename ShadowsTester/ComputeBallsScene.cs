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
            int instancesAxis = 10;
            int instances = instancesAxis * instancesAxis * instancesAxis;
            var ballInfo = Object3dInfo.LoadFromObjSingle(Media.Get("star3d.obj"));
           // var cb = Object3dGenerator.CreateCube(new Vector3(1000, 1000, 1000), new Vector2(1, 1));
           // cb.FlipFaces();
           // var sky = new Mesh3d(cb, new GenericMaterial(Color.Black));
           // Add(sky);
            ballInfo.Normalize();
            var instanced = new InstancedMesh3d(ballInfo, new GenericMaterial(new Vector4(0, 1, 0, 1)));
            var instanced2 = new InstancedMesh3d(ballInfo, new GenericMaterial(Color.Green));
            //var model = Object3dInfo.LoadFromObjSingle(Media.Get("monkey.obj"));
            //var vrts = model.GetOrderedVertices();
            //vrts = vrts.Distinct().ToList();
            var bts3 = new List<Vector4>();
            for(int x = 0; x < 20; x++)
                for(int y = 60; y > 0; y--)
            {
                bts3.Add(new Vector4(x, y, 0, 1));
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
                for(int x = 0; x < 420; x++)
                {
                    for(int y = 0; y < 20; y++)
                    {
                        instanced.Transformations.Add(new TransformationManager(new Vector3(x, y + 300, 0), Quaternion.Identity,1.5f));
                        bts.Add(new Vector4(x, y + 300, 0, 1));
                        bts2.Add(new Vector4(0, 0, (new Vector2(100, 100) - new Vector2(x, y)).Length *0.1f, 1));
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
                    cshader.SetUniform("Time", (float)(DateTime.Now - GLThread.StartTime).TotalMilliseconds / 1000);
                    cshader.Dispatch(420, 60, 1);
                };
            });
            Add(instanced);
        }

    }
}
