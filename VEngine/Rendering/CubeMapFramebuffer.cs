namespace VEngine
{
    internal class CubeMapFramebuffer : ITransformable
    {
        public int Resolution;
        public TransformationManager Transformation;

        public TransformationManager GetTransformationManager()
        {
            return Transformation;
        }
    }
}