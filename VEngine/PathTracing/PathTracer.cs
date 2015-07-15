using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine.PathTracing
{
    public class Vertex
    {
        public Vector3 Position, Normal, Albedo;
        public void Tranform(Matrix4 matrix)
        {
            Position = Vector4.Transform(new Vector4(Position, 1.0f), matrix).Xyz;
            Normal = Vector3.Transform(Normal, matrix.ExtractRotation(true));
        }
    }

    public class Triangle
    {
        public Triangle()
        {
            Vertices = new List<Vertex>();
        }
        public List<Vertex> Vertices;
    }

    public class PathTracer
    {
        ShaderStorageBuffer MeshDataSSBO;
        int TriangleCount;
        ComputeShader TracerShader;
        public PathTracer()
        {
            TracerShader = new ComputeShader("PathTracer.compute.glsl");
        }
        
        private Random Rand = new Random();
        public void PathTraceToImage(int imageHandle, int lastBuffer, int Width, int Height)
        {
            TracerShader.Use();
            MeshDataSSBO.Use(0);
            GL.BindImageTexture(0, imageHandle, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba16f);
            GL.BindImageTexture(1, lastBuffer, 0, false, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba16f);
            TracerShader.SetUniform("CameraPosition", Camera.Current.Transformation.GetPosition());
            TracerShader.SetUniform("ViewMatrix", Camera.MainDisplayCamera.ViewMatrix);
            TracerShader.SetUniform("ProjectionMatrix", Camera.MainDisplayCamera.ProjectionMatrix);
            TracerShader.SetUniform("Rand", (float)Rand.NextDouble());
            TracerShader.SetUniform("TrianglesCount", TriangleCount);
            TracerShader.Dispatch(Width / 32 + 1, Height / 32 + 1);
        }

        public void PrepareTrianglesData(List<Mesh3d> meshes)
        {
            List<Triangle> triangles = new List<Triangle>();
            foreach(var mesh in meshes)
            {
                var Triangle = new Triangle();
                var vertices = mesh.MainObjectInfo.GetOrderedVertices();
                var normals = mesh.MainObjectInfo.GetOrderedNormals();
                for(int i = 0; i < vertices.Count; i++)
                {
                    var vertex = new Vertex()
                    {
                        Position = vertices[i],
                        Normal = normals[i],
                        Albedo = mesh.MainMaterial.Color.Xyz
                    };
                    vertex.Tranform(mesh.Matrix);
                    Triangle.Vertices.Add(vertex);
                    if(Triangle.Vertices.Count == 3)
                    {
                        triangles.Add(Triangle);
                        Triangle = new Triangle();
                    }
                }
            }
            SceneOctalTree tree = new SceneOctalTree();
            tree.CreateFromTriangleList(triangles);
            TriangleCount = triangles.Count;
            // lets prepare byte array
            // layout
            // posx, posy, poz, norx, nory, norz, albr, albg, albz
            List<byte> bytes = new List<byte>();
            foreach(var triangle in triangles)
            {
                foreach(var vertex in triangle.Vertices)
                {
                    bytes.AddRange(BitConverter.GetBytes(vertex.Position.X));
                    bytes.AddRange(BitConverter.GetBytes(vertex.Position.Y));
                    bytes.AddRange(BitConverter.GetBytes(vertex.Position.Z));
                    bytes.AddRange(BitConverter.GetBytes(0.0f));

                    bytes.AddRange(BitConverter.GetBytes(vertex.Normal.X));
                    bytes.AddRange(BitConverter.GetBytes(vertex.Normal.Y));
                    bytes.AddRange(BitConverter.GetBytes(vertex.Normal.Z));
                    bytes.AddRange(BitConverter.GetBytes(0.0f));

                    bytes.AddRange(BitConverter.GetBytes(vertex.Albedo.X));
                    bytes.AddRange(BitConverter.GetBytes(vertex.Albedo.Y));
                    bytes.AddRange(BitConverter.GetBytes(vertex.Albedo.Z));
                    bytes.AddRange(BitConverter.GetBytes(0.0f));
                }
            }
            MeshDataSSBO = new ShaderStorageBuffer();
            GLThread.Invoke(() => MeshDataSSBO.MapData(bytes.ToArray()));
        }
    }
}
