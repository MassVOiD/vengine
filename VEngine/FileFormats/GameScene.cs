using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using OpenTK;
using System.IO;

namespace VEngine.FileFormats
{
    class GameScene
    {
        public List<ILight> Lights;
        public List<GenericMaterial> Materials;
        public List<Mesh3d> Meshes;
        public List<Camera> Cameras;

        public GameScene(string filekey)
        {
            string file = Media.Get(filekey);
            Load(file);
        }

        private void Load(string file)
        {
            LoadFromString(File.ReadAllLines(file));
        }

        private void LoadFromString(string[] lines)
        {

            var regx = new Regex("(.+?)[ ]+(.+)");
            var currentMaterial = new GenericMaterial(Vector4.One);
            ILight tempLight = null;
            GenericMaterial tempMaterial = null;
            Mesh3d tempMesh = null;
            Camera tempCamera = null;
            Action flush = () =>
            {
                if(tempLight != null)
                    Lights.Add(tempLight);
                if(tempMaterial != null)
                    Materials.Add(tempMaterial);
                if(tempMesh != null)
                    Meshes.Add(tempMesh);
                if(tempCamera != null)
                    Cameras.Add(tempCamera);

                tempLight = null;
                tempMaterial = null;
                tempMesh = null;
                tempCamera = null;
            };
            foreach(var l in lines)
            {
                var regout = regx.Match(l);
                if(!regout.Success)
                {
                    if(l.StartsWith("//"))
                        continue;
                    else
                        throw new Exception("Invalid line in scene string: " + l);
                }
                string command = regout.Groups[1].Value.Trim();
                string data = regout.Groups[2].Value.Trim();
                if(command.Length == 0 || data.Length == 0)
                    throw new Exception("Invalid line in scene string: " + l);

                switch(command)
                {
                    // Mesh3d start
                    case "mesh":
                    {
                        flush();
                        tempMesh = new Mesh3d();
                        tempMesh.Name = data;
                        break;
                    }
                    
                    case "usematerial":
                    {
                        flush();
                        tempMesh = new Mesh3d();
                       // tempMesh.MainMaterial = 
                        break;
                    }

                    case "translate":
                    {
                        if(tempMesh == null)
                            throw new Exception("Invalid line in scene string: " + l);
                        string[] literals = data.Split(' ');
                        if(literals.Length != 3)
                            throw new Exception("Invalid line in scene string: " + l);
                        float x, y, z;
                        if(!float.TryParse(literals[0], out x))
                            throw new Exception("Invalid line in scene string: " + l);
                        if(!float.TryParse(literals[1], out y))
                            throw new Exception("Invalid line in scene string: " + l);
                        if(!float.TryParse(literals[2], out z))
                            throw new Exception("Invalid line in scene string: " + l);
                        tempMesh.Transformation.Translate(x, y, z);
                        break;
                    }
                    case "scale":
                    {
                        if(tempMesh == null)
                            throw new Exception("Invalid line in scene string: " + l);
                        string[] literals = data.Split(' ');
                        if(literals.Length != 3)
                            throw new Exception("Invalid line in scene string: " + l);
                        float x, y, z;
                        if(!float.TryParse(literals[0], out x))
                            throw new Exception("Invalid line in scene string: " + l);
                        if(!float.TryParse(literals[1], out y))
                            throw new Exception("Invalid line in scene string: " + l);
                        if(!float.TryParse(literals[2], out z))
                            throw new Exception("Invalid line in scene string: " + l);
                        tempMesh.Transformation.Scale(x, y, z);
                        break;
                    }
                    case "rotate":
                    {
                        if(tempMesh == null)
                            throw new Exception("Invalid line in scene string: " + l);
                        string[] literals = data.Split(' ');
                        if(literals.Length < 3  || literals.Length > 4)
                            throw new Exception("Invalid line in scene string: " + l);
                        float x, y, z;
                        if(!float.TryParse(literals[0], out x))
                            throw new Exception("Invalid line in scene string: " + l);
                        if(!float.TryParse(literals[1], out y))
                            throw new Exception("Invalid line in scene string: " + l);
                        if(!float.TryParse(literals[2], out z))
                            throw new Exception("Invalid line in scene string: " + l);
                        if(literals.Length == 3)
                        {
                            var rotx = Matrix3.CreateRotationX(MathHelper.DegreesToRadians(x));
                            var roty = Matrix3.CreateRotationY(MathHelper.DegreesToRadians(y));
                            var rotz = Matrix3.CreateRotationZ(MathHelper.DegreesToRadians(z));
                            var quat = Quaternion.FromMatrix(rotx * roty * rotz);
                            tempMesh.Rotate(quat);
                        }
                        if(literals.Length == 4)
                        {
                            float w;
                            if(!float.TryParse(literals[3], out w))
                                throw new Exception("Invalid line in scene string: " + l);
                            tempMesh.Rotate(new Quaternion(x, y, z, w));
                        }
                       
                        tempMesh.Transformation.Translate(x, y, z);
                        break;
                    }

                    // Mesh3d end
                    // Material start
                    case "material":
                    {
                        flush();
                        tempMaterial = new GenericMaterial(Vector4.One);
                        break;
                    }

                    // Material end
                    default:
                    throw new Exception("Invalid line in scene string: " + l);
                }
            }

        }
    }
}
