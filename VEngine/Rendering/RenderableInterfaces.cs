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

        LightMixMode GetMixMode();

        MixRange GetMixRange();

        Vector3 GetPosition();
    }

    public interface IPhysical
    {
        CollisionShape GetCollisionShape();

        RigidBody GetRigidBody();
    }

    public interface IRenderable
    {
        void Draw(Matrix4 parentTransformation);
    }

    public interface IShadowMapableLight
    {
        float GetFarPlane();

        Matrix4 GetPMatrix();

        Matrix4 GetVMatrix();

        void Map(Matrix4 parentTransformation);

        void UseTexture(int index);
    }

    public interface ITransformable
    {
        TransformationManager GetTransformationManager();
    }
}