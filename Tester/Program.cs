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

            World.Root = new World();
            Mesh3d postPlane = null;

            Camera camera = new Camera(new Vector3(52, 22, 5), new Vector3(0, 2, 0), 1600.0f / 900.0f, 3.14f / 2.0f, 20.0f, 10000.0f);

            Object3dInfo postPlane3dInfo = new Object3dInfo(postProcessingPlaneVertices.ToList(), postProcessingPlaneIndices.ToList());
            postPlane = new Mesh3d(postPlane3dInfo, new PostProcessLoadingMaterial());

            World.Root.UpdateMatrix();
            World.Root.Add(postPlane);

            Object3dInfo ball3dInfo = Object3dInfo.LoadFromObj(Media.Get("earth.obj"));
            Object3dInfo terrain3dInfo = Object3dInfo.LoadFromCompressed(Media.Get("terrain.rend"));
            Random rand = new Random();

            bool Shooting = false;
            GLThread.OnKeyDown += (o, e) =>
            {
                if (e.Key == OpenTK.Input.Key.Space)
                {
                    if (Shooting) return;
                    Shooting = true;
                    Mesh3d ball = new Mesh3d(ball3dInfo, new SolidColorMaterial(Color.Yellow));
                    ball.SetScale(1);
                    ball.SetPosition(Camera.Current.Position);
                    ball.SetCollisionShape(new BulletSharp.SphereShape(1));
                    ball.SetMass(4.1f);
                    World.Root.Add(ball);
                    Vector3 direction = Camera.Current.GetDirection() * 140.0f;
                    ball.PhysicalBody.ApplyCentralImpulse(direction);
                }
            };
            GLThread.OnKeyUp += (o, e) =>
            {
                if (e.Key == OpenTK.Input.Key.Space)
                {
                    Shooting = false;
                }
            };

            Texture tex = new Texture(Media.Get("earthsmall.png")); 
            for (int i = 0; i < 5; i++)
            {
                Mesh3d ball = new Mesh3d(ball3dInfo, new SingleTextureMaterial(tex));
                ball.SetScale(1);
                ball.SetPosition(new Vector3(rand.Next(-200, 200), 850, rand.Next(-200, 200)));
                ball.SetCollisionShape(new BulletSharp.SphereShape(4));
                ball.SetMass(15.5f);
                World.Root.Add(ball);
            }


            Mesh3d terrain = new Mesh3d(terrain3dInfo,
                    new ManualShaderMaterial(
                        Media.ReadAllText("Mountains.vertex.glsl"),
                        Media.ReadAllText("Mountains.fragment.glsl")
                ));
            terrain.SetScale(30);
            terrain.SetPosition(Vector3.Zero);
            terrain.SetMass(0);
            var terrainShape = terrain3dInfo.GetAccurateCollisionShape(30.0f);
            terrain.SetCollisionShape(terrainShape);
            World.Root.Add(terrain);

            Airplane copter = new Airplane();
            World.Root.Add(copter);


            World.Root.Remove(postPlane);

            GLThread.Invoke(() =>  window.StartPhysicsThread());
            renderThread.Wait();
        }
    }
}