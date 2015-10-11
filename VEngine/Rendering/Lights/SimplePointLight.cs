using OpenTK;

namespace VEngine
{
    public class SimplePointLight : ITransformable, ILight
    {
        public float Attenuation = 1.0f;
        public Vector4 Color;
        public TransformationManager Transformation;

        public SimplePointLight(Vector3 position, Vector4 color)
        {
            Transformation = new TransformationManager(position);
            Color = color;
        }

        public Vector4 GetColor()
        {
            return Color;
        }

        public LightMixMode GetMixMode()
        {
            return LightMixMode.Additive;
        }

        public MixRange GetMixRange()
        {
            return new MixRange();
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