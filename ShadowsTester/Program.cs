using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDGTech;
using System.Threading.Tasks;
using System.Drawing;
using VDGTech.Generators;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.Vehicle;

namespace ShadowsTester
{
    class Program
    {
        static void Main(string[] args)
        {
            VEngineWindowAdapter window = null;
            var Config = SharpScript.CreateClass(System.IO.File.ReadAllText("Config.css"));
            Media.SearchPath = Config.MediaPath;

            var renderThread = Task.Factory.StartNew(() =>
            {
                window = new VEngineWindowAdapter("Test", Config.Width, Config.Height);
                window.Run(60);
            });
            GLThread.Invoke(() =>
            {
                window.SetCustomPostProcessingMaterial(new PostProcessLoadingMaterial());
            });
            World.Root = new World();

            System.Threading.Thread.Sleep(1000);

            var freeCamera = new FreeCamera();


            Object3dInfo icosphereInfo = Object3dInfo.LoadFromCompressed(Media.Get("Icosphere.o3i"));
            Mesh3d icosphere = new Mesh3d(icosphereInfo, new SolidColorMaterial(Color.White));

            icosphere.SetScale(30.0f);
            World.Root.Add(icosphere);
            /*
            Object3dInfo cube1Info = Object3dInfo.LoadFromCompressed(Media.Get("Cube_Cube.001.o3i"));
            Mesh3d cube1 = new Mesh3d(cube1Info, new SolidColorMaterial(Color.Yellow));
            World.Root.Add(cube1);

            Object3dInfo cube2Info = Object3dInfo.LoadFromCompressed(Media.Get("Cube.001_Cube.002.o3i"));
            Mesh3d cube2 = new Mesh3d(cube2Info, new SolidColorMaterial(Color.Yellow));
            World.Root.Add(cube2);

            Object3dInfo ballInfo = Object3dInfo.LoadFromCompressed(Media.Get("Mball_Meta.o3i"));
            Mesh3d ball = new Mesh3d(ballInfo, new SolidColorMaterial(Color.Yellow));
            World.Root.Add(ball);

            Object3dInfo textInfo = Object3dInfo.LoadFromCompressed(Media.Get("Text.o3i"));
            Mesh3d text = new Mesh3d(textInfo, new SolidColorMaterial(Color.Yellow));
            World.Root.Add(text);

            Object3dInfo suzanneInfo = Object3dInfo.LoadFromCompressed(Media.Get("Suzanne.o3i"));
            Mesh3d suzanne = new Mesh3d(suzanneInfo, new SolidColorMaterial(Color.Red));
            World.Root.Add(suzanne);


            Texture tankTex = new Texture(Media.Get("tank.png"));

            Object3dInfo tankHeadInfo = Object3dInfo.LoadFromCompressed(Media.Get("tank_head.o3i"));
            Mesh3d tankHead = new Mesh3d(tankHeadInfo, new SingleTextureMaterial(tankTex));
            World.Root.Add(tankHead);

            Object3dInfo tankBodyInfo = Object3dInfo.LoadFromCompressed(Media.Get("tank_body.o3i"));
            Mesh3d tankBody = new Mesh3d(tankBodyInfo, new SingleTextureMaterial(tankTex));

            tankBody.SetCollisionShape(new Box(tankBody.GetPosition(), 12, 4, 14, 1200.0f));
            tankBody.GetCollisionShape().CollisionInformation.LocalPosition += new Vector3(0, 2, 0).ToBepu();

            World.Root.Add(tankBody);

            var tankLinked = MeshLinker.Link(tankBody, tankHead, Vector3.Zero, Quaternion.Identity);

            System.Timers.Timer tankMover = new System.Timers.Timer(16);
            tankMover.Elapsed += (o, e) =>
            {
                tankLinked.Rotation = Quaternion.Multiply(tankLinked.Rotation, Quaternion.FromAxisAngle(Vector3.UnitY, 0.01f));
            };
            tankMover.Start();



            Object3dInfo fiatBodyInfo = Object3dInfo.LoadFromCompressed(Media.Get("cinkus_body.o3i"));
            Object3dInfo fiatWheelInfo = Object3dInfo.LoadFromCompressed(Media.Get("cinkus_wheel.o3i"));

            Mesh3d fiatBody = new Mesh3d(fiatBodyInfo, new SolidColorMaterial(Color.Cyan));
            fiatBody.SetCollisionShape(new Box(tankBody.GetPosition(), 12, 4, 14, 1200.0f));
            fiatBody.GetCollisionShape().CollisionInformation.LocalPosition += new Vector3(0, 2, 0).ToBepu();
            World.Root.Add(fiatBody);

            Mesh3d fiatWheelFR = new Mesh3d(fiatWheelInfo, new SolidColorMaterial(Color.Brown));
            World.Root.Add(fiatWheelFR);

            Mesh3d fiatWheelFL = new Mesh3d(fiatWheelInfo, new SolidColorMaterial(Color.Brown));
            fiatWheelFR.SetPosition(new Vector3(0.83068f, -0.05185f, -1.31027f));
            World.Root.Add(fiatWheelFL);

            Mesh3d fiatWheelRR = new Mesh3d(fiatWheelInfo, new SolidColorMaterial(Color.Brown));
            fiatWheelFR.SetPosition(new Vector3(0.83068f, -0.05185f, 1.24699f));
            World.Root.Add(fiatWheelRR);

            Mesh3d fiatWheelRL = new Mesh3d(fiatWheelInfo, new SolidColorMaterial(Color.Brown));
            fiatWheelFR.SetPosition(new Vector3(0.83068f, -0.05185f, 1.24699f));
            World.Root.Add(fiatWheelRL);

            var whellFR = MeshLinker.Link(fiatBody, fiatWheelFR, new Vector3(-0.83068f, 0.05185f, -1.31027f), Quaternion.Identity);
            var whellFL = MeshLinker.Link(fiatBody, fiatWheelFR, new Vector3(0.83068f, 0.05185f, -1.31027f), Quaternion.Identity);
            var whellRR = MeshLinker.Link(fiatBody, fiatWheelFR, new Vector3(-0.83068f, 0.05185f, 1.24699f), Quaternion.Identity);
            var whellRL = MeshLinker.Link(fiatBody, fiatWheelFR, new Vector3(0.83068f, 0.05185f, 1.24699f), Quaternion.Identity);
            */


            /*Object3dInfo coneInfo = Object3dInfo.LoadFromCompressed(Media.Get("Cone.o3i"));
            Mesh3d cone = new Mesh3d(coneInfo, new SolidColorMaterial(Color.White));
            cone.Translate(new Vector3(20, 20, 20));
            cone.Rotate(Quaternion.FromAxisAngle(new Vector3(1, 0, -1), MathHelper.Pi/3));
            World.Root.Add(cone);*/

            var datetex = Texture.FromText("Date", "Impact", 160.0f, Color.Black, Color.White);


            Random rand = new Random();

            Func<uint, uint, float> terrainGen = (x, y) =>
            {
                float h =
                    (SimplexNoise.Noise.Generate((float)x, (float)y) * 2) +
                    (SimplexNoise.Noise.Generate((float)x / 6, (float)y / 7) * 2) +
                    (SimplexNoise.Noise.Generate((float)x / 24, (float)y / 23) * 2) +
                    (SimplexNoise.Noise.Generate((float)x / 35, (float)y / 66) * 33) +
                    (SimplexNoise.Noise.Generate((float)x / 99, (float)y / 111) * 400);
                return h;
            };
            Func<uint, uint, float> waterGen = (x, y) =>
            {
                float h =
                    (SimplexNoise.Noise.Generate((float)x, (float)y) * 2) +
                    (SimplexNoise.Noise.Generate((float)x / 4, (float)y / 4) * 3) +
                    (SimplexNoise.Noise.Generate((float)x / 7, (float)y / 7) * 6);
                return h;
            };

            //Object3dInfo waterInfo = Object3dGenerator.CreateGround(new Vector2(-15000, -15000), new Vector2(15000, 15000), new Vector2(100, 100), Vector3.UnitY);
             //Object3dInfo groundInfo = Object3dGenerator.CreateTerrain(new Vector2(-4000, -4000), new Vector2(4000, 4000), new Vector2(10, 10), Vector3.UnitY, 512, terrainGen);
             Object3dInfo waterInfo = Object3dGenerator.CreateTerrain(new Vector2(-4000, -4000), new Vector2(4000, 4000), new Vector2(100, 100), Vector3.UnitY, 256, waterGen);

            //  Object3dInfo groundInfo = Object3dGenerator.CreateTerrain(new Vector2(-1000, -1000), new Vector2(1000, 1000), new Vector2(100, 100), Vector3.UnitY, 512, terrainGen);
            //Object3dInfo groundInfo = Object3dInfo.LoadFromCompressed(Media.Get("terrain4.o3i"));


             var texx = SingleTextureMaterial.FromMedia("tiles_tex.png", "tiles_map.png");
            var co = Color.FromArgb(255, Color.FromArgb(0, 92, 143));
            var color = new SolidColorMaterial(Color.Silver);
            var watercolor = new SolidColorMaterial(co);
            Mesh3d water = new Mesh3d(waterInfo, texx);
            water.SetStaticCollisionMesh(waterInfo.GetAccurateCollisionShape(Vector3.Zero));
            water.GetStaticCollisionMesh().Material.Bounciness = 1.0f;
            //water.SpecularSize = 15.0f;
            water.DiffuseComponent = 0.5f;
            World.Root.Add(water);
            Object3dInfo simplecubeInfo = Object3dInfo.LoadFromObj(Media.Get("cube_simple.obj"))[1];
            
            InstancedMesh3d instancedBalls = new InstancedMesh3d(simplecubeInfo, watercolor);
            for(int i = 0; i < 4000; i++)
            {
                instancedBalls.Positions.Add(new Vector3((float)Math.Sin((float)i / 30.0f) * 500, 300.0f + (float)Math.Cos((float)i / 30.0f) * 500, (i - 750.0f)));
                instancedBalls.Orientations.Add(Quaternion.Identity);
                instancedBalls.Scales.Add(rand.Next(11) + 11);

            }
            GLThread.OnBeforeDraw += (o, e) =>
            {
                for(int i = 0; i < 1000; i++)
                {
                    instancedBalls.Positions[i] = (new Vector3((float)Math.Sin((float)DateTime.Now.TimeOfDay.TotalMilliseconds * i / 600000.0f) * 500,
                        300.0f + (float)Math.Cos((float)DateTime.Now.TimeOfDay.TotalMilliseconds * i / 600000.0f) * 500, (i - 750.0f)));
                    instancedBalls.Rotate(i, Quaternion.FromAxisAngle(Vector3.UnitZ, 0.001f * i));
                }
                instancedBalls.UpdateMatrix();
            };
            instancedBalls.Instances = 1000;
            instancedBalls.UpdateMatrix();
            GLThread.OnUpdate += (o, e) =>
            {
            };
            World.Root.Add(instancedBalls);
            
            /*
            Mesh3d ground = new Mesh3d(groundInfo, texx);
            ground.SetStaticCollisionMesh(groundInfo.GetAccurateCollisionShape(Vector3.Zero));
            ground.GetStaticCollisionMesh().Material.Bounciness = 1.0f;
            World.Root.Add(ground);*/

            //  Mesh3d ground = new Mesh3d(groundInfo, texx);
            //   ground.SetStaticCollisionMesh(groundInfo.GetAccurateCollisionShape(Vector3.Zero));
            //  ground.GetStaticCollisionMesh().Material.Bounciness = 0.0f;
            // World.Root.Add(ground);

            /*Object3dInfo waterInfo = Object3dGenerator.CreateGround(new Vector2(-10000, -10000), new Vector2(10000, 10000), new Vector2(20, 20), Vector3.UnitY);

            Mesh3d water = new Mesh3d(waterInfo, new SolidColorMaterial(Color.FromArgb(140, Color.Blue)));
            water.SetPosition(new Vector3(0, -150, 0));
            World.Root.Add(water);*/
            /*
            Object3dInfo chickInfo = Object3dInfo.LoadFromObjSingle(Media.Get("cave.obj"));
            Mesh3d chick = new Mesh3d(chickInfo, color);
            chick.SetScale(20.4f);
            chick.SetPosition(new Vector3(0, 200.0f + 3.3f * 4, 0));
            World.Root.Add(chick);*/

            FOVLight coneLight = new FOVLight(new Vector3(65, 30, 65), Quaternion.FromAxisAngle(new Vector3(1, 0, -1), MathHelper.Pi / 3), 1024, 1024, MathHelper.PiOver2, 1.0f, 13200.0f);
            icosphere.SetPosition(new Vector3(65, 30, 65));
            LightPool.Add(coneLight);
            /*
            FOVLight coneLight2 = new FOVLight(new Vector3(-30, 15, -30), Quaternion.FromAxisAngle(new Vector3(1, 0, -1), MathHelper.Pi / 3), 256, 256, MathHelper.PiOver2 * 1.1f, 1.0f, 13200.0f);
            LightPool.Add(coneLight2);

            FOVLight coneLight3 = new FOVLight(new Vector3(30, 15, -30), Quaternion.FromAxisAngle(new Vector3(1, 0, -1), MathHelper.Pi / 3), 256, 256, MathHelper.PiOver2 * 1.1f, 1.0f, 13200.0f);
            LightPool.Add(coneLight3);

            FOVLight coneLight4 = new FOVLight(new Vector3(-30, 15, 30), Quaternion.FromAxisAngle(new Vector3(1, 0, -1), MathHelper.Pi / 3), 256, 256, MathHelper.PiOver2 * 1.1f, 1.0f, 13200.0f);
            LightPool.Add(coneLight4);

            FOVLight coneLight5 = new FOVLight(new Vector3(-30, 15, 30), Quaternion.FromAxisAngle(new Vector3(1, 0, -1), MathHelper.Pi / 3), 256, 256, MathHelper.PiOver2 * 1.1f, 1.0f, 13200.0f);
            LightPool.Add(coneLight5);

            FOVLight coneLight6 = new FOVLight(new Vector3(-30, 15, 30), Quaternion.FromAxisAngle(new Vector3(1, 0, -1), MathHelper.Pi / 3), 256, 256, MathHelper.PiOver2 * 1.1f, 1.0f, 13200.0f);
            LightPool.Add(coneLight6);
            */
            /*
            for(int i = 0; i < 12; i++)
            {
                Mesh3d a = new Mesh3d(icosphereInfo, color);
                a.SetScale(1);
                a.SetPosition(new Vector3(rand.Next(-150, 150), 25, rand.Next(-150, 150)));
                a.SetMass(15.5f);
                a.SetCollisionShape(new Sphere(a.GetPosition(), 0.8f, 20.0f));
                var s = a.GetCollisionShape();
                s.Material.Bounciness = 1.0f;
                s.LinearDamping = 0;
                s.Material.KineticFriction = 1.0f;
                s.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Discrete;
                
                World.Root.Add(a);
            }

            SingleTextureMaterial cubewood = new SingleTextureMaterial(new Texture(Media.Get("wood.jpg")));

            for(int i = 0; i < 3; i++)
                for(int g = 0; g < 16; g++)
                    for(int h = 0; h < 3; h++)
                    {
                        float scale = 25.0f;
                        float cubesize = 0.384960f * scale;
                        Mesh3d a = new Mesh3d(icosphereInfo, color);
                        a.SetScale(scale / 2.0f);
                        a.SetPosition(new Vector3(2.0f * i * cubesize, 2.0f * g * cubesize + 20.0f, 2.0f * cubesize * h + 20.0f));
                        a.SetMass(0.5f);
                        a.SpecularComponent = 1.0f;
                        a.DiffuseComponent = 0.3f;
                        a.SpecularSize = 5.1f;
                        a.SetCollisionShape(new Sphere(a.GetPosition(), cubesize, 11.1f));
                        var s = a.GetCollisionShape();
                        s.Material.Bounciness = 0.0f;
                        s.LinearDamping = 0;
                        s.Material.KineticFriction = 1.0f;
                        s.Material.StaticFriction = 1.0f;
                        s.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Discrete;

                        World.Root.Add(a);
                    }
            */
            //mirror

            Skybox skybox = new Skybox(ManualShaderMaterial.FromName("Skybox"));
            skybox.Use();

            Object3dInfo oldhouseInfo = Object3dInfo.LoadFromObjSingle(Media.Get("oldhouse.obj"));
            SolidColorMaterial oldhouseMaterial = new SolidColorMaterial(Color.Red);

            InstancedMesh3d oldhouseInstances = new InstancedMesh3d(oldhouseInfo, oldhouseMaterial);
            World.Root.Add(oldhouseInstances);

            GLThread.OnMouseUp += (o, e) =>
            {
                if(e.Button == OpenTK.Input.MouseButton.Right)
                {
                    Vector3 position = Camera.Current.RayCastPosition();
                    if(position != null)
                    {
                        Console.WriteLine(position);
                        World.Root.Explode(position, 2000.0f, 1000.0f);
                        icosphere.SetPosition(position);
                        oldhouseInstances.Positions.Add(position);
                        oldhouseInstances.Orientations.Add(Quaternion.Identity);
                        oldhouseInstances.Scales.Add(0.2f);
                        oldhouseInstances.Instances++;
                        oldhouseInstances.UpdateMatrix();

                    }
                }
                if(e.Button == OpenTK.Input.MouseButton.Middle)
                {
                    Mesh3d mesh = Camera.Current.RayCastMesh3d();
                    if(mesh != null && mesh.GetCollisionShape() != null)
                    {
                        Console.WriteLine(mesh.GetCollisionShape().ToString());
                        mesh.GetCollisionShape().LinearVelocity += (Camera.Current.GetDirection() * 20.0f).ToBepu();
                    }
                }
                if(e.Button == OpenTK.Input.MouseButton.Left)
                {
                    Mesh3d mesh = Camera.Current.RayCastMesh3d();
                    if(mesh != null && mesh.GetCollisionShape() != null)
                    {
                        Console.WriteLine(mesh.GetCollisionShape().ToString());
                        mesh.GetCollisionShape().LinearVelocity += (Vector3.UnitY * 20.0f).ToBepu();
                    }
                }

            };

            GLThread.OnKeyUp += (o, e) =>
            {
                if(e.Key == OpenTK.Input.Key.Number1)
                {
                    coneLight.SetPosition(freeCamera.Cam.Position, freeCamera.Cam.Position + freeCamera.Cam.Orientation.ToDirection());
                    //coneLight2.SetPosition(freeCamera.Cam.Position, freeCamera.Cam.Position - freeCamera.Cam.Orientation.ToDirection());
                    //coneLight3.SetPosition(freeCamera.Cam.Position, freeCamera.Cam.Position + freeCamera.Cam.Orientation.GetTangent(MathExtensions.TangentDirection.Left));
                    //coneLight4.SetPosition(freeCamera.Cam.Position, freeCamera.Cam.Position + freeCamera.Cam.Orientation.GetTangent(MathExtensions.TangentDirection.Right));
                    //coneLight5.SetPosition(freeCamera.Cam.Position, freeCamera.Cam.Position + freeCamera.Cam.Orientation.GetTangent(MathExtensions.TangentDirection.Down));
                    //coneLight6.SetPosition(freeCamera.Cam.Position, freeCamera.Cam.Position + freeCamera.Cam.Orientation.GetTangent(MathExtensions.TangentDirection.Up));
                    //icosphere.SetPosition(freeCamera.Cam.Position);
                    //icosphere.DiffuseComponent = 999.0f;
                }/*
                if(e.Key == OpenTK.Input.Key.Number2)
                {
                    coneLight2.SetPosition(freeCamera.Cam.Position, freeCamera.Cam.Orientation);
                    icosphere.SetPosition(freeCamera.Cam.Position);
                    icosphere.DiffuseComponent = 999.0f;
                }
                if(e.Key == OpenTK.Input.Key.Number3)
                {
                    coneLight3.SetPosition(freeCamera.Cam.Position, freeCamera.Cam.Orientation);
                    icosphere.SetPosition(freeCamera.Cam.Position);
                    icosphere.DiffuseComponent = 999.0f;
                }
                if(e.Key == OpenTK.Input.Key.Number4)
                {
                    coneLight4.SetPosition(freeCamera.Cam.Position, freeCamera.Cam.Orientation);
                    icosphere.SetPosition(freeCamera.Cam.Position);
                    icosphere.DiffuseComponent = 999.0f;
                }*/
            };
            /*
            System.Timers.Timer datetimer = new System.Timers.Timer(1000);
            datetimer.Elapsed += (o, e) =>
            {
                datetex.UpdateFromText(DateTime.Now.ToShortTimeString() + ":" + DateTime.Now.Second.ToString(), "Segoe UI", 160.0f, Color.Black, Color.White);
            };*/
            //datetimer.Start();
            GLThread.Invoke(() => window.StartPhysicsThread());
            GLThread.Invoke(() => window.SetDefaultPostProcessingMaterial());
            renderThread.Wait();
        }
    }
}
