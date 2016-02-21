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
        Vector3 GetColor();

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

        float GetBlurFactor();

        int GetExclusionGroup();

        void Map();
    }

    public interface ITransformable
    {
        TransformationManager GetTransformationManager();
    }
}