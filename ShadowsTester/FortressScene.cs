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
using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

namespace ShadowsTester
{
    public class FortressScene : Scene
    {

        public FortressScene()
        {
            var whiteboxInfo = Object3dInfo.LoadFromObjSingle(Media.Get("whiteroom.obj"));
            var whitebox = new Mesh3d(whiteboxInfo, new GenericMaterial(new Vector4(1000, 1000, 1000, 1000)));
            whitebox.Scale(300);
            whitebox.Translate(0, -2, 0);
            Add(whitebox);
            /* var sun = new Sun(new Vector3(0.1f, -1, 0).ToQuaternion(Vector3.UnitY), new Vector4(1, 0.97f, 0.92f, 120), 300, 100, 70, 40, 10, 1);
             GLThread.OnUpdate += (o, e) =>
             {
                 var kb = OpenTK.Input.Keyboard.GetState();
                 if(kb.IsKeyDown(OpenTK.Input.Key.U))
                 {
                     var quat = Quaternion.FromAxisAngle(sun.Orientation.GetTangent(MathExtensions.TangentDirection.Left), -0.01f);
                     sun.Orientation = Quaternion.Multiply(sun.Orientation, quat);
                 }
                 if(kb.IsKeyDown(OpenTK.Input.Key.J))
                 {
                     var quat = Quaternion.FromAxisAngle(sun.Orientation.GetTangent(MathExtensions.TangentDirection.Left), 0.01f);
                     sun.Orientation = Quaternion.Multiply(sun.Orientation, quat);
                 }
                 if(kb.IsKeyDown(OpenTK.Input.Key.H))
                 {
                     var quat = Quaternion.FromAxisAngle(Vector3.UnitY, -0.01f);
                     sun.Orientation = Quaternion.Multiply(sun.Orientation, quat);
                 }
                 if(kb.IsKeyDown(OpenTK.Input.Key.K))
                 {
                     var quat = Quaternion.FromAxisAngle(Vector3.UnitY, 0.01f);
                     sun.Orientation = Quaternion.Multiply(sun.Orientation, quat);
                 }
             };*/

            List<Mesh3d> nodes1 = new List<Mesh3d>();
            List<Mesh3d> leaves1 = new List<Mesh3d>();
            Random rand = new Random();
            for(int x = 0; x < 1; x++)
            {
                for(int z = 0; z < 1; z++)
                {
                    var tree = TreeGenerator.CreateTree(MathHelper.DegreesToRadians(40), MathHelper.DegreesToRadians(80), 5, 5, 6666, 0.3f, true);
                    var scale = (float)rand.NextDouble() * 2 + 1;
                    var tx = (float)rand.NextDouble() * 2 + 4;
                    var ty = (float)rand.NextDouble() * 2 + 4;
                    var mergedNodes = tree[0].Merge();
                    mergedNodes.Translate(x * 5 + tx, 0, z * 5 + ty);
                    mergedNodes.Scale(scale);
                    nodes1.Add(mergedNodes);

                    var mergedLeaves = tree[1].Merge();
                    mergedLeaves.Translate(x * 5 + tx, 0, z * 5 + ty);
                    mergedLeaves.Scale(scale);
                    leaves1.Add(mergedLeaves);

                    //tree[0].ObjectInfo.FreeCPUMemory();
                    //tree[1].ObjectInfo.FreeCPUMemory();
                }
            }
            Mesh3d singleNodes = Mesh3d.Merge(nodes1);
            Mesh3d singleLeaves = Mesh3d.Merge(leaves1);
            foreach(var o in nodes1)
                o.MainObjectInfo.FreeCPUMemory();
            foreach(var o in leaves1)
                o.MainObjectInfo.FreeCPUMemory();

            InstancedMesh3d nodes = new InstancedMesh3d(singleNodes.MainObjectInfo, singleNodes.MainMaterial);
            InstancedMesh3d leaves = new InstancedMesh3d(singleLeaves.MainObjectInfo, singleLeaves.MainMaterial);
            for(int x = 0; x < 1; x++)
            {
                for(int z = 0; z < 1; z++)
                {
                    nodes.Transformations.Add(new TransformationManager(new Vector3(x * 50, 0, z * 50)));
                    leaves.Transformations.Add(new TransformationManager(new Vector3(x * 50, 0, z * 50)));
                }
            }
            nodes.UpdateMatrix();
            leaves.UpdateMatrix();
            Add(nodes);
            Add(leaves);
            
            /*GLThread.CreateTimer(() =>
            {
                GLThread.Invoke(() => read());
                
            }, 2000).Start();*/
            Object3dInfo waterInfo = Object3dGenerator.CreateTerrain(new Vector2(-200, -200), new Vector2(200, 200), new Vector2(100, 100), Vector3.UnitY, 333, (x, y) => 0);


            var color = GenericMaterial.FromMedia("checked.png");
            //color.SetBumpMapFromMedia("lightref.png");
            Mesh3d water = new Mesh3d(waterInfo, color);
            water.SetMass(0);
            water.Translate(0, 0, 0);
            water.SetCollisionShape(new BulletSharp.StaticPlaneShape(Vector3.UnitY, 0));
            Add(water);
        }
        /*
        unsafe void read()
        {
            //var VBOFreeMemory = GL.GetInteger((GetPName)0x87FB);
            //var TextureFreeMemory = GL.GetInteger((GetPName)0x87FC);
            int[] z = new int[4] { 0, 0, 0, 0 };
            //var o = &z;
            fixed(int* FirstResult = &z[0])
            {
                glGetIntegerv(0x87FC, FirstResult);
                //var RenderBufferFreeMemory = GL.GetInteger((GetPName)0x87FD);
                //Console.WriteLine("VBO Free memory " + VBOFreeMemory);
                float mbs = ((float)(z[0])) / 1024;
                Console.WriteLine("Total memory free in the pool in MB: " + mbs);
                mbs = ((float)(z[1])) / 1024;
                Console.WriteLine("Largest available free block in the pool in MB: " + mbs);
                mbs = ((float)(z[2])) / 1024;
                Console.WriteLine("Total auxiliary memory free in MB: " + mbs);
                mbs = ((float)(z[3])) / 1024;
                Console.WriteLine("Largest auxiliary free block in MB: " + mbs);
                //Console.WriteLine("RenderBuffer Free memory " + RenderBufferFreeMemory);
            }
            
        }*/
    }
}
