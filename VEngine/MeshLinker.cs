using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace VDGTech
{
    public class MeshLinker
    {
        public class LinkInfo
        {
            public Mesh3d Parent, Child;
            public Vector3 Offset;
            public Quaternion Rotation;
        }

        static List<LinkInfo> Links = new List<LinkInfo>();

        public static LinkInfo Link(Mesh3d parent, Mesh3d child, Vector3 offset, Quaternion Rotation)
        {
            var link = new LinkInfo(){
                Parent = parent,
                Child = child,
                Offset = offset,
                Rotation = Rotation
            };
            Links.Add(link);
            return link;
        }

        public static void Unlink(LinkInfo info)
        {
            Links.Remove(info);
        }

        public static void Resolve()
        {
            foreach(var link in Links)
            {
                link.Child.SetPosition(link.Parent.GetPosition() + link.Offset);
                link.Child.SetOrientation(Quaternion.Multiply(link.Parent.GetOrientation(), link.Rotation));
            }
        }
    }
}
