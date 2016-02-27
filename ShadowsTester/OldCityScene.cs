using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using VEngine;
using VEngine.FileFormats;

namespace ShadowsTester
{
    public class OldCityScene
    {
        dynamic max(params dynamic[] obj)
        {
            dynamic m = obj[0];
            foreach(dynamic a in obj)
                if(a > m)
                    m = a;
            return m;
        }
        dynamic min(params dynamic[] obj)
        {
            dynamic m = obj[0];
            foreach(dynamic a in obj)
                if(a < m)
                    m = a;
            return m;
        }
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

            var cube = VEngine.Generators.Object3dGenerator.CreateCube(new Vector3(300), new Vector2(10, 10));
            cube.ReverseFaces();
            var mesh = Mesh3d.Create(new Object3dInfo(cube.Vertices), new GenericMaterial()
            {
                Roughness = 1.0f,
                DiffuseColor = new Vector3(20),
                SpecularColor = Vector3.Zero
            });
            //  Game.World.Scene.Add(mesh);

             var scene = new GameScene("Scene.scene");
             scene.Load();
           // var scene = Object3dManager.LoadFromObj(Media.Get("originalsponza.obj"));
            var cnt = scene.Meshes.Count;
            var sc = new Vector3(2.0f, 2.0f, 2.0f);
           // VoxelGI gi = Game.DisplayAdapter.MainRenderer.VXGI;
            Game.Invoke(() =>
            {
                PassiveVoxelizer vox = new PassiveVoxelizer();
                List<VoxelGI.VoxelContainer> containers = new List<VoxelGI.VoxelContainer>();
                Random rand = new Random();
                for(var i = 0; i < cnt; i++)
                {
                    var o = scene.Meshes[i];
                   // var b = o.GetInstance(0).GetPosition();
                   //  b = new Vector3(b.X * 0.25f, b.Y * 0.25f, b.Z * 0.25f);
                   // o.GetInstance(0).SetPosition(b);
                   // o.GetInstance(0).Scale(0.25f);
                   // var bdshape = o.GetLodLevel(0).Info3d.GetConvexHull();
                   // var bd = Game.World.Physics.CreateBody(0, o.GetInstance(0), bdshape);
                   // bd.Enable();
                    Game.World.Scene.Add(o);
                    /*
                    // voxelize
                    float acceptableVoxelSize = 0.2f;
                    Object3dManager mgm = scene[i];
                    mgm.RecalulateNormals(Object3dManager.NormalRecalculationType.Flat);
                    var aabb = mgm.GetAxisAlignedBox();
                    var aabbEx = mgm.GetAxisAlignedBoxEx();
                    float maxbba = max(aabb.X, aabb.Y, aabb.Z);
                    //// if(maxbba > 9.0)
                    //      continue;
                    float divc = maxbba / acceptableVoxelSize;
                    int grid = min(32, (int)Math.Floor(divc));

                    Console.WriteLine("VOXELIZING:: {0} GRID SIZE:: {1}", o.GetInstance(0).Name, grid);

                    var voxels = vox.Voxelize(mgm, grid);
                    var vw = new Vector3(
                        (float)rand.NextDouble(),
                        (float)rand.NextDouble(),
                        (float)rand.NextDouble()
                    );
                    voxels.ForEach((a) => a.Albedo = vw*0.01f);
                    Console.WriteLine("RESULT COUNT:: {0}", voxels.Count);

                    var container = new VoxelGI.VoxelContainer(aabbEx[0], aabbEx[1], voxels);
                    o.GIContainer = containers.Count;
                    containers.Add(container);*/

                    o.GetLodLevel(0).Info3d.Manager = null;
                    //   bd.Enable();

                }
                //gi.UpdateVoxels(containers);
                Game.OnBeforeDraw += (aa, aaa) =>
                {
                  //  gi.UpdateGI();
                };
                GenericMaterial.UpdateMaterialsBuffer();
            });


