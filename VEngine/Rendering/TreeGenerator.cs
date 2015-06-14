using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace VEngine.Rendering
{
    public class TreeGenerator
    {

        static Random Randomizer;

        static Object3dInfo Node = Object3dInfo.LoadFromObjSingle(Media.Get("tree_singlenode.obj"));
        static Object3dInfo Leaf = Object3dInfo.LoadFromObjSingle(Media.Get("tree_singleleaf.obj"));
        static GenericMaterial NodeMaterial = new GenericMaterial(Color.Brown);
        static GenericMaterial LeafMaterial = new GenericMaterial(Color.Green);

        private static float Rand()
        {
            return (float)Randomizer.NextDouble();
        }

        public static List<InstancedMesh3d> CreateTree(float minNodesAngle, float maxNodesAngle, int maxNodesCountPerLevel, int iterations, int randomSeed, float scaleDescendant, bool addLeaves)
        {
            List<InstancedMesh3d> elements = new List<InstancedMesh3d>();

            Randomizer = new Random(DateTime.Now.Millisecond);

            var tree = CreateNode(Vector3.Zero, Quaternion.Identity, minNodesAngle, maxNodesAngle, maxNodesCountPerLevel, 1.0f, iterations, scaleDescendant, addLeaves);
            elements.Add(InstancedMesh3d.FromSimilarMesh3dList(tree.Nodes));
            if(addLeaves) elements.Add(InstancedMesh3d.FromSimilarMesh3dList(tree.Leafs));

            return elements;
        }

        public static List<Mesh3d> CreateTreeSingle(float minNodesAngle, float maxNodesAngle, int maxNodesCountPerLevel, int iterations, int randomSeed, float scaleDescendant, bool addLeaves, bool addBones)
        {
            List<Mesh3d> elements = new List<Mesh3d>();

            Randomizer = new Random(DateTime.Now.Millisecond);

            var bones = new List<VEngine.Mesh3d.Bone>();

            var currentBone = new Mesh3d.Bone()
            {
                Head = Vector3.Zero,
                Tail = Vector3.Zero,
                Name = "rootofroot",
                Orientation = Quaternion.Identity,
                Parent = null,
                ParentName = ""
            };
            bones.Add(currentBone);

            var tree = CreateNode(Vector3.Zero, Quaternion.Identity, minNodesAngle, maxNodesAngle, maxNodesCountPerLevel, 1.0f, iterations, scaleDescendant, addLeaves, bones, currentBone);
            var nodesMesh = InstancedMesh3d.FromSimilarMesh3dList(tree.Nodes).Merge();
            nodesMesh.Bones = bones;
            elements.Add(nodesMesh);
            if(addLeaves)
                elements.Add(InstancedMesh3d.FromSimilarMesh3dList(tree.Leafs).Merge());

            return elements;
        }

        private static float Mix(float a, float b, float f)
        {
            return a - a * f + b * f;
        }

        class NodeOut
        {
            public List<Mesh3d> Nodes, Leafs;
        }
        private static NodeOut CreateNode(Vector3 origin, Quaternion rotation, float minNodesAngle, float maxNodesAngle, int maxNodesCountPerLevel, float scale, int iterations, float scaleDescendant, bool addLeaves, List<VEngine.Mesh3d.Bone> boneSystem = null, VEngine.Mesh3d.Bone boneParent = null)
        {
            List<Mesh3d> elements = new List<Mesh3d>();
            List<Mesh3d> leafs = new List<Mesh3d>();
            float nodeModelHeight = Node.GetAxisAlignedBox().Y * scale;
            var currentBone = new Mesh3d.Bone();
            if(boneSystem != null)
            {
                currentBone = new Mesh3d.Bone()
                {
                    Head = origin,
                    Tail = origin + Vector3.Transform(Vector3.UnitY * nodeModelHeight, rotation),
                    Name = boneParent.Name == "rootofroot" ? "root" : "bone" + boneSystem.Count.ToString(),
                    Orientation = Quaternion.Identity,
                    Parent = boneParent,
                    ParentName = boneParent.Name
                };
            }
            if(iterations > 1)
            {
                var root = new Mesh3d(Node, NodeMaterial);
                root.Translate(origin);
                root.Scale(scale);
                root.Rotate(rotation);
                elements.Add(root);
                if(boneSystem != null)
                {
                    boneSystem.Add(currentBone);
                }
            }

            Vector3 direction = rotation.ToDirection(Vector3.UnitY);
            Vector3 tgLeft = rotation.ToDirection(Vector3.UnitZ);
            if(iterations > 0)
            {
                for(int i = 0; i < maxNodesCountPerLevel; i++)
                {
                    var randomOrigin = origin + (direction * nodeModelHeight * (Rand() * 0.5f + 0.5f));

                    float sign1 = Rand() > 0.5f ? -1 : 1;
                    float angle = Mix(minNodesAngle, maxNodesAngle, Rand()) * sign1;
                    var rotLeft = Quaternion.FromAxisAngle(tgLeft, angle);
                    var rotForward = Quaternion.FromAxisAngle(direction, MathHelper.TwoPi * Rand());

                    var randomRotation = Quaternion.Multiply(rotForward, Quaternion.Multiply(rotLeft, rotation));
                    var single = CreateNode(randomOrigin, randomRotation, minNodesAngle, maxNodesAngle, maxNodesCountPerLevel, scale * (Rand() * 0.2f + scaleDescendant), iterations - 1, scaleDescendant, addLeaves, boneSystem, currentBone);
                    elements.AddRange(single.Nodes);
                    leafs.AddRange(single.Leafs);
                }
            }
            if(iterations < 2 && addLeaves)
            {
                for(int i = 0; i < 12; i++)
                {
                    var randomOrigin = origin + (direction * nodeModelHeight * (Rand()));
                    randomOrigin += (new Vector3(Rand(), Rand(), Rand()) - new Vector3(0.5f)) * 1.0f;

                    float sign1 = Rand() > 0.5f ? -1 : 1;
                    float angle = Mix(minNodesAngle, maxNodesAngle, Rand()) * sign1;
                    var rotLeft = Quaternion.FromAxisAngle(tgLeft, angle);
                    var rotForward = Quaternion.FromAxisAngle(direction, MathHelper.TwoPi * Rand());

                    var randomRotation = Quaternion.Multiply(rotForward, Quaternion.Multiply(rotLeft, rotation));
                    var leaf = new Mesh3d(Leaf, LeafMaterial);
                    leaf.Translate(randomOrigin);
                    leaf.Scale(Rand() * 1.5f + 0.4f);
                    leaf.Rotate(randomRotation);
                    leafs.Add(leaf);
                }
            }

            return new NodeOut()
            {
                Nodes = elements,
                Leafs = leafs
            };
        }

    }
}
