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
    
    public interface IPhysical
    {
        CollisionShape GetCollisionShape();

        RigidBody GetRigidBody();
    }

    public interface IRenderable
    {
        bool Draw();
    }

    public interface ITransformable
    {
        TransformationManager GetTransformationManager();
    }
}