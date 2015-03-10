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
    public class ManyCubesScene : Scene
    {
        public ManyCubesScene()
        {

            Object3dInfo wall = Object3dGenerator.CreateCube(new Vector3(10.0f, 10.0f, 1.0f), new Vector2(1, 1));
            InstancedMesh3d wallsInst = new InstancedMesh3d(wall, new SolidColorMaterial(Color.Red));
            wallsInst.Transformations.Add(new TransformationManager(new Vector3(0, 5, 10), Quaternion.Identity, 1));
            wallsInst.Transformations.Add(new TransformationManager(new Vector3(0, 5, -10), Quaternion.Identity, 1));
            wallsInst.Transformations.Add(new TransformationManager(new Vector3(10, 5, 0), Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.PiOver2), 1));
            wallsInst.Transformations.Add(new TransformationManager(new Vector3(-10, 5, 0), Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.PiOver2), 1));
            wallsInst.Instances = 4;
            wallsInst.UpdateMatrix();
            World.Root.CreateRigidBody(0, wallsInst.Transformations[0].GetWorldTransform(), new BulletSharp.BoxShape(wall.GetAxisAlignedBox()), null);
            World.Root.CreateRigidBody(0, wallsInst.Transformations[1].GetWorldTransform(), new BulletSharp.BoxShape(wall.GetAxisAlignedBox()), null);
            World.Root.CreateRigidBody(0, wallsInst.Transformations[2].GetWorldTransform(), new BulletSharp.BoxShape(wall.GetAxisAlignedBox()), null);
            World.Root.CreateRigidBody(0, wallsInst.Transformations[3].GetWorldTransform(), new BulletSharp.BoxShape(wall.GetAxisAlignedBox()), null);
            Add(wallsInst);

            Object3dInfo cube = Object3dGenerator.CreateCube(new Vector3(1, 1, 1), new Vector2(1, 1));
            Object3dInfo icosphere = Object3dInfo.LoadFromCompressed(Media.Get("Icosphere.o3i"));
            IMaterial material = new SolidColorMaterial(Color.Cyan);
            int allCount = 0;
            var meshes = new List<Mesh3d>();
            Mesh3d lastmesh = null;
            for(int x = 0; x < 5; x++)
            {
                for(int y = 0; y < 100; y++)
                {
                    for(int z = 0; z < 5; z++)
                    {
                        Mesh3d mesh = new Mesh3d(icosphere, material);
                        mesh.Transformation.SetPosition(new Vector3(x, y + 10.0f, z ) - new Vector3(4, 0, 4));
                        mesh.SetMass(11.0f);
                        mesh.SetCollisionShape(new BulletSharp.SphereShape(icosphere.GetAxisAlignedBox().Z));
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
                }
            }
            var inst = InstancedMesh3d.FromSimilarMesh3dList(meshes);
            GLThread.OnUpdate += (o, e) => {
                inst.UpdateMatrix();
                //wallsInst.UpdateMatrix();
            };
            Add(inst);
            Console.WriteLine("allCount " + allCount);
        }

    }
}
