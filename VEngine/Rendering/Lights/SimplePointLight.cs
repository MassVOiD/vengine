using OpenTK;

namespace VEngine
{
    public class SimplePointLight : ITransformable, ILight
    {
        public Vector3 Color;
        public TransformationManager Transformation;
        public float Angle = MathHelper.Pi * 2.0f;

        public SimplePointLight(Vector3 position, Vector3 color)
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
            return Transformation.GetPosition();
        }

        public TransformationManager GetTransformationManager()
        {
            return Transformation;
        }
    }
}