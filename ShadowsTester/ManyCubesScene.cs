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
    public class ManyCubesScene : Scene
    {
        public ManyCubesScene()
        {
            var whiteboxInfo = Object3dInfo.LoadFromObjSingle(Media.Get("whiteroom.obj"));
            var whitebox = new Mesh3d(whiteboxInfo, new GenericMaterial(new Vector4(1000, 1000, 1000, 1000)));
            whitebox.Scale(3000);
            whitebox.Translate(0, -1500, 0);
            Add(whitebox);
            Object3dInfo waterInfo = Object3dGenerator.CreateTerrain(new Vector2(-200, -200), new Vector2(200, 200), new Vector2(100, 100), Vector3.UnitY, 333, (x, y) => 0);
            var color = GenericMaterial.FromMedia("bluetex.png");
            //color.SetNormalMapFromMedia("06_NORMAL.jpg");
            color.SetBumpMapFromMedia("1bump.jpg");
            color.Roughness = 1.0f;
            Mesh3d water = new Mesh3d(waterInfo, color);
            water.SetMass(0);
            // color.Type = GenericMaterial.MaterialType.Grass;
            //  color.TesselationMultiplier = 0.1f;
            // water.Translate(0, -0.941f*2.0f, 0);
            // water.SetCollisionShape(new BulletSharp.StaticPlaneShape(Vector3.UnitY, 0));
            Add(water);
            Object3dInfo wall = Object3dGenerator.CreateCube(new Vector3(10.0f, 10.0f, 1.0f), new Vector2(1, 1));
            InstancedMesh3d wallsInst = new InstancedMesh3d(wall, new GenericMaterial(Color.Red));
            wallsInst.Transformations.Add(new TransformationManager(new Vector3(0, 5, 10), Quaternion.Identity, 1));
            wallsInst.Transformations.Add(new TransformationManager(new Vector3(0, 5, -10), Quaternion.Identity, 1));
            wallsInst.Transformations.Add(new TransformationManager(new Vector3(10, 5, 0), Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.PiOver2), 1));
            wallsInst.Transformations.Add(new TransformationManager(new Vector3(-10, 5, 0), Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.PiOver2), 1));
            wallsInst.Instances = 4;
            wallsInst.UpdateMatrix();
            World.Root.CreateRigidBody(0, wallsInst.Transformations[0].GetWorldTransform(), new BulletSharp.BoxShape(wall.GetAxisAlignedBox()/2), null);
            World.Root.CreateRigidBody(0, wallsInst.Transformations[1].GetWorldTransform(), new BulletSharp.BoxShape(wall.GetAxisAlignedBox() / 2), null);
            World.Root.CreateRigidBody(0, wallsInst.Transformations[2].GetWorldTransform(), new BulletSharp.BoxShape(wall.GetAxisAlignedBox() / 2), null);
            World.Root.CreateRigidBody(0, wallsInst.Transformations[3].GetWorldTransform(), new BulletSharp.BoxShape(wall.GetAxisAlignedBox() / 2), null);
            Add(wallsInst);

            World.Root.CreateRigidBody(0, Matrix4.Identity, new BulletSharp.StaticPlaneShape(Vector3.UnitY, 0), null);
            /*
            var roadtile = Object3dGenerator.CreateGround(new Vector2(-0.47f, -0.5f) * 20, new Vector2(0.47f, 0.5f) * 20, new Vector2(1, 1), Vector3.UnitY);
            var roadsInstances = new InstancedMesh3d(roadtile, GenericMaterial.FromMedia("roadtex.png", "roadnormaltex.png"));
            var advancer = new Vector3(0, 0, (roadtile.GetAxisAlignedBox() * 2).Z);
            roadsInstances.Transformations.Add(new TransformationManager(new Vector3(0, 0.1f, 0)));
            for(int i = 0; i < 120; i++)
            {
                roadsInstances.Transformations.Add(new TransformationManager(new Vector3(0, 0.1f, 0) + advancer * i));
            }
            roadsInstances.UpdateMatrix();
            Add(roadsInstances);*/

            //Object3dInfo cube = Object3dGenerator.CreateCube(new Vector3(1, 1, 1), new Vector2(1, 1));
            var cube = Object3dInfo.LoadFromObjSingle(Media.Get("portalcube.obj"));
            cube.MakeDoubleFaced();
            GenericMaterial material = new GenericMaterial(new Vector4(1, 1, 1, 1.000f));
            int allCount = 0;
            var meshes = new List<Mesh3d>();
            Mesh3d lastmesh = null;
            var rand = new Random();
            var sphere3dInfo = Object3dInfo.LoadFromObjSingle(Media.Get("lightsphere.obj"));
            sphere3dInfo.Normalize();
            for(int y = 0; y < 100; y++)
            {
                Mesh3d mesh = new Mesh3d(cube, material);
                mesh.DisableDepthWrite = true;
                mesh.Transformation.SetPosition(new Vector3(0, (y + 10.0f) * 12.0f, 0));
                Vector3 scaleRand = new Vector3((float)rand.NextDouble() * 6.0f + 5.0f, (float)rand.NextDouble() * 6.0f + 5.0f, (float)rand.NextDouble() * 6.0f + 5.0f);
                mesh.SetMass(11.0f);
                mesh.Transformation.Scale(1);
                mesh.SetCollisionShape(new BulletSharp.BoxShape(cube.GetAxisAlignedBox() / 2));
                meshes.Add(mesh);
                World.Root.PhysicalWorld.AddCollisionObject(mesh.CreateRigidBody());
                if(lastmesh != null)
                {
                    //var offset = (mesh.Transformation.GetPosition() - lastmesh.Transformation.GetPosition()) / 2;
                    //var cst = new BulletSharp.FixedConstraint(mesh.PhysicalBody, lastmesh.PhysicalBody, (-offset).ToMatrix(), offset.ToMatrix());
                    //World.Root.PhysicalWorld.AddConstraint(cst, true);
                }

                lastmesh = mesh;
            }

            var inst = InstancedMesh3d.FromSimilarMesh3dList(meshes);
            GLThread.OnUpdate += (o, e) =>
            {
                inst.UpdateMatrix();
                //wallsInst.UpdateMatrix();
            };
            Add(inst);
            Console.WriteLine("allCount " + allCount);
        }

    }
}
