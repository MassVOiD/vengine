using OpenTK;
using System.Collections.Generic;

namespace VDGTech
{
    public class SceneNode : IRenderable
    {
        public static SceneNode Root;
        public Quaternion Orientation = Quaternion.Identity;
        public Vector3 Position = new Vector3(0, 0, 0);
        public float Scale = 1.0f;
        private List<IRenderable> Children;
        private Matrix4 Matrix;

        public SceneNode()
        {
            Children = new List<IRenderable>();
        }

        public void Add(IRenderable renderable)
        {
            Children.Add(renderable);
        }

        public void Draw(Matrix4 translation)
        {
            foreach (var c in Children) c.Draw(translation);
        }

        public void Remove(IRenderable renderable)
        {
            Children.Remove(renderable);
        }

        public void UpdateMatrix()
        {
            Matrix = Matrix4.CreateTranslation(Position) * Matrix4.CreateFromQuaternion(Orientation) * Matrix4.CreateScale(Scale);
        }
    }
}