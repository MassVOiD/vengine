using OpenTK;
using System;
using System.Linq;
using System.Threading.Tasks;
using VDGTech;

namespace Tester
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            VEngineWindowAdapter window;
            Task.Factory.StartNew(() =>
            {
                window = new VEngineWindowAdapter("Test", 1200, 600);
                window.Run(60);
            });

            float[] postProcessingPlaneVertices = {
                -1.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, -1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f,
                -1.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, 1.0f, 0.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f
            };
            uint[] postProcessingPlaneIndices = {
                0, 1, 2, 1, 2, 3
            };

            GLThread.Invoke(() =>
            {
                Camera camera = new Camera(new Vector3(3.1f, 0.5f, 3.0f), new Vector3(0, 0.1f, 0), 3.14f / 2.0f);
                /*
                Object3dInfo teapot3dInfo = Object3dInfo.LoadFromObj(Media.Get("teapot.obj"));
                Mesh3d teapot = new Mesh3d(teapot3dInfo, new SolidColorMaterial(Color.Beige));
                teapot.UpdateMatrix();
                Object3dInfo sphere3dInfo = Object3dInfo.LoadFromObj(Media.Get("lightsphere.obj"));
                Mesh3d sphere = new Mesh3d(sphere3dInfo, new SolidColorMaterial(Color.Beige));
                sphere.UpdateMatrix();*/

                Object3dInfo postPlane3dInfo = new Object3dInfo(postProcessingPlaneVertices.ToList(), postProcessingPlaneIndices.ToList());
                Mesh3d postPlane = new Mesh3d(postPlane3dInfo, new PostProcessLoadingMaterial());
                postPlane.UpdateMatrix();

                SceneNode.Root.UpdateMatrix();
                SceneNode.Root.Add(postPlane);
                //SceneNode.Root.Add(teapot);
                //SceneNode.Root.Add(sphere);
            });
            Console.Read();
        }
    }
}