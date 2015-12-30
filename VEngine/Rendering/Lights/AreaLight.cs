using OpenTK;

namespace VEngine
{
    public class AreaLight : ITransformable, ILight
    {
        public Vector4 Color;
        public Vector3 Normal, Tangent;
        public TransformationManager Transformation;

        public AreaLight(Vector3 position, Vector4 color, Vector3 normal, Vector3 tangent)
        {
            Transformation = new TransformationManager(position);
            Color = color;
        }

        public Vector4 GetColor()
        {
            return Color;
        }

        public Vector3 GetPosition()
        {
            return Transformation.Position;
        }

        public TransformationManager GetTransformationManager()
        {
            return Transformation;
        }
    }
}