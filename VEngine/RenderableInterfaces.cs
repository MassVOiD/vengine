using OpenTK;

namespace VDGTech
{
    public interface IRenderable
    {
        void Draw();
    }
    public interface IPhysical
    {
        void Draw();
    }
    public interface IHasMass
    {
        void Draw();
    }
    public interface IControllable
    {
        void Draw();
    }
}