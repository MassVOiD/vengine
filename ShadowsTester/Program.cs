using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDGTech;
using System.Threading.Tasks;
using System.Drawing;
using VDGTech.Generators;
using VDGTech.Particles;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.Vehicle;

namespace ShadowsTester
{
    class Program
    {
        public static FreeCamera FreeCam;
        static void Main(string[] args)
        {
            //System.Threading.Thread.Sleep(1000);
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

            //System.Threading.Thread.Sleep(1000);

            var freeCamera = new FreeCamera();
            FreeCam = freeCamera;

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

            Texture tankTex = new Texture(Media.Get("tank.png"));

            Object3dInfo tankHeadInfo = Object3dInfo.LoadFromCompressed(Media.Get("tank_head.o3i"));
            //tankHeadInfo.Normalize();
            Mesh3d tankHead = new Mesh3d(tankHeadInfo, new SingleTextureMaterial(tankTex));
            tankHead.SetScale(10.0f);
            World.Root.Add(tankHead);

            Object3dInfo tankBodyInfo = Object3dInfo.LoadFromCompressed(Media.Get("tank_body.o3i"));
            //tankBodyInfo.Normalize();
            Mesh3d tankBody = new Mesh3d(tankBodyInfo, new SingleTextureMaterial(tankTex));
            tankBody.SetScale(10.0f);

            tankBody.SetPosition(new Vector3(66, 33, 0));
            tankBody.SetCollisionShape(tankBodyInfo.GetConvexHull(tankBody.GetPosition(), 10.0f, 1000.0f));
            //tankBody.GetCollisionShape().CollisionInformation.LocalPosition += new Vector3(0, 2, 0).ToBepu();
            tankBody.GetCollisionShape().PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Continuous;
            tankBody.GetCollisionShape().LinearDamping = 0.9f;
            tankBody.GetCollisionShape().Material.KineticFriction = 0.7f;
            tankBody.GetCollisionShape().Material.StaticFriction = 0.70f;

            World.Root.Add(tankBody);

            var tankLinked = MeshLinker.Link(tankBody, tankHead, Vector3.Zero, Quaternion.Identity);

            System.Timers.Timer tankMover = new System.Timers.Timer(16);
            tankMover.Elapsed += (o, e) =>
            {
                tankLinked.Rotation = Quaternion.Multiply(tankLinked.Rotation, Quaternion.FromAxisAngle(Vector3.UnitY, 0.01f));
            };
            tankMover.Start();

            GLThread.OnUpdate += (o, e) =>
            {
                var keyboard = OpenTK.Input.Keyboard.GetState();
                if(keyboard.IsKeyDown(OpenTK.Input.Key.Up))
                {
                    var dir = tankBody.GetOrientation().ToDirection();
                    dir.Y = 0;
                    tankBody.GetCollisionShape().ApplyImpulse(tankBody.GetPosition() - dir, -dir.ToBepu() * 16140.0f);
                }
                if(keyboard.IsKeyDown(OpenTK.Input.Key.Down))
                {
                    var dir = tankBody.GetOrientation().ToDirection();
                    dir.Y = 0;
                    tankBody.GetCollisionShape().ApplyImpulse(tankBody.GetPosition() - dir, dir.ToBepu() * 16140.0f);
                }
                if(keyboard.IsKeyDown(OpenTK.Input.Key.Left))
                {
                    var dir = tankBody.GetOrientation().ToDirection();
                    //dir.Y = 0;
                    tankBody.GetCollisionShape().ApplyImpulse((tankBody.GetPosition() - dir * 100.0f), tankBody.GetOrientation().GetTangent(MathExtensions.TangentDirection.Left).ToBepu() * 5140.0f);
                    tankBody.GetCollisionShape().ApplyImpulse((tankBody.GetPosition() + dir * 100.0f), tankBody.GetOrientation().GetTangent(MathExtensions.TangentDirection.Right).ToBepu() * 5140.0f);
                }
                if(keyboard.IsKeyDown(OpenTK.Input.Key.Right))
                {
                    var dir = tankBody.GetOrientation().ToDirection();
                    dir.Y = 0;
                    tankBody.GetCollisionShape().ApplyImpulse((tankBody.GetPosition() + dir * 100.0f), tankBody.GetOrientation().GetTangent(MathExtensions.TangentDirection.Left).ToBepu() * 5140.0f);
                    tankBody.GetCollisionShape().ApplyImpulse((tankBody.GetPosition() - dir * 100.0f), tankBody.GetOrientation().GetTangent(MathExtensions.TangentDirection.Right).ToBepu() * 5140.0f);
                }
                if(tankBody.GetCollisionShape().AngularVelocity.Length() > 0.5f)
                {
                    tankBody.GetCollisionShape().AngularVelocity = tankBody.GetCollisionShape().AngularVelocity.ToOpenTK() * 0.4f;
                }
                if(tankBody.GetCollisionShape().LinearVelocity.Length() > 60.0f)
                {
                   // tankBody.GetCollisionShape().LinearVelocity = tankBody.GetCollisionShape().LinearVelocity.ToOpenTK() * 0.9f;
                }
            };

            Random rand = new Random();

            Func<uint, uint, float> terrainGen = (x, y) =>
            {
                float h =
                    (SimplexNoise.Noise.Generate((float)x, (float)y)) +
                    (SimplexNoise.Noise.Generate((float)x / 6, (float)y / 7) * 8) +
                    (SimplexNoise.Noise.Generate((float)x / 24, (float)y / 23) * 31) +
                    (SimplexNoise.Noise.Generate((float)x / 35, (float)y / 66) * 80) +
                    (SimplexNoise.Noise.Generate((float)x / 99, (float)y / 111) * 122);
                return h;
            };
            Func<uint, uint, float> waterGen = (x, y) =>
            {
                float h =
                    (SimplexNoise.Noise.Generate((float)x, (float)y) / 2) +
                    (SimplexNoise.Noise.Generate((float)x / 4, (float)y / 4));
                return h;
            };

            //Object3dInfo waterInfo = Object3dGenerator.CreateGround(new Vector2(-15000, -15000), new Vector2(15000, 15000), new Vector2(100, 100), Vector3.UnitY);
            //Object3dInfo groundInfo = Object3dGenerator.CreateTerrain(new Vector2(-1500, -1500), new Vector2(1500, 1500), new Vector2(100, 100), Vector3.UnitY, 256, terrainGen);
            Object3dInfo waterInfo = Object3dGenerator.CreateTerrain(new Vector2(-1500, -1500), new Vector2(1500, 1500), new Vector2(100, 100), Vector3.UnitY, 24, waterGen);

            //  Object3dInfo groundInfo = Object3dGenerator.CreateTerrain(new Vector2(-1000, -1000), new Vector2(1000, 1000), new Vector2(100, 100), Vector3.UnitY, 512, terrainGen);
            //Object3dInfo groundInfo = Object3dInfo.LoadFromCompressed(Media.Get("terrain4.o3i"));


            var texx = new ManualShaderMaterial(Media.ReadAllText("Tesselation.vertex.glsl"), Media.ReadAllText("SingleTextureMaterial.fragment.glsl"), null,
            Media.ReadAllText("Generic.tesscontrol.glsl"), Media.ReadAllText("Generic.tesseval.glsl"));
            texx.SetTexture(new Texture(Media.Get("grass.jpg")));
            var co = Color.FromArgb(255, Color.FromArgb(0, 94, 255));
            var color = new SolidColorMaterial(Color.Silver);
            var watercolor = new SolidColorMaterial(co);
            Mesh3d water = new Mesh3d(waterInfo, texx);
            water.SetStaticCollisionMesh(waterInfo.GetAccurateCollisionShape(Vector3.Zero));
            water.GetStaticCollisionMesh().Material.Bounciness = 1.0f;
            //water.SpecularSize = 15.0f;
            // water.DiffuseComponent = 0.5f;
            World.Root.Add(water);
            Object3dInfo simplecubeInfo = Object3dInfo.LoadFromObj(Media.Get("cube_simple.obj"))[1];
            /*
            InstancedMesh3d instancedBalls = new InstancedMesh3d(simplecubeInfo, watercolor);
            for(int i = 0; i < 1000; i++)
            {
                instancedBalls.Positions.Add(new Vector3((float)Math.Sin((float)i / 30.0f) * 500, 300.0f + (float)Math.Cos((float)i / 30.0f) * 500, (i - 750.0f)));
                instancedBalls.Orientations.Add(Quaternion.Identity);
                instancedBalls.Scales.Add(rand.Next(11) + 11);

            }
            GLThread.OnUpdate += (o, e) =>
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
            World.Root.Add(instancedBalls);*/

            Object3dInfo cube3dInfo = Object3dInfo.LoadFromObjSingle(Media.Get("cube_simple.obj"));
            /*
            Airplane copter = new Airplane();
            World.Root.Add(copter);

            bool Shooting = false;
            GLThread.OnKeyDown += (o, e) =>
            {
                if(e.Key == OpenTK.Input.Key.Space)
                {
                    //if (Shooting) return;
                    Shooting = true;
                    Mesh3d ball = new Mesh3d(cube3dInfo, new SolidColorMaterial(Color.Yellow));
                    ball.SetScale(10.0f);
                    ball.SetPosition(copter.Body.GetPosition() + copter.Body.GetOrientation().ToDirection() * 20.0f);
                    ball.SetCollisionShape(new Sphere(ball.GetPosition(), 10.0f, 4.1f));
                    ball.SetMass(4.1f);
                    World.Root.Add(ball);
                    Vector3 direction = copter.Body.GetOrientation().ToDirection() * 1140.0f;
                    ball.GetCollisionShape().ApplyImpulse(Vector3.Zero, direction);
                }
            };
            GLThread.OnKeyUp += (o, e) =>
            {
                if(e.Key == OpenTK.Input.Key.Space)
                {
                    Shooting = false;
                }
            };*/
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


              ProjectionLight coneLight = new ProjectionLight(new Vector3(65, 0, 65), Quaternion.FromAxisAngle(new Vector3(1, 0, -1), MathHelper.Pi / 2), 7300, 7300, MathHelper.PiOver2, 1.0f, 13200.0f);
            icosphere.SetPosition(new Vector3(65, 30, 65));
            //coneLight.BuildOrthographicProjection(4000, 4000, -1000, 1000);
            LightPool.Add(coneLight);


            //var gen = ParticleGenerator.CreateBox(new Vector3(0, 100, 0), new Vector3(1000, 100, 1000), Quaternion.Identity, Vector3.UnitZ, Vector3.UnitZ * 50.0f, 1.0f, 1.0f, 2.0f);
            //ParticleSystem.Generators.Add(gen);
            
            var planet3dInfo = Object3dInfo.LoadFromObjSingle(Media.Get("planet.obj"));
            planet3dInfo.Normalize();
            Mesh3d planet = new Mesh3d(planet3dInfo, new SolidColorMaterial(Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255))));
            planet.SetScale(130.0f);
            planet.Translate(new Vector3(0, 6500, 0));
            planet.SetCollisionShape(planet3dInfo.GetConvexHull(planet.GetPosition(), 130.0f, 0));
            World.Root.Add(planet);

            
            System.Timers.Timer lensFocusTimer = new System.Timers.Timer();
            lensFocusTimer.Interval = 100;
            lensFocusTimer.Elapsed += (o, e) =>
            {
                GLThread.Invoke(() =>
                Camera.Current.CurrentDepthFocus = (Camera.Current.CurrentDepthFocus * 4.0f + window.PostProcessFramebuffer.GetDepth(0.5f, 0.5f)) / 5.0f);
            };
            lensFocusTimer.Start();

