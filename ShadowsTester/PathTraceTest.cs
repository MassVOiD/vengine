using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using VEngine;
using VEngine.Generators;
using OpenTK;
using VEngine.Rendering;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;
using VEngine.PathTracing;

namespace ShadowsTester
{
    public class PathTraceTest : Scene
    {
        public static PathTracer Tracer;

        public PathTraceTest()
        {
            Object3dInfo[] skydomeInfo = Object3dInfo.LoadFromObj(Media.Get("ptracetest.obj"));
            //var sponza = Object3dInfo.LoadSceneFromObj(Media.Get("cryteksponza.obj"), Media.Get("cryteksponza.mtl"), 0.03f);
            List<Mesh3d> meshes = new List<Mesh3d>();
            List<GenericMaterial> mats = new List<GenericMaterial>
            {
                new GenericMaterial(new Vector4(1.0f, 1.0f, 1.0f, 1.0f)),
                new GenericMaterial(new Vector4(0.1f, 0, 0, 1.0f)),
                new GenericMaterial(new Vector4(0, 0.1f, 0, 1.0f)),
                new GenericMaterial(new Vector4(0, 0, 0.1f, 1.0f)),
                new GenericMaterial(new Vector4(1.0f, 1.0f, 1.0f, 1.0f)),
                new GenericMaterial(new Vector4(1.0f, 1.0f, 1.0f, 1.0f))
            };
            int ix = 0;
            foreach(var sd in skydomeInfo)
            {
                var skydomeMaterial = mats[ix++];
                var skydome = new Mesh3d(sd, skydomeMaterial);
                meshes.Add(skydome);
            }
            Tracer = new PathTracer();
            Tracer.PrepareTrianglesData(meshes);
            PostProcessing.Tracer = Tracer;
        }
    }
}