            //   var sss = Object3dManager.LoadSceneFromObj("sintel.obj", "sintel.mtl");
            //  sss.ForEach((a) => Game.World.Scene.Add(a));
            /*
            SimplePointLight spl = new SimplePointLight(new Vector3(0, 2, 0), new Vector3(12, 12, 12));
            spl.Angle = 90;
            spl.SetOrientation(new Vector3(1, -1, 0).ToQuaternion(Vector3.UnitY));
            float t = 0;
            Game.OnUpdate += (z, xx) =>{
                spl.SetPosition((float)Math.Sin(t), 2, 0);
                    t += (float)0.01;
            };
            Game.World.Scene.Add(spl);*/
            /*
            var lss = new List<SimplePointLight>();
            for(int x = 0; x < 500; x++)
            {
                SimplePointLight light = new SimplePointLight(new Vector3(-4 + (8 * (x/100.0f)), 1, 0), new Vector4((float)Math.Sin(x * 0.1), (float)Math.Cos(x*0.6), (float)Math.Tan(x * 0.9), 1.0f));
                Game.World.Scene.Add(light);
                lss.Add(light);
            }

            Game.OnBeforeDraw += (oo, ee) =>
            {
                float a = (float)(DateTime.Now - Game.StartTime).TotalMilliseconds / 1000;
                for(int x = 0; x < 100; x++)
                {
                    lss[x].SetPosition(new Vector3(-4 + (8 * (x / 100.0f)), 1 + (float)Math.Sin(x * 0.1 + a), (float)Math.Cos(x * 0.6 + a)));
                }
            };*/

            // var viperobj = Object3dManager.LoadSceneFromObj(Media.Get("viper.obj"), Media.Get("viper.mtl"), 0.1f);
            // viperobj.ForEach((a) => a.GetInstance(0).Rotate(Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(90))));
            //  viperobj.ForEach((a) => Game.World.Scene.Add(a));

            /*   var lucy2 = Mesh3d.Create(new Object3dInfo(Object3dManager.LoadFromRaw(Media.Get("lucy.vbo.raw")).Vertices), GenericMaterial.FromColor(Color.Gold));
               lucy2.GetInstance(0).Scale(0.2f);
               lucy2.GetLodLevel(0).Material.Roughness = 0.2f;
               lucy2.GetLodLevel(0).Material.Metalness = 0.01f;
               Game.World.Scene.Add(lucy2);*/
            /*
         var barrelmanager = Object3dManager.LoadFromObjSingle(Media.Get("barrel.obj"));
         var barrelinfo = new Object3dInfo(barrelmanager.Vertices);
         var barrelshape = Physics.CreateConvexCollisionShape(barrelmanager);
         var barrels = Mesh3d.Create(barrelinfo, new GenericMaterial());
         barrels.AutoRecalculateMatrixForOver16Instances = true;
         barrels.GetLodLevel(0).Material.Roughness = 0.0f;
         barrels.ClearInstances();

         Game.OnKeyUp += (ox, oe) =>
         {
             if(oe.Key == OpenTK.Input.Key.Keypad0)
             {
                 var instance = barrels.AddInstance(new TransformationManager(Camera.MainDisplayCamera.GetPosition()));
                 var phys = Game.World.Physics.CreateBody(0.7f, instance, barrelshape);
                 phys.Enable();
                 phys.Body.LinearVelocity += Camera.MainDisplayCamera.GetDirection() * 6;
             }
         };
         Game.World.Scene.Add(barrels);*/
            // DynamicCubeMapController.Create();
            /*
            List<Mesh3d> cubes = new List<Mesh3d>();
            for(int i = 0; i < 80; i++)
                cubes.Add(new Mesh3d(Object3dInfo.LoadFromObjSingle(Media.Get("normalizedcube.obj")), new GenericMaterial(Color.White)));
            cubes.ForEach((a) => Game.World.Scene.Add(a));

            List<SimplePointLight> lights = new List<SimplePointLight>();
            for(int i = 0; i < 80; i++)
            {
                lights.Add(new SimplePointLight(new Vector3(0), Vector4.Zero));
                Game.World.Scene.Add(lights[i]);
            }
                Game.OnBeforeDraw += (o, e) =>
            {
                float timeadd = (float)(DateTime.Now - Game.StartTime).TotalMilliseconds * 0.00001f;
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
            Game.OnKeyPress += (o, e) =>
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