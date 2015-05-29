using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using VEngine;
using VEngine.Generators;
using OpenTK;

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
            Random rand = new Random();
            GLThread.CreateTimer(() =>
            {
                foreach(var b in dragon.Bones)
                {
                    var orient = b.Orientation;
                    orient = Quaternion.Multiply(orient, Quaternion.FromAxisAngle(
                        new Vector3((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble()), (float)rand.NextDouble() * 0.1f));
                    b.Orientation = orient;

                }
                /*var orient = dragon.Bones.First((a) => a.Name == "LegStartRight").Orientation;
                orient = Quaternion.Multiply(orient, Quaternion.FromAxisAngle(Vector3.UnitX, 0.08f));
                dragon.Bones.First((a) => a.Name == "LegStartRight").Orientation = orient;

                orient = dragon.Bones.First((a) => a.Name == "ArmLeftEnd").Orientation;
                orient = Quaternion.Multiply(orient, Quaternion.FromAxisAngle(Vector3.UnitZ, 0.08f));
                dragon.Bones.First((a) => a.Name == "ArmLeftEnd").Orientation = orient;*/
            }, 100).Start();
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
