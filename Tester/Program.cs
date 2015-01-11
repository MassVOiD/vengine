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
            VEngineWindowAdapter window;
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
                0, 1, 2, 1, 2, 3
            };

            GLThread.Invoke(() =>
            {
                Camera camera = new Camera(new Vector3(52, 22, 5), new Vector3(0, 2, 0), 1600.0f/900.0f, 3.14f / 2.0f, 2.0f, 8000.0f);

                Object3dInfo teapot3dInfo = Object3dInfo.LoadFromObj(Media.Get("teapot.obj"));
                Mesh3d teapot = new Mesh3d(teapot3dInfo, new SolidColorMaterial(Color.Blue));
                teapot.Scale = 2.7f;
                teapot.UpdateMatrix();

                Object3dInfo city3dInfo = Object3dInfo.LoadFromObj(Media.Get("city2_superbig.obj"));
                Mesh3d city = new Mesh3d(city3dInfo, new SolidColorMaterial(Color.Green));
                city.Scale = 0.3f;
                city.Position = new Vector3(0, -20.0f, 0);
                city.UpdateMatrix();

                Object3dInfo postPlane3dInfo = new Object3dInfo(postProcessingPlaneVertices.ToList(), postProcessingPlaneIndices.ToList());
                Mesh3d postPlane = new Mesh3d(postPlane3dInfo, new PostProcessLoadingMaterial());
                postPlane.UpdateMatrix();

                SceneNode.Root.UpdateMatrix();
                //SceneNode.Root.Add(postPlane);
                SceneNode.Root.Add(teapot);
                SceneNode.Root.Add(city);
                GLThread.OnUpdate += (o,p) => {
                    var rotation = Quaternion.FromAxisAngle(Vector3.UnitY, 0.001f);
                    teapot.Orientation = Quaternion.Multiply(teapot.Orientation, rotation);
                    teapot.UpdateMatrix();
                };
            });
            renderThread.Wait();
        }
    }
}