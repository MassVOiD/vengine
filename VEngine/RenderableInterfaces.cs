using OpenTK;
using BulletSharp;

namespace VDGTech
{
    public interface IRenderable
    {
        void Draw();
    }
    public interface IPhysical
    {
        CollisionShape GetCollisionShape();
        RigidBody GetRigidBody();
    }
    public interface IHasMass
    {
        void GetMass();
    }
    public interface IControllable
    {
        void Draw();
    }
}