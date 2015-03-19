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
            /*
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
            Add(wallsInst);*/

            Object3dInfo cube = Object3dGenerator.CreateCube(new Vector3(1, 1, 1), new Vector2(1, 1));
            IMaterial material = new SolidColorMaterial(Color.Cyan);
            int allCount = 0;
            var meshes = new List<Mesh3d>();
            Mesh3d lastmesh = null;
            var rand = new Random();
            for(int y = 0; y < 100; y++)
            {
                    Mesh3d mesh = new Mesh3d(cube, material);
                    mesh.Transformation.SetPosition(new Vector3(0, (y + 10.0f) * 12.0f, 0 ));
                    Vector3 scaleRand = new Vector3((float)rand.NextDouble() * 6.0f + 5.0f, (float)rand.NextDouble() * 6.0f + 5.0f, (float)rand.NextDouble() * 6.0f + 5.0f);
                    mesh.SetMass(11.0f);
                    mesh.Transformation.Scale(scaleRand);
                    mesh.SetCollisionShape(new BulletSharp.BoxShape(cube.GetAxisAlignedBox() * scaleRand));
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
            GLThread.OnUpdate += (o, e) => {
                inst.UpdateMatrix();
                //wallsInst.UpdateMatrix();
            };
            Add(inst);
            Console.WriteLine("allCount " + allCount);
        }

    }
}
