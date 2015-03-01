using OpenTK;
using BulletSharp;


namespace VDGTech
{
    public interface IRenderable
    {
        void Draw();
    }
    public interface ITransformable
    {
        TransformationManager GetTransformationManager();
    }
    public interface ILight
    {
        void Map();
        void UseTexture(int index);
        Matrix4 GetPMatrix();
        Matrix4 GetVMatrix();
        Vector3 GetPosition();
        Vector4 GetColor();
        float GetFarPlane();
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