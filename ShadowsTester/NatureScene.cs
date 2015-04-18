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

            var dragon3dInfo = Object3dInfo.LoadFromRaw(Media.Get("lucy.vbo.raw"), Media.Get("lucy.indices.raw"));
            dragon3dInfo.ScaleUV(100);
            var dragon = new Mesh3d(dragon3dInfo, SingleTextureMaterial.FromMedia("180.JPG", "180_norm.JPG"));
            dragon.Transformation.Scale(2);
            //dragon.DrawOddOnly = true;
            //dragon.DiffuseComponent = 0.5f;
            //dragon.SpecularSize = 28.0f;
            Add(dragon);

            var rand = new Random();
            
            var scene = Object3dInfo.LoadSceneFromObj(Media.Get("tree1.obj"), Media.Get("tree1.mtl"), 12);
            List<Mesh3d> trees = new List<Mesh3d>();
            for(int i = 0; i < 50; i++)
            {
                foreach(var ob in scene)
                {
                    var copy = new Mesh3d(ob.ObjectInfo, ob.Material);
                    copy.Translate(rand.Next(-40, 40), 10, rand.Next(-40, 40));
                    trees.Add(copy);
                }
            }
            InstancedMesh3d.FromMesh3dList(trees).ForEach((a) =>
            {
                a.UpdateMatrix();
                Add(a);
            });

            var grassBlock3dInfo = Object3dInfo.LoadFromRaw(Media.Get("grassblock2.vbo.raw"), Media.Get("grassblock2.indices.raw"));
            //grassBlock3dInfo.MakeDoubleFaced();
            var grassInstanced = new InstancedMesh3d(grassBlock3dInfo, new SolidColorMaterial(Color.DarkGreen));
            for(int x = -20; x < 20; x++)
            {
                for(int y = -20; y < 20; y++)
                {
                    grassInstanced.Transformations.Add(new TransformationManager(new Vector3(x * 2, 0, y * 2), Quaternion.FromAxisAngle(Vector3.UnitY, (float)rand.NextDouble() * MathHelper.Pi), new Vector3(2, 7, 2)));
                    grassInstanced.Instances++;
                }
            }
            grassInstanced.UpdateMatrix();
            Add(grassInstanced);

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
            Add(grassInstanced2);
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
