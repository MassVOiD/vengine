using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using VDGTech;
using VDGTech.Generators;
using OpenTK;

namespace ShadowsTester
{
    public class NatureScene : Scene
    {
        public NatureScene()
        {

            var lod0 = Object3dInfo.LoadFromRaw(Media.Get("lucy_lod0.vbo.raw"), Media.Get("lucy_lod0.indices.raw"));
            var lod1 = Object3dInfo.LoadFromRaw(Media.Get("lucy_lod1.vbo.raw"), Media.Get("lucy_lod1.indices.raw"));
            var lod2 = Object3dInfo.LoadFromRaw(Media.Get("lucy_lod2.vbo.raw"), Media.Get("lucy_lod2.indices.raw"));
            var lod3 = Object3dInfo.LoadFromRaw(Media.Get("lucy_lod3.vbo.raw"), Media.Get("lucy_lod3.indices.raw"));
            var lod4 = Object3dInfo.LoadFromRaw(Media.Get("lucy_lod4.vbo.raw"), Media.Get("lucy_lod4.indices.raw"));
            var lod5 = Object3dInfo.LoadFromRaw(Media.Get("lucy_lod5.vbo.raw"), Media.Get("lucy_lod5.indices.raw"));
            var mat = new SolidColorMaterial(new Vector4(0, 0, 1, 1f));
            var dragon = new Mesh3d(lod0, mat);
            dragon.AddLodLevel(40, lod1, mat);
            dragon.AddLodLevel(60, lod2, mat);
            dragon.AddLodLevel(90, lod3, mat);
            dragon.AddLodLevel(100, lod4, mat);
            dragon.AddLodLevel(130, lod5, mat);
            //var dragon = new Mesh3d(dragon3dInfo, SingleTextureMaterial.FromMedia("180.JPG", "180_norm.JPG"));
            dragon.Transformation.Scale(2);
            //dragon.DrawOddOnly = true;
            //dragon.DiffuseComponent = 0.5f;
            //dragon.SpecularSize = 28.0f;
            Add(dragon);

            var rand = new Random();
            
            var scene1 = Object3dInfo.LoadSceneFromObj(Media.Get("tree1.obj"), Media.Get("tree1.mtl"), 12);
            List<Mesh3d> trees = new List<Mesh3d>();
            for(int i = 0; i < 50; i++)
            {
                foreach(var ob in scene1)
                {
                    var copy = new Mesh3d(ob.MainObjectInfo, ob.MainMaterial);
                    copy.Translate(rand.Next(-40, 40), 10, rand.Next(-40, 40));
                    trees.Add(copy);
                }
            }
            InstancedMesh3d.FromMesh3dList(trees).ForEach((a) =>
            {
                a.UpdateMatrix();
                Add(a);
            });
            var grasslod0 = Object3dInfo.LoadFromObjSingle(Media.Get("grasslod0.obj"));
            var grasslod1 = Object3dInfo.LoadFromObjSingle(Media.Get("grasslod1.obj"));
            var grasslod2 = Object3dInfo.LoadFromObjSingle(Media.Get("grasslod2.obj"));
            var grasslod3 = Object3dInfo.LoadFromObjSingle(Media.Get("grasslod3.obj"));
            //var grassBlock3dInfo = Object3dInfo.LoadFromRaw(Media.Get("grassblock2.vbo.raw"), Media.Get("grassblock2.indices.raw"));
            //grassBlock3dInfo.MakeDoubleFaced();
            var grasscolor = new SolidColorMaterial(Color.DarkGreen);
            var grassInstanced = new InstancedMesh3d(grasslod0, grasscolor);
            grassInstanced.AddLodLevel(70, grasslod0, grasscolor);
            grassInstanced.AddLodLevel(260, grasslod1, grasscolor);
            grassInstanced.AddLodLevel(400, grasslod2, grasscolor);
            grassInstanced.AddLodLevel(600, grasslod3, grasscolor);
            GLThread.CreateTimer(() =>
            {
                grassInstanced.RecalculateLod();
            }, 100).Start();
            for(int x = -18; x < 18; x++)
            {
                for(int y = -18; y < 18; y++)
                {
                    grassInstanced.Transformations.Add(new TransformationManager(new Vector3(x * 10, 0, y * 10), Quaternion.FromAxisAngle(Vector3.UnitY, (float)rand.NextDouble() * MathHelper.Pi), new Vector3(5, 7, 5)));
                    grassInstanced.Instances++;
                }
            }
            grassInstanced.UpdateMatrix();
            Add(grassInstanced);
            /*
            var grassBlock3dInfo2 = Object3dInfo.LoadFromRaw(Media.Get("grassblock2.vbo.raw"), Media.Get("grassblock2.indices.raw"));
            //grassBlock3dInfo2.MakeDoubleFaced();
            var grassInstanced2 = new InstancedMesh3d(grassBlock3dInfo2, new SolidColorMaterial(Color.DarkGoldenrod));
            for(int x = -20; x < 20; x++)
            {
                for(int y = -20; y < 20; y++)
                {
                    grassInstanced2.Transformations.Add(new TransformationManager(new Vector3(x * 2, 0, y * 2), Quaternion.FromAxisAngle(Vector3.UnitY, (float)rand.NextDouble() * MathHelper.Pi), 2.3f));
                    grassInstanced2.Instances++;
                }
            }
            grassInstanced2.UpdateMatrix();
            Add(grassInstanced2);*/
            /*var scene = Object3dInfo.LoadSceneFromObj(Media.Get("cryteksponza.obj"), Media.Get("cryteksponza.mtl"), 0.03f);
            //var instances = InstancedMesh3d.FromMesh3dList(testroom);
            foreach(var ob in scene)
            {
                //ob.SetMass(0);
                // ob.SetCollisionShape(ob.ObjectInfo.GetAccurateCollisionShape());
                //ob.SpecularComponent = 0.1f;
                this.Add(ob);
            }*/
            
        }

    }
}
