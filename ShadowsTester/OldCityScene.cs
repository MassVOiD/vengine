using OpenTK;
using VEngine;

namespace ShadowsTester
{
    public class OldCityScene
    {
        public OldCityScene()
        {
            /*  var scene = Object3dInfo.LoadSceneFromObj(Media.Get("desertcity.obj"), Media.Get("desertcity.mtl"), 1.0f);
              foreach(var ob in scene)
              {
                  ob.SetMass(0);
                  this.Add(ob);
              }*/
            // var scene = Object3dInfo.LoadSceneFromObj(Media.Get("cryteksponza.obj"),
            // Media.Get("cryteksponza.mtl"), 0.03f);

            var script = System.IO.File.ReadAllText(Media.Get("init.js"));
            var js = new Script();
            js.SetValue("Scene", World.Root.RootScene);
            js.Execute("var VEngine = importNamespace('VEngine');");
            js.Execute(script);
            GLThread.Invoke(() =>
            {
                GLThread.DisplayAdapter.Pipeline.PostProcessor.AABoxesBuffer.MapData(new Vector4[4 * 3]{
                    new Vector4(-5.826700f, 0.183174f, -4.040090f, 0), new Vector4(-5.277061f, 2.116685f, -3.999181f, 0), new Vector4(1, 1, 1.1f, 55),
                    new Vector4(-5.218601f, 0.183174f, -4.040090f, 0), new Vector4(-4.668962f, 2.116685f, -3.999181f, 0), new Vector4(1, 1, 1.1f, 55),
                    new Vector4(-7.154271f, 0.183174f, -4.040090f, 0), new Vector4(-6.604632f, 2.116685f, -3.999181f, 0), new Vector4(1, 1, 1.1f, 55),
                    new Vector4(-6.554271f, 0.183174f, -4.040090f, 0), new Vector4(-6.004632f, 2.116685f, -3.999181f, 0), new Vector4(1, 1, 1.1f, 55),
                });
                GLThread.DisplayAdapter.Pipeline.PostProcessor.AABoxesCount = 4;
            });

            /*
            List<Mesh3d> cubes = new List<Mesh3d>();
            for(int i = 0; i < 80; i++)
                cubes.Add(new Mesh3d(Object3dInfo.LoadFromObjSingle(Media.Get("normalizedcube.obj")), new GenericMaterial(Color.White)));
            cubes.ForEach((a) => World.Root.RootScene.Add(a));

            List<SimplePointLight> lights = new List<SimplePointLight>();
            for(int i = 0; i < 80; i++)
            {
                lights.Add(new SimplePointLight(new Vector3(0), Vector4.Zero));
                World.Root.RootScene.Add(lights[i]);
            }
                GLThread.OnBeforeDraw += (o, e) =>
            {
                float timeadd = (float)(DateTime.Now - GLThread.StartTime).TotalMilliseconds * 0.00001f;
                for(int x = 0; x < 80; x++)
                {
                    var center = new Vector3((x - 40) * 0.1f, (float)Math.Cos(x+ timeadd * x), (float)Math.Sin(x + timeadd * x));
                    lights[x].SetPosition(center);
                    lights[x].Color = new Vector4((float)Math.Sin(x + timeadd * x), (float)Math.Cos(x + timeadd * x), (float)Math.Cos(x + timeadd * 3 * x), 0.1f);

                    cubes[x].MainMaterial.Color = lights[x].Color;
                    cubes[x].Transformation.SetPosition(center);
                    cubes[x].Transformation.SetScale(0.05f);
                }
            };*/
            /*
            var scene = new VEngine.FileFormats.GameScene("home1.scene");
            //var instances = InstancedMesh3d.FromMesh3dList(testroom);
            scene.OnObjectFinish += (o, a) =>
            {
                if(!(a is Mesh3d))
                    return;
                var ob = a as Mesh3d;
                ob.SetOrientation(ob.GetPosition());
                //ob.SetCollisionShape(ob.MainObjectInfo.GetAccurateCollisionShape());
                //ob.SpecularComponent = 0.1f;
                //ob.MainMaterial.ReflectionStrength = ob.MainMaterial.SpecularComponent;
                //ob.SetCollisionShape(ob.ObjectInfo.GetAccurateCollisionShape());
                // ob.Material = new SolidColorMaterial(new Vector4(1, 1, 1, 0.1f));
                //(ob.MainMaterial as GenericMaterial).Type = GenericMaterial.MaterialType.WetDrops;
                //(ob.MainMaterial as GenericMaterial).BumpMap = null;
                this.Add(ob);
            };
            scene.Load();
            SimplePointLight p = new SimplePointLight(new Vector3(0, 3, 0), new Vector4(-2.2f, -2.2f, -2.2f, 2.6f));
            //LightPool.Add(p);
            GLThread.OnKeyPress += (o, e) =>
            {
                if(e.KeyChar == 'o')
                    p.Color = new Vector4(p.Color.X + 1.1f, p.Color.Y + 1.1f, p.Color.Z + 1.1f, p.Color.W);
                if(e.KeyChar == 'l')
                    p.Color = new Vector4(p.Color.X - 1.1f, p.Color.Y - 1.1f, p.Color.Z - 1.1f, p.Color.W);
                if(e.KeyChar == 'i')
                    p.Color = new Vector4(p.Color.X, p.Color.Y, p.Color.Z, p.Color.W + 1);
                if(e.KeyChar == 'k')
                    p.Color = new Vector4(p.Color.X, p.Color.Y, p.Color.Z, p.Color.W - 1);
            };*/
            // var s = VEngine.FileFormats.GameScene.FromMesh3dList(scene);
            // System.IO.File.WriteAllText("Scene.scene", s); Object3dInfo[] skydomeInfo =
            // Object3dInfo.LoadFromObj(Media.Get("sponza_verysimple.obj")); var sponza =
            // Object3dInfo.LoadSceneFromObj(Media.Get("cryteksponza.obj"),
            // Media.Get("cryteksponza.mtl"), 0.03f); List<Mesh3d> meshes = new List<Mesh3d>();
            /*   List<GenericMaterial> mats = new List<GenericMaterial>
               {
                   new GenericMaterial(new Vector4(1f, 0.6f, 0.6f, 1.0f)) {Roughness = 0.2f },
                   new GenericMaterial(new Vector4(0.9f, 0.9f, 0.9f, 1.0f)) {Roughness = 0.5f },
                   new GenericMaterial(new Vector4(0.6f, 0.6f, 1f, 1.0f)) {Roughness = 0.2f },
                   new GenericMaterial(new Vector4(1, 1, 2.05f, 1.0f)) {Roughness = 0.8f },
                   new GenericMaterial(new Vector4(0.6f, 1f, 1f, 1.0f)) {Roughness = 0.2f },
                   new GenericMaterial(new Vector4(1f, 0.6f, 1f, 1.0f)),
                   new GenericMaterial(new Vector4(1, 0, 0.1f, 1.0f)),
                   new GenericMaterial(new Vector4(1, 0, 0.1f, 1.0f)),
                   new GenericMaterial(new Vector4(1, 0, 0.1f, 1.0f)),
                   new GenericMaterial(new Vector4(1, 0, 0.1f, 1.0f)),
               };
               int ix = 0;
               foreach(var sd in skydomeInfo)
               {
                   var skydomeMaterial = mats[ix++ % mats.Count];
                   var skydome = new Mesh3d(sd, skydomeMaterial);
                   skydome.Scale(3);
                   meshes.Add(skydome);
               }*/
            // var Tracer = new PathTracer(); Tracer.PrepareTrianglesData(meshes);
            /* List<Triangle> triangles = new List<Triangle>();
             foreach(var mesh in scene)
             {
                 var Triangle = new Triangle();
                 Triangle.Tag = mesh;
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
                         Triangle.Tag = mesh;
                     }
                 }
             }
             SceneOctalTree tree = new SceneOctalTree();
             Triangle[] trcopy = new Triangle[triangles.Count];
             triangles.CopyTo(trcopy);
             tree.CreateFromTriangleList(trcopy.ToList());
             var TriangleCount = triangles.Count;
             // lets prepare byte array layout posx, posy, poz, norx, nory, norz, albr, albg, albz
             List<byte> bytes = new List<byte>();
             foreach(var triangle in triangles)
             {
                 foreach(var vertex in triangle.Vertices)
                 {
                     bytes.AddRange(BitConverter.GetBytes(vertex.Position.X));
                     bytes.AddRange(BitConverter.GetBytes(vertex.Position.Y));
                     bytes.AddRange(BitConverter.GetBytes(vertex.Position.Z));
                     bytes.AddRange(BitConverter.GetBytes((float)(triangle.Tag as Mesh3d).MainMaterial.Roughness));

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
             var octree = new SceneOctalTree();
             octree.CreateFromTriangleList(triangles);
             InstancedMesh3d ims = new InstancedMesh3d(Object3dInfo.LoadFromObjSingle(Media.Get("normalizedcube.obj")), new GenericMaterial(Color.White));
             Action<SceneOctalTree.Box> a = null;
             a = new Action<SceneOctalTree.Box>((box) =>
             {
                 if(box.Children.Count == 0)
                     ims.Transformations.Add(new TransformationManager(new Vector3(box.Center), Quaternion.Identity, box.Radius));
                 else
                     foreach(var b in box.Children)
                         a(b);
             });
             foreach(var b in octree.BoxTree.Children)
             {
                 a(b);
             }
             ims.UpdateMatrix();
             Add(ims);
             */
            //   PostProcessing.Tracer = Tracer;
            //var protagonist = Object3dInfo.LoadSceneFromObj(Media.Get("protagonist.obj"), Media.Get("protagonist.mtl"), 1.0f);
            //foreach(var o in protagonist)
            //    Add(o);
            /*
           var fountainWaterObj = Object3dInfo.LoadFromObjSingle(Media.Get("turbinegun.obj"));
           var water = new Mesh3d(fountainWaterObj, new GenericMaterial(new Vector4(1, 1, 1, 1)));
           water.Transformation.Scale(1.0f);
           water.Translate(0, 10, 0);
           Add(water);*/
            /*
             Object3dInfo waterInfo = Object3dGenerator.CreateTerrain(new Vector2(-200, -200), new Vector2(200, 200), new Vector2(30, 30), Vector3.UnitY, 333, (x, y) => 0);

            var color = new GenericMaterial(Color.SkyBlue);
            color.SetBumpMapFromMedia("cobblestone.jpg");
           // color.Type = GenericMaterial.MaterialType.Water;
            Mesh3d water2 = new Mesh3d(waterInfo, color);
            water2.SetMass(0);
             color.Roughness = 0.8f;
            water2.Translate(0, 0.1f, 0);
            water2.MainMaterial.ReflectionStrength = 1;
            //water.SetCollisionShape(new BulletSharp.StaticPlaneShape(Vector3.UnitY, 0));
            Add(water2);
            var dragon3dInfo = Object3dInfo.LoadFromObjSingle(Media.Get("apple.obj"));
            dragon3dInfo.ScaleUV(0.1f);
            var mat = GenericMaterial.FromMedia("skin.jpg");
            var dragon = new Mesh3d(dragon3dInfo, mat);
            //mat.Type = GenericMaterial.MaterialType.WetDrops;
            //dragon.Scale(5);
            dragon.SetMass(0);
            dragon.SetCollisionShape(dragon3dInfo.GetAccurateCollisionShape());
            Add(dragon);
             /*
             Object3dInfo waterInfo = Object3dGenerator.CreateTerrain(new Vector2(-200, -200), new Vector2(200, 200), new Vector2(100, 100), Vector3.UnitY, 333, (x, y) => 0);

             var color = new GenericMaterial(Color.Green);
             color.SetBumpMapFromMedia("grassbump.png");
             color.Type = GenericMaterial.MaterialType.Grass;
             Mesh3d water = new Mesh3d(waterInfo, color);
             water.SetMass(0);
             water.Translate(0, 1, 0);
             water.SetCollisionShape(new BulletSharp.StaticPlaneShape(Vector3.UnitY, 0));
             Add(water);

             var dragon3dInfo = Object3dInfo.LoadFromRaw(Media.Get("lucymidres.vbo.raw"), Media.Get("lucymidres.indices.raw"));
             dragon3dInfo.ScaleUV(0.1f);
             var dragon = new Mesh3d(dragon3dInfo, new GenericMaterial(Color.White));
             //dragon.Translate(0, 0, 20);
             dragon.Scale(80);
             Add(dragon);
             */
        }
    }
}