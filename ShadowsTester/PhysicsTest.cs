using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using VEngine;
using VEngine.FileFormats;
using VEngine.Generators;

namespace ShadowsTester
{
    public class PhysicsTest
    {

        Mesh3d CreateWall(Vector2 start, Vector2 end, Quaternion rotation, Vector3 position, Vector3 color)
        {
            var terrain3dManager = Object3dGenerator.CreateGround(start, end, new Vector2(1), Vector3.UnitY);
            var terrain3dInfo = new Object3dInfo(terrain3dManager.Vertices);
            var terrainMaterial = new GenericMaterial();
            terrainMaterial.DiffuseColor = color;
            terrainMaterial.SpecularColor = Vector3.Zero;
            terrainMaterial.Roughness = 1.0f;
            var terrainMesh = Mesh3d.Create(terrain3dInfo, terrainMaterial);
            terrainMesh.GetInstance(0).Rotate(rotation);
            return terrainMesh;
        }

        Mesh3d CreateDiffuseModelFromRaw(string obj, Vector3 color)
        {
            var terrain3dManager = Object3dManager.LoadFromRaw(Media.Get(obj));
            var terrain3dInfo = new Object3dInfo(terrain3dManager.Vertices);
            var terrainMaterial = new GenericMaterial();
            terrainMaterial.DiffuseColor = color;
            terrainMaterial.SpecularColor = color;
            terrainMaterial.Roughness = 0.1f;
            var terrainMesh = Mesh3d.Create(terrain3dInfo, terrainMaterial);
            return terrainMesh;
        }
        static Random rdz = new Random();
        static float rand(float min, float max)
        {
            return ((float)rdz.NextDouble()) * (max - min) + min;
        }

        struct Particle
        {
            public Vector3 Position;
            public Vector3 Velocity;
        }

        public PhysicsTest()
        {
            var scene = Game.World.Scene;

            Game.Invoke(() =>
            {
                Object3dInfo point3dinfo = new Object3dInfo(new VertexInfo[] { new VertexInfo() {
                    Position = Vector3.One,
                    Normal = Vector3.UnitZ,
                    UV = Vector2.Zero
                } });
                Object3dInfo point3dinfo2 = Object3dGenerator.CreateCube(Vector3.One * 0.1f, Vector2.One).AsObject3dInfo();
                OpenTK.Graphics.OpenGL4.GL.PointSize(111);
                GenericMaterial whitemat = new GenericMaterial()
                {
                    DiffuseColor = Vector3.One
                };
                Mesh3d points = Mesh3d.Create(point3dinfo2, whitemat);
                //point3dinfo.DrawMode = OpenTK.Graphics.OpenGL4.PrimitiveType.Points;
                //whitemat.CustomShaderProgram = ShaderProgram.Compile("Points.vertex.glsl", "Points.fragment.glsl");
                points.ClearInstances();

                ComputeShader updateShader = new ComputeShader("PhysicsUpdater.compute.glsl");
                ShaderStorageBuffer pointsBuffer = new ShaderStorageBuffer();
                var parts = new List<Particle>();
                List<byte> bytes = new List<byte>();
                for(int i = 0; i < 1024 * 2; i++)
                {
                    var part = new Particle()
                    {
                        Position = new Vector3(rand(-20, 20), rand(0, 20), rand(-10, 10)),
                        Velocity = Vector3.Zero
                    };
                    points.AddInstance(new TransformationManager(part.Position));

                    bytes.AddRange(BitConverter.GetBytes(part.Position.X));
                    bytes.AddRange(BitConverter.GetBytes(part.Position.Y));
                    bytes.AddRange(BitConverter.GetBytes(part.Position.Z));
                    bytes.AddRange(BitConverter.GetBytes(part.Position.X));

                    bytes.AddRange(BitConverter.GetBytes(part.Velocity.X));
                    bytes.AddRange(BitConverter.GetBytes(part.Velocity.Y));
                    bytes.AddRange(BitConverter.GetBytes(part.Velocity.Z));
                    bytes.AddRange(BitConverter.GetBytes(part.Velocity.X));
                }
                pointsBuffer.MapData(bytes.ToArray());

                pointsBuffer.Use(9);
                Game.OnBeforeDraw += (z, x) =>
                {
                    updateShader.Use();
                    pointsBuffer.Use(9);
                    updateShader.SetUniform("ParticlesCount", 1024 * 2);
                    updateShader.Dispatch(2, 1, 1);
                    
                    pointsBuffer.Use(9);
                };

                points.UpdateMatrix();
                scene.Add(points);
                
            });
        }
    }
}