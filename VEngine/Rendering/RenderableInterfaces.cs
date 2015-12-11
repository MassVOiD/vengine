using BulletSharp;
using OpenTK;

namespace VEngine
{
    public class MixRange
    {
        public float Start, End;
    }

    public enum LightMixMode
    {
        Additive, Exclusive, SunCascade
    }

    public interface IControllable
    {
        void Draw();
    }

    public interface IHasMass
    {
        void GetMass();
    }

    public interface ILight
    {
        Vector4 GetColor();

        Vector3 GetPosition();
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

    public interface IShadowMapableLight
    {
        Matrix4 GetPMatrix();

        Matrix4 GetVMatrix();

        void Map();

        void UseTexture(int index);
    }

    public interface ITransformable
    {
        TransformationManager GetTransformationManager();
    }
}