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
            List<Mesh3d> nodes1 = new List<Mesh3d>();
            List<Mesh3d> leaves1 = new List<Mesh3d>();
            for(int x = 0; x < 10; x++)
            {
                for(int z = 0; z < 10; z++)
                {
                    var tree = TreeGenerator.CreateTree(MathHelper.DegreesToRadians(30), MathHelper.DegreesToRadians(45), 4, 4, 6666, 0.3f, true);

                    var mergedNodes = tree[0].Merge();
                    mergedNodes.Translate(x * 5, 0, z * 5);
                    nodes1.Add(mergedNodes);

                    var mergedLeaves = tree[1].Merge();
                    mergedLeaves.Translate(x * 5, 0, z * 5);
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
            for(int x = 0; x < 5; x++)
            {
                for(int z = 0; z < 5; z++)
                {
                    nodes.Transformations.Add(new TransformationManager(new Vector3(x * 100, 0, z * 100)));
                    leaves.Transformations.Add(new TransformationManager(new Vector3(x * 100, 0, z * 100)));
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
