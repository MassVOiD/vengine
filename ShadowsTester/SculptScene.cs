using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using VEngine;
using VEngine.Generators;
using VEngine.Rendering;
using VEngine.UI;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace ShadowsTester
{
    public class SculptScene : Scene
    {
        public SculptScene()
        {

            var sun = new Sun(new Vector3(0.1f, -1, 0).ToQuaternion(Vector3.UnitY), new Vector4(1, 0.97f, 0.92f, 120), 300, 100, 70, 40, 10, 1);
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
            };
            
            var skysphere = Object3dInfo.LoadFromObjSingle(Media.Get("skyicosphere.obj"));
            var skymesh = new Mesh3d(skysphere, new GenericMaterial(Color.SkyBlue));
            skymesh.Scale(8000);
            skymesh.IgnoreLighting = true;
            Add(skymesh);
            
            var dragon3dInfo = Object3dInfo.LoadFromObjSingle(Media.Get("desertcity.obj"));
            dragon3dInfo.ScaleUV(0.1f);
            var mat = new GenericMaterial(Color.WhiteSmoke);
            var dragon = new Mesh3d(dragon3dInfo, mat);
            //mat.Type = AbsMaterial.MaterialType.WetDrops;
            dragon.Translate(0, 0, 20);
            //dragon.Scale(5);
            dragon.SetMass(0);
            dragon.SetCollisionShape(dragon3dInfo.GetAccurateCollisionShape());
            Add(dragon);
            /*
            var planeinfo = Object3dGenerator.CreateTerrain(new Vector2(-100, -100), new Vector2(100, 100), new Vector2(50, 50), Vector3.UnitY, 300, (x, y) => 0);
            var plane = new Mesh3d(planeinfo, new GenericMaterial(Color.Gainsboro));

            (plane.MainMaterial as GenericMaterial).SetBumpMapFromMedia("bumpy.jpg");
            Add(plane);*/
            
            //var text = new Text(0.0f, 0.5f, "Hello żółć 汉语 / 漢語; Hànyǔ or 中文; Zhōngwén", "Segoe UI", 24, Color.White);
            //World.Root.UI.Elements.Add(text);

            /*
            var tree = TreeGenerator.CreateTreeSingle(MathHelper.DegreesToRadians(30), MathHelper.DegreesToRadians(45), 5, 5, 6666, 0.3f, false, true);
            Mesh3d nodes = tree[0];
            foreach(var t in tree)
                Add(t);

            Random rand = new Random();

            GLThread.OnUpdate += (o, e) =>
            {
                foreach(var b in nodes.Bones)
                {
                    if(b.Name == "root" || b.Name == "rootofroot")
                        continue;
                    var orient = b.Orientation;
                    var randomQuat = Quaternion.Multiply(Quaternion.FromAxisAngle(Vector3.UnitX, (float)rand.NextDouble() - 0.5f), Quaternion.FromAxisAngle(Vector3.UnitZ, (float)rand.NextDouble() - 0.5f));
                    var neworient = Quaternion.Slerp(orient, randomQuat, 0.11f);
                    b.Orientation = neworient;
                }
            };*/
            
        }

    }
}
