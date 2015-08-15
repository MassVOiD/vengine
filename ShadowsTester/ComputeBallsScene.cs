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
            Object3dInfo skydomeInfo = Object3dInfo.LoadFromObjSingle(Media.Get("usky.obj"));
            var skydomeMaterial = GenericMaterial.FromMedia("skyreal.png");
            var skydome = new Mesh3d(skydomeInfo, skydomeMaterial);
            skydome.Scale(55000);
            skydome.Translate(0, -100, 0);
            //skydome.IgnoreLighting = true;
            //skydome.DiffuseComponent = 0.2f;
            Add(skydome);
            int instancesAxis = 10;
            int instances = instancesAxis * instancesAxis * instancesAxis;
            var ballInfo = Object3dInfo.LoadFromObjSingle(Media.Get("star3d.obj"));
            ballInfo.Normalize();
           // var cb = Object3dGenerator.CreateCube(new Vector3(1000, 1000, 1000), new Vector2(1, 1));
           // cb.FlipFaces();
           // var sky = new Mesh3d(cb, new GenericMaterial(Color.Black));
           // Add(sky);
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
                for(int x = 0; x < 30; x++)
                {
                    for(int y = 0; y < 200; y++)
                    {
                        for(int z = 0; z < 30; z++)
                        {
                            var vec = new Vector3(x * 4 + (float)rand.NextDouble()*3, y*3 + 20 + (float)rand.NextDouble(), z * 4 + (float)rand.NextDouble() * 3);
                            instanced.Transformations.Add(new TransformationManager(vec, Quaternion.Identity, 1f));
                            bts.Add(new Vector4(vec.X, vec.Y, vec.Z, 1));
                            bts2.Add(new Vector4((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1));
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
                    cshader.SetUniform("Time", (float)(DateTime.Now - GLThread.StartTime).TotalMilliseconds / 1000);
                    cshader.Dispatch(30, 30, 200/50);
                };
            });
            Add(instanced);
            Object3dInfo waterInfo = Object3dGenerator.CreateTerrain(new Vector2(-200, -200), new Vector2(200, 200), new Vector2(300, 300), Vector3.UnitY, 333, (x, y) => 0);

            var color = new GenericMaterial(Color.White);
           // color.SetNormalMapFromMedia("watermap.png");
            //color.Type = GenericMaterial.MaterialType.Water;
            //color.SetBumpMapFromMedia("lightref.png");
            Mesh3d water2 = new Mesh3d(waterInfo, color);
            water2.SetMass(0);
            color.Roughness = 0.1f;
            water2.Translate(0, -20.0f, 0);
           // water2.MainMaterial.ReflectionStrength = 1;
            //water.SetCollisionShape(new BulletSharp.StaticPlaneShape(Vector3.UnitY, 0));
            Add(water2);
        }

    }
}
