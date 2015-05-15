using BulletSharp;
using OpenTK;

namespace VEngine
{
    public interface IControllable
    {
        void Draw();
    }

    public interface IHasMass
    {
        void GetMass();
    }

    public enum LightMixMode
    {
        Additive, Exclusive, SunCascade
    }

    public class MixRange
    {
        public float Start, End;
    }

    public interface ILight
    {
        Vector4 GetColor();

        float GetFarPlane();

        Matrix4 GetPMatrix();

        Vector3 GetPosition();

        Matrix4 GetVMatrix();
        LightMixMode GetMixMode();
        MixRange GetMixRange();

        void Map();

        void UseTexture(int index);
    }

    public interface IPhysical
    {
        CollisionShape GetCollisionShape();

        RigidBody GetRigidBody();
    }

    public interface IRenderable
    {
        void Draw();
    }

    public interface ITransformable
    {
        TransformationManager GetTransformationManager();
    }
}