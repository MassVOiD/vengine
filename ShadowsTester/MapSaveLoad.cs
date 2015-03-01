using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDGTech;

namespace ShadowsTester
{
    public class MapSaveLoad
    {
        public static void SaveArray(string file, Mesh3d[] meshes)
        {
            List<byte> buffer = new List<byte>();
            foreach(var mesh in meshes)
            {
                buffer.AddRange(BitConverter.GetBytes(mesh.Transformation.GetPosition().X));
                buffer.AddRange(BitConverter.GetBytes(mesh.Transformation.GetPosition().Y));
                buffer.AddRange(BitConverter.GetBytes(mesh.Transformation.GetPosition().Z));

                buffer.AddRange(BitConverter.GetBytes(mesh.Transformation.GetOrientation().X));
                buffer.AddRange(BitConverter.GetBytes(mesh.Transformation.GetOrientation().Y));
                buffer.AddRange(BitConverter.GetBytes(mesh.Transformation.GetOrientation().Z));
                buffer.AddRange(BitConverter.GetBytes(mesh.Transformation.GetOrientation().W));
            }
            System.IO.File.WriteAllBytes(file, buffer.ToArray());
        }

        public class LoadedMeshInfo
        {
            public OpenTK.Vector3 Position;
            public OpenTK.Quaternion Orientation;
        }

        public static List<LoadedMeshInfo> LoadMeshes(string file)
        {
            byte[] content = System.IO.File.ReadAllBytes(file);
            int i = 0;
            List<LoadedMeshInfo> loaded = new List<LoadedMeshInfo>();
             
            while(i < content.Length)
            {
                float vx = BitConverter.ToSingle(content, i);
                i += 4;
                float vy = BitConverter.ToSingle(content, i);
                i += 4;
                float vz = BitConverter.ToSingle(content, i);
                i += 4;


                float qx = BitConverter.ToSingle(content, i);
                i += 4;
                float qy = BitConverter.ToSingle(content, i);
                i += 4;
                float qz = BitConverter.ToSingle(content, i);
                i += 4;
                float qw = BitConverter.ToSingle(content, i);
                i += 4;

                loaded.Add(new LoadedMeshInfo()
                {
                    Position = new OpenTK.Vector3(vx, vy, vz),
                    Orientation = new OpenTK.Quaternion(qx, qy, qz, qw)
                });
            }
            return loaded;
        }
    }
}
