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
        
        Vector3 GetPosition();
        
        LightMixMode GetMixMode();
        MixRange GetMixRange();
        
    }

    public interface IShadowMapableLight
    {
        float GetFarPlane();

        Matrix4 GetPMatrix();
        
        Matrix4 GetVMatrix();

        void Map(Matrix4 parentTransformation);

        void UseTexture(int index);
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

    public interface ITransformable
    {
        TransformationManager GetTransformationManager();
    }
}