            for(int i = 0; i < 7; i++)
            {
                Mesh3d a = new Mesh3d(icosphereInfo, watercolor);
                a.SetScale(5 + i );
                a.SetPosition(new Vector3(rand.Next(-550, 550), rand.Next(20, 150), rand.Next(-550,550)));
                a.SetMass(15.5f);
                a.SetCollisionShape(new Sphere(a.GetPosition(), 0.8f * (5 + i ), 20.0f));
                var s = a.GetCollisionShape();
                s.Material.Bounciness = 1.0f;
                s.LinearDamping = 0;
                s.Material.KineticFriction = 1.0f;
                s.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Discrete;
                
                World.Root.Add(a);
            }

            //SingleTextureMaterial cubewood = new SingleTextureMaterial(new Texture(Media.Get("wood.jpg")));

            var pipe2dinfo = Object3dInfo.LoadFromObjSingle(Media.Get("pipe.obj"));
            pipe2dinfo.Normalize();
            for(int i = 0; i < 9; i++)
                    for(int h = 0; h < 1; h++)
                    {
                        float scale = 40.0f;
                        Mesh3d a = new Mesh3d(pipe2dinfo, color);
                        a.SetScale(scale);
                        a.SetPosition(new Vector3(20.0f * i, 72.0f, 20.0f * h+40.0f));
                        a.SetMass(14.5f);
                        a.SpecularComponent = 0.7f;
                        a.DiffuseComponent = 0.3f;
                        a.SpecularSize = 2.1f;
                        a.SetCollisionShape(new Cylinder(a.GetPosition(), 1.0f * scale, 0.06f* scale, 14.0f));

                        World.Root.Add(a);
                    }
            
