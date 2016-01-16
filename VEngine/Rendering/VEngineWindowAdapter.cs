using OpenTK;

namespace VEngine
{
    public class VEngineWindowAdapter : AbsDisplayAdapter
    {
        public VEngineWindowAdapter(string title, int width, int height, GameWindowFlags flags)
            : base(title, width, height, flags)
        {
            MainRenderer = new Renderer(width, height);
        }
    }
}