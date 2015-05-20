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
    public class RoadScene : Scene
    {
        public RoadScene()
        {

            Object3dInfo waterInfo = Object3dGenerator.CreateGround(new Vector2(-200, -200), new Vector2(200, 200), new Vector2(100, 100), Vector3.UnitY);


            var color = GenericMaterial.FromMedia("checked.png", "183_norm.JPG");
            Mesh3d water = new Mesh3d(waterInfo, color);
            water.SetMass(0);
            water.SetCollisionShape(new BulletSharp.StaticPlaneShape(Vector3.UnitY, 0));
            World.Root.Add(water);

            var lod1 = Object3dInfo.LoadFromRaw(Media.Get("lucy_lod1.vbo.raw"), Media.Get("lucy_lod1.indices.raw"));
            var mat = new GenericMaterial(new Vector4(1, 1, 1, 0.01f));
            var dragon = new Mesh3d(lod1, mat);
            dragon.Transformation.Scale(2);
            dragon.DisableDepthWrite = true;
            Add(dragon);

            var roadtile = Object3dGenerator.CreateGround(new Vector2(-0.47f, -0.5f) * 20, new Vector2(0.47f, 0.5f) * 20, new Vector2(1, 1), Vector3.UnitY);
            var roadsInstances = new InstancedMesh3d(roadtile, GenericMaterial.FromMedia("roadtex.png", "roadnormaltex.png"));
            var advancer = new Vector3(0, 0, (roadtile.GetAxisAlignedBox() * 2).Z);
            roadsInstances.Transformations.Add(new TransformationManager(new Vector3(0, 0.1f, 0)));
            for(int i = 0; i < 1600; i++)
            {
                roadsInstances.Transformations.Add(new TransformationManager(new Vector3(0, 0.1f, 0) + advancer * i));
            }
            roadsInstances.UpdateMatrix();
            Add(roadsInstances);
        }

    }
}
