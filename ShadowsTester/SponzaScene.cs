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

namespace ShadowsTester
{
    public class SponzaScene : Scene
    {
        public SponzaScene()
        {/*
            var dragon3dInfo = Object3dInfo.LoadFromRaw(Media.Get("lucy.vbo.raw"), Media.Get("lucy.indices.raw"));
            dragon3dInfo.ScaleUV(0.1f);
            var dragon = new Mesh3d(dragon3dInfo, new GenericMaterial(new Vector4(0.2f, 0, 0.2f, 1)));
            dragon.Translate(0, 10, 0);
            dragon.Scale(0.3f);
            dragon.Rotate(Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.PiOver2));
            Add(dragon);*/
            var dragon3dInfo = Object3dInfo.LoadFromObjSingle(Media.Get("ann.obj"));
            var dragon = new Mesh3d(dragon3dInfo, new GenericMaterial(new Vector4(0.7f, 0, 0.2f, 1)));
            dragon.Translate(0, 10, 0);
            dragon.Scale(0.3f);
            dragon.LoadSkeleton(Media.Get("annie_skeleton.txt"));
            dragon.Rotate(Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.PiOver2));
            Add(dragon);

            // (float)(DateTime.Now - GLThread.StartTime).TotalMilliseconds / 1000

            ArmatureAnimation animation = new ArmatureAnimation();

            ArmatureAnimation.KeyFrame f1 = new ArmatureAnimation.KeyFrame()
            {
                Duration = 1.0f,
                Orientations = new Dictionary<string, Quaternion>
                {
{ "LegStartRight", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(-33))},
{ "LegEndRight", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(33))},
{ "LegStartLeft", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(30))},
{ "LegEndLeft", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(5))},
                }
            };
            animation.Frames.Add(f1);

            ArmatureAnimation.KeyFrame f2 = new ArmatureAnimation.KeyFrame()
            {
                Duration = 1.0f,
                Orientations = new Dictionary<string, Quaternion>
                {
{ "LegStartRight", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(-14))},
{ "LegEndRight", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(5))},
{ "LegStartLeft", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(11))},
{ "LegEndLeft", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(40))},
                }
            };
            animation.Frames.Add(f2);

            ArmatureAnimation.KeyFrame f3 = new ArmatureAnimation.KeyFrame()
            {
                Duration = 1.0f,
                Orientations = new Dictionary<string, Quaternion>
                {
{ "LegStartRight", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(-15))},
{ "LegEndRight", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(15))},
{ "LegStartLeft", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(-15))},
{ "LegEndLeft", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(80))},
                }
            };
            animation.Frames.Add(f3);

            ArmatureAnimation.KeyFrame f4 = new ArmatureAnimation.KeyFrame()
            {
                Duration = 1.0f,
                Orientations = new Dictionary<string, Quaternion>
                {
{ "LegStartRight", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(15))},
{ "LegEndRight", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(5))},
{ "LegStartLeft", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(-32))},
{ "LegEndLeft", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(55))},
                }
            };
            animation.Frames.Add(f4);
            ArmatureAnimation.KeyFrame f5 = new ArmatureAnimation.KeyFrame()
            {
                Duration = 1.0f,
                Orientations = new Dictionary<string, Quaternion>
                {
{ "LegStartRight", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(28))},
{ "LegEndRight", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(5))},
{ "LegStartLeft", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(-40))},
{ "LegEndLeft", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(40))},
                }
            };
            animation.Frames.Add(f5);
            ArmatureAnimation.KeyFrame f6 = new ArmatureAnimation.KeyFrame()
            {
                Duration = 1.0f,
                Orientations = new Dictionary<string, Quaternion>
                {
{ "LegStartRight", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(17))},
{ "LegEndRight", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(17))},
{ "LegStartLeft", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(-22))},
{ "LegEndLeft", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(12))},
                }
            };
            animation.Frames.Add(f6);
            ArmatureAnimation.KeyFrame f7 = new ArmatureAnimation.KeyFrame()
            {
                Duration = 1.0f,
                Orientations = new Dictionary<string, Quaternion>
                {
{ "LegStartRight", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(10))},
{ "LegEndRight", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(70))},
{ "LegStartLeft", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(-15))},
{ "LegEndLeft", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(15))},
                }
            };
            animation.Frames.Add(f7);
            ArmatureAnimation.KeyFrame f8 = new ArmatureAnimation.KeyFrame()
            {
                Duration = 1.0f,
                Orientations = new Dictionary<string, Quaternion>
                {
{ "LegStartRight", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(19))},
{ "LegEndRight", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(62))},
{ "LegStartLeft", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(2))},
{ "LegEndLeft", Quaternion.FromAxisAngle(Vector3.UnitX, OpenTK.MathHelper.DegreesToRadians(2))},
                }
            };
            animation.Frames.Add(f8);

            foreach(var ax in animation.Frames)
            {
                var oa1 = Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(45));
                var oa2 = Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(-45));
                ax.Orientations.Add("ArmLeftStart", oa2);
                ax.Orientations.Add("ArmRightStart", oa1);
            }

            GLThread.OnUpdate += (o, e) =>
            {
                animation.Apply(dragon, (float)(DateTime.Now - GLThread.StartTime).TotalMilliseconds / 1000, 7);
            };
            /*  Object3dInfo waterInfo = Object3dGenerator.CreateGround(new Vector2(-2048, -2048), new Vector2(2048, 2048), new Vector2(496, 496), Vector3.UnitY);
              var waterMat = new SolidColorMaterial(new Vector4(0.55f, 0.74f, 0.97f, 1.0f));
              waterMat.SetNormalMapFromMedia("waternormal.png");
              var water = new Mesh3d(waterInfo, waterMat);
              water.Transformation.Translate(0, 1, 0);
              //water.DisableDepthWrite = true;
              Add(water);*/
            var scene = Object3dInfo.LoadSceneFromObj(Media.Get("sponzasimple.obj"), Media.Get("sponzasimple.mtl"), 1f);
            //var instances = InstancedMesh3d.FromMesh3dList(testroom);
            foreach(var ob in scene)
            {
                //ob.SetMass(0);
                // ob.SetCollisionShape(ob.ObjectInfo.GetAccurateCollisionShape());
                //ob.SpecularComponent = 0.1f;
                ob.Translate(0, 10, 0);
                //ob.MainObjectInfo.MakeDoubleFaced();
                this.Add(ob);
            }
        }

    }
}
