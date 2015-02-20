using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VDGTech.Particles
{
    public class ParticleGenerator
    {
        enum GeneratorMode
        {
            Box,
            Plane,
            Point
        };
        GeneratorMode Mode;
        Vector3 InitialVelocity;
        Vector3 Gravity;
        float Bounciness;
        Vector3 Position;
        Vector3 Ground;
        Quaternion Orientation;
        Vector2 PlaneSize;
        Vector3 BoxSize;
        float AlphaDecrease;
        float Scale;

        Object3dInfo Info3d;
        ShaderProgram Program;

        float TimeRate;
        int MaxInstances;
        float TimeToLife;
        DateTime StartTime;
        Texture Tex;

        private ParticleGenerator(GeneratorMode mode, Vector3 position, Quaternion orientation, Vector3 initialVelocity, Vector3 gravity, float scale, float bounciness, float alphaDecrease, Vector2 planeSize, Vector3 boxSize)
        {
            Mode = mode;
            Position = position;
            Orientation = orientation;
            InitialVelocity = initialVelocity;
            Gravity = gravity;
            Bounciness = bounciness;
            PlaneSize = planeSize;
            BoxSize = boxSize;
            Scale = scale;
            AlphaDecrease = alphaDecrease;
            Info3d = Generators.Object3dGenerator.CreateGround(new Vector2(-1, -1), new Vector2(1, 1), new Vector2(1), Vector3.UnitY);
            Program = new ShaderProgram(Media.ReadAllText("ParticleGenerator.vertex.glsl"), Media.ReadAllText("ParticleGenerator.fragment.glsl"));
            TimeRate = 9.0f / 1000.0f;
            TimeToLife = 9.0f;
            MaxInstances = 1000;
            StartTime = DateTime.Now;
            Tex = new Texture(Media.Get("smoke.png"));
        }

        public void Draw()
        {
            Program.Use();
            Program.SetUniform("ModelMatrix", Matrix4.CreateScale(Scale) * Matrix4.CreateTranslation(Position));
            Program.SetUniform("RotationMatrix", Camera.Current.RotationMatrix);
            Program.SetUniform("ViewMatrix", Camera.Current.ViewMatrix);
            Program.SetUniform("ProjectionMatrix", Camera.Current.ProjectionMatrix);

            Program.SetUniform("CameraPosition", Camera.Current.Position);
            Program.SetUniform("FarPlane", Camera.Current.Far);
            Program.SetUniform("Time", (float)(DateTime.Now - GLThread.StartTime).TotalMilliseconds / 1000);
            Program.SetUniform("Resolution", GLThread.Resolution);
            Program.SetUniform("LogEnchacer", 0.01f);

            Program.SetUniform("GeneratorMode", (int)Mode);
            Program.SetUniform("MaxInstances", MaxInstances);
            Program.SetUniform("InitialVelocity", InitialVelocity);
            Program.SetUniform("Gravity", Gravity);
            Program.SetUniform("Bounciness", Bounciness);
            Program.SetUniform("Ground", Ground);
            Program.SetUniform("Orientation", Matrix4.CreateFromQuaternion(Quaternion.Multiply(Camera.Current.Orientation, Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.PiOver2))));
            Program.SetUniform("OrientationOriginal", Matrix4.CreateFromQuaternion(Orientation));
            Program.SetUniform("PlaneSize", PlaneSize);
            Program.SetUniform("BoxSize", BoxSize);
            Program.SetUniform("TimeToLife", TimeToLife);
            Program.SetUniform("TimeRate", TimeRate);
            Program.SetUniform("AlphaDecrease", AlphaDecrease);
            Program.SetUniform("TimeElapsed", (float)(DateTime.Now - StartTime).TotalMilliseconds / 1000.0f);
            Tex.Use(TextureUnit.Texture0);
            GL.Disable(EnableCap.CullFace);
            //GL.Disable(EnableCap.DepthTest);
            //GL.DepthFunc(DepthFunction.Always);
            GL.DepthMask(false);
            Info3d.DrawInstanced(MaxInstances);
            GL.DepthMask(true);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
           // GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

        }

        public static ParticleGenerator CreatePoint(Vector3 position, Quaternion orientation, Vector3 initialVelocity, Vector3 gravity, float bounciness, float alphaDecrease, float scale)
        {
            return new ParticleGenerator(GeneratorMode.Point, position, orientation, initialVelocity, gravity, scale, bounciness, alphaDecrease, Vector2.Zero, Vector3.Zero);
        }
        public static ParticleGenerator CreateBox(Vector3 position, Vector3 boxSize, Quaternion orientation, Vector3 initialVelocity, Vector3 gravity, float bounciness, float alphaDecrease, float scale)
        {
            return new ParticleGenerator(GeneratorMode.Box, position, orientation, initialVelocity, gravity, scale, bounciness, alphaDecrease, Vector2.Zero, boxSize);
        }
        public static ParticleGenerator CreatePlane(Vector3 position, Vector2 planeSize, Quaternion orientation, Vector3 initialVelocity, Vector3 gravity, float bounciness, float alphaDecrease, float scale)
        {
            return new ParticleGenerator(GeneratorMode.Plane, position, orientation, initialVelocity, gravity, scale, bounciness, alphaDecrease, planeSize, Vector3.Zero);
        }

    }
}
