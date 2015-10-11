using OpenTK;

namespace VEngine
{
    public class VEngineInvisibleAdapter : AbsDisplayAdapter
    {
        public VEngineInvisibleAdapter()
            : base("", 1, 1, GameWindowFlags.Default)
        {
        }
    }
}