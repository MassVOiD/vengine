using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using VDGTech;
using VDGTech.Generators;
using OpenTK;
using BulletSharp;

namespace SimpleCarGame
{
    class CarScene : Scene
    {
        public static Mesh3d CurrentCar;
        public CarScene()
        {


            Object3dInfo waterInfo = Object3dGenerator.CreateGround(new Vector2(-200, -200), new Vector2(200, 200), new Vector2(100, 100), Vector3.UnitY);


            var color = SingleTextureMaterial.FromMedia("177.jpg", "177_norm.JPG");
            Mesh3d water = new Mesh3d(waterInfo, color);
            water.SetMass(0);
            water.Translate(0, -10, 0);
            water.SetCollisionShape(new BulletSharp.StaticPlaneShape(Vector3.UnitY, 0));
            World.Root.Add(water);

            ProjectionLight redConeLight = new ProjectionLight(new Vector3(1, 25, 1), Quaternion.Identity, 5000, 5000, MathHelper.PiOver2, 1.0f, 10000.0f);
            redConeLight.LightColor = new Vector4(1, 1, 1, 1);
            redConeLight.BuildOrthographicProjection(500, 500, -100, 100);
            redConeLight.camera.LookAt(Vector3.Zero);

            LightPool.Add(redConeLight);


            var wheel3dInfo = Object3dInfo.LoadFromObjSingle(Media.Get("fcarwheel.obj"));
            var car3dInfo = Object3dInfo.LoadFromObjSingle(Media.Get("fcarbody.obj"));

            var car = new Mesh3d(car3dInfo, new SolidColorMaterial(Color.LightGreen));
            car.SetMass(200);
            //car.Translate(0, 3.76f, 0);
            car.SetCollisionShape(new BoxShape(0.5f, 0.5f, 0.5f));
            car.CreateRigidBody(new Vector3(0, -25, 0), true);
            CurrentCar = car;
            World.Root.Add(car);

            GLThread.OnUpdate += (o, e) =>
            {
                redConeLight.SetPosition(car.GetPosition() + new Vector3(0, 15, 0));
            };

            var shape = new SphereShape(0.5f);
            //var shape = wheel3dInfo.GetAccurateCollisionShape();
            var wheelLF = new Mesh3d(wheel3dInfo, new SolidColorMaterial(Color.White));
            wheelLF.SetMass(100);
            wheelLF.Translate(1.640539f, 0.48866f, -1.94906f);
            wheelLF.SetCollisionShape(shape);
            wheelLF.CreateRigidBody();
            World.Root.Add(wheelLF);
            var wheelRF = new Mesh3d(wheel3dInfo, new SolidColorMaterial(Color.White));
            wheelRF.SetMass(100);
            wheelRF.Translate(1.640539f, 0.48866f, 1.94906f);
            wheelRF.SetCollisionShape(shape);
            wheelRF.CreateRigidBody();
            World.Root.Add(wheelRF);
            var wheelLR = new Mesh3d(wheel3dInfo, new SolidColorMaterial(Color.White));
            wheelLR.SetMass(100);
            wheelLR.Translate(-1.640539f, 0.48866f, -1.94906f);
            wheelLR.SetCollisionShape(shape);
            wheelLR.CreateRigidBody();
            World.Root.Add(wheelLR);
            var wheelRR = new Mesh3d(wheel3dInfo, new SolidColorMaterial(Color.White));
            wheelRR.SetMass(100);
            wheelRR.Translate(-1.640539f, 0.48866f, 1.94906f);
            wheelRR.SetCollisionShape(shape);
            wheelRR.CreateRigidBody();
            World.Root.Add(wheelRR);

            wheelLF.PhysicalBody.Friction = 1200;
            wheelRF.PhysicalBody.Friction = 1200;
            wheelLR.PhysicalBody.Friction = 1200;
            wheelRR.PhysicalBody.Friction = 1200;

            car.PhysicalBody.SetIgnoreCollisionCheck(wheelLF.PhysicalBody, true);
            car.PhysicalBody.SetIgnoreCollisionCheck(wheelRF.PhysicalBody, true);
            car.PhysicalBody.SetIgnoreCollisionCheck(wheelLR.PhysicalBody, true);
            car.PhysicalBody.SetIgnoreCollisionCheck(wheelRR.PhysicalBody, true);


            // location left rear x -5.27709 y 1.56474 z -3.13906
            // location right rear x -5.27709 y 1.56474 z 3.13906
            // location left front x 5.500539 y 1.56474 z -3.13906
            // location right front x 5.500539 y 1.56474 z 3.13906
            //public HingeConstraint(RigidBody rigidBodyA, RigidBody rigidBodyB, Vector3 pivotInA, Vector3 pivotInB, Vector3 axisInA, Vector3 axisInB);

            var frontAxis = new HingeConstraint(wheelLF.PhysicalBody, wheelRF.PhysicalBody,
                new Vector3(1.640539f, 0.48866f, 1.94906f), new Vector3(1.640539f, 0.48866f, -1.94906f), Vector3.UnitZ, Vector3.UnitZ);

            var rearAxis = new HingeConstraint(wheelLR.PhysicalBody, wheelRR.PhysicalBody,
                new Vector3(-1.640539f, 0.48866f, 1.94906f), new Vector3(-1.640539f, 0.48866f, -1.94906f), Vector3.UnitZ, Vector3.UnitZ);

            var centerAxis1 = new HingeConstraint(car.PhysicalBody, wheelLF.PhysicalBody,
                new Vector3(1.640539f, -0.48866f, -1.94906f), new Vector3(0), Vector3.UnitZ, Vector3.UnitZ);
            var centerAxis2 = new HingeConstraint(car.PhysicalBody, wheelRF.PhysicalBody,
                new Vector3(1.640539f, -0.48866f, 1.94906f), new Vector3(0), Vector3.UnitZ, Vector3.UnitZ);

            var centerAxis3 = new HingeConstraint(car.PhysicalBody, wheelLR.PhysicalBody, 
                new Vector3(-1.640539f, -0.48866f, 1.94906f), new Vector3(0), Vector3.UnitZ, Vector3.UnitZ);
            var centerAxis4 = new HingeConstraint(car.PhysicalBody, wheelRR.PhysicalBody,
                new Vector3(-1.640539f, -0.48866f, -1.94906f), new Vector3(0), Vector3.UnitZ, Vector3.UnitZ);

            centerAxis1.SetLimit(0, 10, 0, 0, 0);
            centerAxis2.SetLimit(0, 10, 0, 0, 0);
            centerAxis3.SetLimit(0, 10, 0, 0, 0);
            centerAxis4.SetLimit(0, 10, 0, 0, 0);

            //World.Root.PhysicalWorld.AddConstraint(frontAxis);
            // World.Root.PhysicalWorld.AddConstraint(rearAxis);
          //  centerAxis1.SetLimit(0, 0.01f);
           // centerAxis2.SetLimit(0, 0.01f);
            //centerAxis3.SetLimit(0, 0.01f);
            //centerAxis4.SetLimit(0, 0.01f);
            World.Root.PhysicalWorld.AddConstraint(centerAxis1);
            World.Root.PhysicalWorld.AddConstraint(centerAxis2);
            World.Root.PhysicalWorld.AddConstraint(centerAxis3);
            World.Root.PhysicalWorld.AddConstraint(centerAxis4);

            GLThread.OnKeyDown += (object sender, OpenTK.Input.KeyboardKeyEventArgs e) =>
            {
                if(e.Key == OpenTK.Input.Key.R)
                {
                    car.SetOrientation(Quaternion.Identity);
                }
                if(e.Key == OpenTK.Input.Key.Space)
                {
                    centerAxis1.EnableMotor = false;
                    centerAxis2.EnableMotor = false;
                }
                if(e.Key == OpenTK.Input.Key.Up)
                {
                    centerAxis1.EnableMotor = true;
                    centerAxis1.EnableAngularMotor(true, -100.0f, 10.0f);
                    centerAxis2.EnableMotor = true;
                    centerAxis2.EnableAngularMotor(true, -100.0f, 10.0f);
                }
                if(e.Key == OpenTK.Input.Key.Down)
                {

                    centerAxis1.EnableMotor = true;
                    centerAxis1.EnableAngularMotor(true, 100.0f, 6.0f);
                    centerAxis2.EnableMotor = true;
                    centerAxis2.EnableAngularMotor(true, 100.0f, 6.0f);
                }
                if(e.Key == OpenTK.Input.Key.Left)
                {
                    float angle = 0.6f / (car.PhysicalBody.LinearVelocity.Length + 1.0f);
                    centerAxis3.SetFrames(Matrix4.CreateRotationY(angle) * Matrix4.CreateTranslation(new Vector3(-1.640539f, -0.48866f, 1.94906f)), Matrix4.CreateTranslation(new Vector3(0)));
                    centerAxis4.SetFrames(Matrix4.CreateRotationY(angle) * Matrix4.CreateTranslation(new Vector3(-1.640539f, -0.48866f, -1.94906f)), Matrix4.CreateTranslation(new Vector3(0)));
                    //centerAxis3.SetAxis(new Vector3(1, 0, 1));
                    //centerAxis4.SetAxis(new Vector3(1, 0, 1));
                }
                if(e.Key == OpenTK.Input.Key.Right)
                {
                    float angle = 0.6f / (car.PhysicalBody.LinearVelocity.Length + 1.0f);
                    centerAxis3.SetFrames(Matrix4.CreateRotationY(-angle) * Matrix4.CreateTranslation(new Vector3(-1.640539f, -0.48866f, 1.94906f)), Matrix4.CreateTranslation(new Vector3(0)));
                    centerAxis4.SetFrames(Matrix4.CreateRotationY(-angle) * Matrix4.CreateTranslation(new Vector3(-1.640539f, -0.48866f, -1.94906f)), Matrix4.CreateTranslation(new Vector3(0)));
                    //centerAxis3.SetAxis(new Vector3(-1, 0, 1));
                    //centerAxis4.SetAxis(new Vector3(-1, 0, 1));
                }
            };
            GLThread.OnKeyUp += (object sender, OpenTK.Input.KeyboardKeyEventArgs e) =>
            {
                if(e.Key == OpenTK.Input.Key.Up)
                {

                    centerAxis1.EnableMotor = false;
                    centerAxis2.EnableMotor = false;
                }
                if(e.Key == OpenTK.Input.Key.Down)
                {

                    centerAxis1.EnableMotor = false;
                    centerAxis2.EnableMotor = false;
                }
                if(e.Key == OpenTK.Input.Key.Left)
                {
                    centerAxis3.SetFrames(Matrix4.CreateTranslation(new Vector3(-1.640539f, -0.48866f, 1.94906f)), Matrix4.CreateTranslation(new Vector3(0)));
                    centerAxis4.SetFrames(Matrix4.CreateTranslation(new Vector3(-1.640539f, -0.48866f, -1.94906f)), Matrix4.CreateTranslation(new Vector3(0)));
                    //centerAxis3.SetAxis(new Vector3(0, 0, 1));
                    //centerAxis4.SetAxis(new Vector3(0, 0, 1));
                }
                if(e.Key == OpenTK.Input.Key.Right)
                {
                    centerAxis3.SetFrames( Matrix4.CreateTranslation(new Vector3(-1.640539f, -0.48866f, 1.94906f)), Matrix4.CreateTranslation(new Vector3(0)));
                    centerAxis4.SetFrames(Matrix4.CreateTranslation(new Vector3(-1.640539f, -0.48866f, -1.94906f)), Matrix4.CreateTranslation(new Vector3(0)));
                    //centerAxis3.SetAxis(new Vector3(0, 0, 1));
                    //centerAxis4.SetAxis(new Vector3(0, 0, 1));
                }
            };

        }



    }
}
