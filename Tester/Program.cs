using OpenTK;
using System;
using System.Linq;
using System.Threading.Tasks;
using VDGTech;
using System.Drawing;

namespace Tester
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            VEngineWindowAdapter window = null;
            var renderThread = Task.Factory.StartNew(() =>
            {
                window = new VEngineWindowAdapter("Test", 1600, 900);
                window.Run(60);
            });

            float[] postProcessingPlaneVertices = {
                -1.0f, -1.0f, 0.9f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, -1.0f, 0.9f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f,
                -1.0f, 1.0f, 0.9f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, 1.0f, 0.9f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f
            };
            uint[] postProcessingPlaneIndices = {
                0, 1, 2, 3, 2, 1
            };

            Mesh3d postPlane = null;
            GLThread.Invoke(() =>
            {
                Camera camera = new Camera(new Vector3(52, 22, 5), new Vector3(0, 2, 0), 1600.0f / 900.0f, 3.14f / 2.0f, 2.0f, 8000.0f);

                Object3dInfo postPlane3dInfo = new Object3dInfo(postProcessingPlaneVertices.ToList(), postProcessingPlaneIndices.ToList());
                postPlane3dInfo.GenerateBuffers();
                postPlane = new Mesh3d(postPlane3dInfo, new PostProcessLoadingMaterial());
                postPlane.UpdateMatrix();

                World.Root.UpdateMatrix();
                World.Root.Add(postPlane);
            });

            Object3dInfo ball3dInfo = Object3dInfo.LoadFromObj(Media.Get("cube_simple.obj"));
            Random rand = new Random();

            GLThread.Invoke(() =>
            {
                ball3dInfo.GenerateBuffers();
                Texture tex = new Texture(Media.Get("uv.png")); 
                for (int i = 0; i < 20; i++)
                {
                    Mesh3d ball = new Mesh3d(ball3dInfo, new SingleTextureMaterial(tex));
                    ball.SetScale(2);
                    ball.SetPosition(new Vector3(rand.Next(-200, 200), rand.Next(-200, 200), rand.Next(-200, 200)));
                    ball.SetCollisionShape(new BulletSharp.SphereShape(1));
                    ball.SetMass(15.5f);
                    ball.UpdateMatrix();
                    World.Root.Add(ball);
                }
                Helicopter copter = new Helicopter();
                World.Root.Add(copter);

                InfiniteGround ground = new InfiniteGround();
                World.Root.Add(ground);
                
                World.Root.Remove(postPlane);
            });
            GLThread.Invoke(() =>  window.StartPhysicsThread());
            renderThread.Wait();
        }
    }
}