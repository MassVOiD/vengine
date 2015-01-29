using OpenTK;
using BEPUphysics.Entities;


namespace VDGTech
{
    public interface IRenderable
    {
        void Draw();
    }
    public interface ILight
    {
        void Map();
        void UseTexture(int index);
        Matrix4 GetVPMatrix();
    }
    public interface IPhysical
    {
        Entity GetCollisionShape();
        Entity GetRigidBody();
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