            //mirror
            
            //Object3dInfo flat3dInfo = Object3dInfo.LoadFromObjSingle(Media.Get("block_flat.obj"));
            IMaterial flatCursorMaterial = new SolidColorMaterial(Color.Blue);
            IMaterial flatBlockMaterial = new SolidColorMaterial(Color.White);
            Mesh3d flatCursor = new Mesh3d(cube3dInfo, flatCursorMaterial);
            flatCursor.SetScale(10.0f);
            InstancedMesh3d flatBlock = new InstancedMesh3d(cube3dInfo, flatCursorMaterial);

            List<Mesh3d> blocks = new List<Mesh3d>();
            if(System.IO.File.Exists("map.bin"))
            {
                var loaded = MapSaveLoad.LoadMeshes("map.bin");
                foreach(var item in loaded)
                {
                    Mesh3d block = new Mesh3d(cube3dInfo, flatBlockMaterial);
                    block.SetScale(10.0f);
                    block.SetPosition(item.Position);
                    block.SetOrientation(item.Orientation);
                    block.SetStaticCollisionMesh(cube3dInfo.GetAccurateCollisionShape(block.GetPosition(), 10.0f));
                    World.Root.Add(block);
                    blocks.Add(block);
                }
            }

            World.Root.Add(flatCursor);

