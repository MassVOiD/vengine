using OpenTK;

namespace VEngine
{
    public class AreaLight : ITransformable, ILight
    {
        public Vector3 Color;
        public Vector3 Normal, Tangent;
        public TransformationManager Transformation;

        public AreaLight(Vector3 position, Vector3 color, Vector3 normal, Vector3 tangent)
        {
            Transformation = new TransformationManager(position);
            Color = color;
        }

        public Vector3 GetColor()
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