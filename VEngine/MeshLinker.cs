using System.Collections.Generic;
using OpenTK;

namespace VEngine
{
    public class MeshLinker
    {
        private static List<LinkInfo> Links = new List<LinkInfo>();

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
                    if(mesh.PhysicalBody != null)
                    {
                        var newpos = link.Parent.GetTransformationManager().GetPosition() + link.Offset.Rotate(link.Parent.GetTransformationManager().GetOrientation());
                        var oldpos = mesh.Transformation.GetPosition();
                        mesh.PhysicalBody.LinearVelocity = (mesh.PhysicalBody.LinearVelocity * 2.0f + (newpos - oldpos) * 10.0f) / 3.0f;
                        //mesh.PhysicalBody.AngularVelocity *= 0.9f;
                        if(link.UpdateRotation)
                            mesh.PhysicalBody.WorldTransform = Matrix4.CreateFromQuaternion(Quaternion.Multiply(link.Parent.GetTransformationManager().GetOrientation(), link.Rotation))
                            * Matrix4.CreateTranslation(link.Parent.GetTransformationManager().GetPosition() + link.Offset.Rotate(link.Parent.GetTransformationManager().GetOrientation()));
                        else
                            mesh.PhysicalBody.WorldTransform = Matrix4.CreateFromQuaternion(mesh.PhysicalBody.Orientation) * Matrix4.CreateTranslation(link.Parent.GetTransformationManager().GetPosition() + link.Offset.Rotate(link.Parent.GetTransformationManager().GetOrientation()));
                    }
                    else
                    {

                        link.Child.GetTransformationManager().SetPosition(link.Parent.GetTransformationManager().GetPosition() + link.Offset.Rotate(link.Parent.GetTransformationManager().GetOrientation()));
                        if(link.UpdateRotation)
                            link.Child.GetTransformationManager().SetOrientation(Quaternion.Multiply(link.Parent.GetTransformationManager().GetOrientation(), link.Rotation));
                    }
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

        public class LinkInfo
        {
            public Vector3 Offset;
            public ITransformable Parent, Child;
            public Quaternion Rotation;
            public bool UpdateRotation = true;
        }
    }
}