            GLThread.OnUpdate += (o, e) =>
            {
                Vector3 position = Camera.Current.RayCastPosition();
                if(position != null)
                {
                    position.X = (float)Math.Floor(position.X / (0.384960f * 20.0f)) * 0.384960f * 20.0f + (0.384960f * 10.0f);
                    position.Y = (float)Math.Floor((position.Y + (0.384960f * 10.0f)) / (0.384960f * 20.0f)) * 0.384960f * 20.0f + (0.384960f * 10.0f);
                    position.Z = (float)Math.Floor(position.Z / (0.384960f * 20.0f)) * 0.384960f * 20.0f + (0.384960f * 10.0f);
                    flatCursor.SetPosition(position);
                }
            };

            GLThread.OnMouseUp += (o, e) =>
            {
                if(e.Button == OpenTK.Input.MouseButton.Left)
                {
                    Mesh3d block = new Mesh3d(cube3dInfo, flatBlockMaterial);
                    block.SetScale(10.0f);
                    block.SetPosition(flatCursor.GetPosition());
                    block.SetCollisionShape(cube3dInfo.GetConvexHull(block.GetPosition(), 10.0f));
                    World.Root.Add(block);
                    blocks.Add(block);
                    MapSaveLoad.SaveArray("map.bin", blocks.ToArray());
                }
            };
            
            Skybox skybox = new Skybox(ManualShaderMaterial.FromName("Skybox"));
            skybox.Use();

            Object3dInfo treeRoot = Object3dInfo.LoadFromObjSingle(Media.Get("tree_root.obj"));
            Object3dInfo treeOld = Object3dInfo.LoadFromObjSingle(Media.Get("tree_head_old.obj"));
            Object3dInfo treeNormal = Object3dInfo.LoadFromObjSingle(Media.Get("tree_head_normal.obj"));
            Object3dInfo treeLong = Object3dInfo.LoadFromObjSingle(Media.Get("tree_head_long.obj"));
            SolidColorMaterial greenMaterial = new SolidColorMaterial(Color.Green);
            SolidColorMaterial brownMaterial = new SolidColorMaterial(Color.Brown);

