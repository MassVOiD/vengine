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
            /*
            var lightSphereInfo = Object3dInfo.LoadFromObjSingle(Media.Get("lightsphere.obj"));

            var dragon3dInfo = Object3dInfo.LoadFromObjSingle(Media.Get("fleur_toskeleton.obj"));
            var dragon = new Mesh3d(dragon3dInfo, new GenericMaterial(new Vector4(0.7f, 0, 0.2f, 1)));
           // dragon.Translate(0, 10, 0);
            //dragon.Scale(0.3f);
            dragon.LoadSkeleton(Media.Get("fleurskeleton.txt"), true);
            //dragon.Rotate(Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.PiOver2));
            Add(dragon);

            var heads = new InstancedMesh3d(lightSphereInfo, new GenericMaterial(Color.Green));
            var tails = new InstancedMesh3d(lightSphereInfo, new GenericMaterial(Color.Red));

            foreach(var b in dragon.Bones)
            {
                heads.Transformations.Add(new TransformationManager(b.Head));
                tails.Transformations.Add(new TransformationManager(b.Tail));
            }
            heads.UpdateMatrix();
            tails.UpdateMatrix();
            Add(heads);
            Add(tails);

            Dictionary<string, Quaternion> restpose = new Dictionary<string, Quaternion>();
            restpose.Add("rfemur", Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(-20)));
            restpose.Add("lfemur", Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(20)));
            restpose.Add("rhumerus", Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(90)));
            restpose.Add("lhumerus", Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(-90)));

            ArmatureAnimation animation = new ArmatureAnimation();
            AMCParsedCapture captured = new AMCParsedCapture(Media.Get("walk.amc"));
            foreach(var frame in captured.Frames)
            {
                ArmatureAnimation.KeyFrame kf = new ArmatureAnimation.KeyFrame();
                kf.Duration = 1.0f;
                kf.Orientations = new Dictionary<string, Quaternion>();
                foreach(var elem in frame)
                {
                    var rot = elem.Value;
                    var nam = elem.Key;
                    if(nam.Substring(1) == "clavicle" ||
                        nam.Substring(1) == "humerus" ||
                        nam.Substring(1) == "radius" ||
                        nam.Substring(1) == "wrist" ||
                        nam.Substring(1) == "hand" ||
                        nam.Substring(1) == "fingers" ||
                        nam.Substring(1) == "thumb")
                    {
                        if(nam.StartsWith("r") && nam != "root")
                            nam = "l" + nam.Substring(1);
                        else if(nam.StartsWith("l") && nam != "lowerback" && nam != "lowerneck")
                            nam = "r" + nam.Substring(1);
                    }
                    kf.Orientations.Add(nam, rot);

                }
                animation.Frames.Add(kf);
            }
            


            GLThread.OnUpdate += (o, e) =>
            {
                animation.Apply(dragon, (float)(DateTime.Now - GLThread.StartTime).TotalMilliseconds / 1000, 87);
            };*/
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
                //ob.Translate(0, 10, 0);
                //ob.MainObjectInfo.MakeDoubleFaced();
                this.Add(ob);
            }/*
            var meshEmit = Object3dInfo.LoadFromObjSingle(Media.Get("emitplane.obj"));
            var verts = meshEmit.GetOrderedVertices();
            foreach(var v in verts)
            {
                var light = new SimplePointLight(v, Color.White);
                LightPool.Add(light);
            }
            var emitter = new Mesh3d(meshEmit, new GenericMaterial(Color.White));
            Add(emitter);*/
        }

    }
}
