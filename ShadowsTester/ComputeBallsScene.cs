﻿using System;
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
            var ballInfo = Object3dInfo.LoadFromObjSingle(Media.Get("lightsphere.obj"));
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
            for(float x = 0; x < MathHelper.TwoPi; x++)
                for(float y = 0; y < MathHelper.TwoPi; y++)
                {
                bts3.Add(new Vector4((float)Math.Sin(x), (float)Math.Cos(y), (float)Math.Cos(y), 1)*12);
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
                for(int x = 0; x < 2; x++)
                {
                    for(int y = 0; y < 50; y++)
                    {
                        for(int z = 0; z < 2; z++)
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
                    cshader.Dispatch(2, 2, 50/50);
                };
            });
            Add(instanced);

            var whiteboxInfo = Object3dInfo.LoadFromObjSingle(Media.Get("whiteroom.obj"));
            var whitebox = new Mesh3d(whiteboxInfo, new GenericMaterial(new Vector4(1000, 1000, 1000, 1000)));
            whitebox.Scale(300);
            whitebox.Translate(0, -2, 0);
            Add(whitebox);
            var whiteboxInfo2 = Object3dInfo.LoadFromObjSingle(Media.Get("reflector.obj"));
            //whiteboxInfo2.ScaleUV(33.0f);
            var whitebox2 = new Mesh3d(whiteboxInfo2, new GenericMaterial(Color.White));
            //whitebox2.MainMaterial.SetBumpMapFromMedia("cobblestone.jpg");
            whitebox2.Scale(2);
            whitebox2.Rotate(Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(90)));
            Add(whitebox2);
            var lod1 = Object3dInfo.LoadFromRaw(Media.Get("lucy.vbo.raw"), Media.Get("lucy.indices.raw"));
            lod1.ScaleUV(8.0f);
            var chairInfo = Object3dInfo.LoadFromObjSingle(Media.Get("nicechair.obj"));
            var chair = new Mesh3d(lod1, GenericMaterial.FromMedia("168.JPG", "168_norm.JPG", "168_norm.JPG"));
            chair.MainMaterial.Roughness = 0.9f;
            //chair.MainMaterial.SetNormalMapFromMedia("clothnorm.png");
            chair.MainMaterial.ReflectionStrength = 1.0f;
            Add(chair);
        }

    }
}
