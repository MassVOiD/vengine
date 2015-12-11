using System.Collections.Generic;
using OpenTK;

namespace VEngine
{
    public class TransformationJoint
    {
        private static List<LinkInfo> Links = new List<LinkInfo>();

        public class LinkInfo
        {
            public Vector3 Offset;
            public ITransformable Parent, Child;
            public Quaternion Rotation;
            public bool UpdateRotation = true;
        }

        public static LinkInfo Link(ITransformable parent, ITransformable child, Vector3 offset, Quaternion Rotation)
        {
            var link = new LinkInfo()
            {
                Parent = parent,
                Child = child,
                Offset = offset,
                Rotation = Rotation
            };
            Links.Add(link);
            return link;
        }

        public static void Resolve()
        {
            foreach(var link in Links)
            {
                if(link.Child is Mesh3d)
                {
                    Mesh3d mesh = link.Child as Mesh3d;
                    link.Child.GetTransformationManager().SetPosition(link.Parent.GetTransformationManager().GetPosition() + link.Offset.Rotate(link.Parent.GetTransformationManager().GetOrientation()));
                    if(link.UpdateRotation)
                        link.Child.GetTransformationManager().SetOrientation(Quaternion.Multiply(link.Parent.GetTransformationManager().GetOrientation(), link.Rotation));
                }
                else if(link.Child is Camera)
                {
                    link.Child.GetTransformationManager().SetPosition(link.Parent.GetTransformationManager().GetPosition() + link.Offset.Rotate(link.Parent.GetTransformationManager().GetOrientation()));
                    if(link.UpdateRotation)
                        link.Child.GetTransformationManager().SetOrientation(Quaternion.Multiply(link.Parent.GetTransformationManager().GetOrientation(), link.Rotation));
                    ((Camera)link.Child).Update();
                }
                else
                {
                    link.Child.GetTransformationManager().SetPosition(link.Parent.GetTransformationManager().GetPosition() + link.Offset.Rotate(link.Parent.GetTransformationManager().GetOrientation()));
                    if(link.UpdateRotation)
                        link.Child.GetTransformationManager().SetOrientation(Quaternion.Multiply(link.Parent.GetTransformationManager().GetOrientation(), link.Rotation));
                }
            }
        }

        public static void Unlink(LinkInfo info)
        {
            Links.Remove(info);
        }
    }
}