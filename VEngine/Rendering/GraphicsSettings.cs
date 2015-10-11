namespace VEngine
{
    public class GraphicsSettings
    {
        public bool UseBloom = false;

        public bool UseDeferred = false;

        public bool UseDepth = false;

        public bool UseFog = false;

        public bool UseHBAO = false;

        public bool UseLightPoints = false;

        public bool UseRSM = false;

        public bool UseSSReflections = false;

        public bool UseVDAO = false;

        public GraphicsSettings()
        {
        }

        public void SetUniforms(ShaderProgram program)
        {
        }
    }
}