using OpenTK;
using VEngine.Generators;

namespace VEngine.UI
{
    public abstract class AbsUIElement
    {
        public static Object3dInfo Info3d = Object3dGenerator.CreatePlane(new Vector3(-1, -1, 0), new Vector3(1, -1, 0),
            new Vector3(-1, 1, 0), new Vector3(1, 1, 0), new Vector2(-1, -1), Vector3.UnitZ);

        public Vector2 Position;
        public Vector2 Size;

        public abstract void Draw();
    }
}