            InstancedMesh3d rootInstances = new InstancedMesh3d(treeRoot, brownMaterial);
            InstancedMesh3d oldInstances = new InstancedMesh3d(treeOld, brownMaterial);
            InstancedMesh3d normalInstances = new InstancedMesh3d(treeNormal, greenMaterial);
            InstancedMesh3d longInstances = new InstancedMesh3d(treeLong, greenMaterial);
            /*
            for(int i = -32; i < 32; i++)
            {

                for(int g = -32; g < 32; g++)
                {
                    Vector3 position = new Vector3((float)i * 73.0f + (float)rand.NextDouble() * 43.0f, 0, (float)g * 73.0f + (float)rand.NextDouble() * 43.0f);
                    rootInstances.Positions.Add(position);
                    rootInstances.Orientations.Add(Quaternion.Identity);
                    float scale = 6.6f * (float)rand.NextDouble() + 3.0f;
                    rootInstances.Scales.Add(scale);
                    rootInstances.Instances++;
                    int choice = rand.Next(0, 3);
                    if(choice == 0)
                    {

                        oldInstances.Positions.Add(position);
                        oldInstances.Orientations.Add(Quaternion.Identity);
                        oldInstances.Scales.Add(scale);
                        oldInstances.Instances++;
                    }
                    else if(choice == 1)
                    {

                        normalInstances.Positions.Add(position);
                        normalInstances.Orientations.Add(Quaternion.Identity);
                        normalInstances.Scales.Add(scale);
                        normalInstances.Instances++;
                    }
                    else if(choice == 2)
                    {

                        longInstances.Positions.Add(position);
                        longInstances.Orientations.Add(Quaternion.Identity);
                        longInstances.Scales.Add(scale);
                        longInstances.Instances++;
                    }
                }
            }
            rootInstances.UpdateMatrix();
            oldInstances.UpdateMatrix();
            normalInstances.UpdateMatrix();
            longInstances.UpdateMatrix();*/
            World.Root.Add(rootInstances);
            World.Root.Add(oldInstances);
            World.Root.Add(normalInstances);
            World.Root.Add(longInstances);
            GLThread.OnMouseUp += (o, e) =>
            {
                if(e.Button == OpenTK.Input.MouseButton.Right)
                {
                    Vector3 position = Camera.Current.RayCastPosition();
                    if(position != null)
                    {
                        Console.WriteLine(position);
                        World.Root.Explode(position, 2200.0f, 1000.0f);
                        //icosphere.SetPosition(position);


                        rootInstances.Positions.Add(position + new Vector3(0, 12, 0));
                        rootInstances.Orientations.Add(Quaternion.Identity);
                        rootInstances.Scales.Add(3.6f);
                        rootInstances.Instances++;
                        rootInstances.UpdateMatrix();
                        int choice = rand.Next(0, 3);
                        if(choice == 0)
                        {

                            oldInstances.Positions.Add(position + new Vector3(0, 12, 0));
                            oldInstances.Orientations.Add(Quaternion.Identity);
                            oldInstances.Scales.Add(3.6f);
                            oldInstances.Instances++;
                            oldInstances.UpdateMatrix();
                        }
                        else if(choice == 1)
                        {

                            normalInstances.Positions.Add(position + new Vector3(0, 12, 0));
                            normalInstances.Orientations.Add(Quaternion.Identity);
                            normalInstances.Scales.Add(3.6f);
                            normalInstances.Instances++;
                            normalInstances.UpdateMatrix();
                        }
                        else if(choice == 2)
                        {

                            longInstances.Positions.Add(position + new Vector3(0, 12, 0));
                            longInstances.Orientations.Add(Quaternion.Identity);
                            longInstances.Scales.Add(3.6f);
                            longInstances.Instances++;
                            longInstances.UpdateMatrix();
                        }



                        //float depth = (position - Camera.Current.Position).Length;
                        //float badass_depth = (float)(Math.Log(0.01f * depth + 1.0f) / Math.Log(0.01f * Camera.Current.Far + 1.0f));
                        //Camera.Current.CurrentDepthFocus = badass_depth;

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

            GLThread.OnMouseWheel += (o, e) =>
            {
                Camera.Current.LensBlurAmount -= e.Delta / 20.0f;
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
                }

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
