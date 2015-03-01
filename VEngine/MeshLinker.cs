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
            public ITransformable Parent, Child;
            public Vector3 Offset;
            public Quaternion Rotation;
        }

        static List<LinkInfo> Links = new List<LinkInfo>();

        public static LinkInfo Link(ITransformable parent, ITransformable child, Vector3 offset, Quaternion Rotation)
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
                link.Child.GetTransformationManager().SetPosition(link.Parent.GetTransformationManager().GetPosition() + link.Offset);
                link.Child.GetTransformationManager().SetOrientation(Quaternion.Multiply(link.Parent.GetTransformationManager().GetOrientation(), link.Rotation));
            }
        }
    }
}
