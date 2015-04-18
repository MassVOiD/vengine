using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VDGTech.Particles
{
    public class ParticleGenerator
    {
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
            Program = ShaderProgram.Compile("ParticleGenerator.vertex.glsl", "ParticleGenerator.fragment.glsl");
            DepthWriter = ShaderProgram.Compile("ParticleGenerator.vertex.glsl", "ParticleGeneratorWriteDepth.fragment.glsl");
            TimeRate = 9.0f / 1000000.0f;
            TimeToLife = 9.0f;
            MaxInstances = 1000000;
            StartTime = DateTime.Now;
            Tex = new Texture(Media.Get("smoke.png"));
        }

        private float AlphaDecrease;

        private float Bounciness;

        private Vector3 BoxSize;

        private ShaderProgram DepthWriter;

        private Vector3 Gravity;

        private Vector3 Ground;

        private Object3dInfo Info3d;

        private Vector3 InitialVelocity;

        private int MaxInstances;

        private GeneratorMode Mode;

        private Quaternion Orientation;

        private Vector2 PlaneSize;

        private Vector3 Position;

        private ShaderProgram Program;

        private float Scale;

        private DateTime StartTime;

        private Texture Tex;

        private float TimeRate;

        private float TimeToLife;

        private enum GeneratorMode
        {
            Box,
            Plane,
            Point
        };

        public static ParticleGenerator CreateBox(Vector3 position, Vector3 boxSize, Quaternion orientation, Vector3 initialVelocity, Vector3 gravity, float bounciness, float alphaDecrease, float scale)
        {
            return new ParticleGenerator(GeneratorMode.Box, position, orientation, initialVelocity, gravity, scale, bounciness, alphaDecrease, Vector2.Zero, boxSize);
        }

        public static ParticleGenerator CreatePlane(Vector3 position, Vector2 planeSize, Quaternion orientation, Vector3 initialVelocity, Vector3 gravity, float bounciness, float alphaDecrease, float scale)
        {
            return new ParticleGenerator(GeneratorMode.Plane, position, orientation, initialVelocity, gravity, scale, bounciness, alphaDecrease, planeSize, Vector3.Zero);
        }

        public static ParticleGenerator CreatePoint(Vector3 position, Quaternion orientation, Vector3 initialVelocity, Vector3 gravity, float bounciness, float alphaDecrease, float scale)
        {
            return new ParticleGenerator(GeneratorMode.Point, position, orientation, initialVelocity, gravity, scale, bounciness, alphaDecrease, Vector2.Zero, Vector3.Zero);
        }

        public void Draw(bool onlyDepth = false)
        {
            if(!onlyDepth)
            {
                Program.Use();
                Program.SetUniform("ModelMatrix", Matrix4.CreateScale(Scale) * Matrix4.CreateTranslation(Position));
                Program.SetUniform("RotationMatrix", Camera.Current.RotationMatrix);
                Program.SetUniform("ViewMatrix", Camera.Current.ViewMatrix);
                Program.SetUniform("ProjectionMatrix", Camera.Current.ProjectionMatrix);

                Program.SetUniform("CameraPosition", Camera.Current.Transformation.GetPosition());
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
                Program.SetUniform("Orientation", Matrix4.CreateFromQuaternion(Quaternion.Multiply(Camera.Current.Transformation.GetOrientation(), Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.PiOver2))));
                Program.SetUniform("OrientationOriginal", Matrix4.CreateFromQuaternion(Orientation));
                Program.SetUniform("PlaneSize", PlaneSize);
                Program.SetUniform("BoxSize", BoxSize);
                Program.SetUniform("TimeToLife", TimeToLife);
                Program.SetUniform("TimeRate", TimeRate);
                Program.SetUniform("AlphaDecrease", AlphaDecrease);
                Program.SetUniform("TimeElapsed", (float)(DateTime.Now - StartTime).TotalMilliseconds / 1000.0f);
                Tex.Use(TextureUnit.Texture0);
                //GL.Disable(EnableCap.CullFace);
                //GL.Disable(EnableCap.DepthTest);
                //GL.DepthFunc(DepthFunction.Always);
                GL.DepthMask(false);
                Info3d.DrawInstanced(MaxInstances);
                GL.DepthMask(true);
            }
            DepthWriter.Use();
            DepthWriter.SetUniform("ModelMatrix", Matrix4.CreateScale(Scale * 4.0f) * Matrix4.CreateTranslation(Position));
            DepthWriter.SetUniform("RotationMatrix", Camera.Current.RotationMatrix);
            DepthWriter.SetUniform("ViewMatrix", Camera.Current.ViewMatrix);
            DepthWriter.SetUniform("ProjectionMatrix", Camera.Current.ProjectionMatrix);

            DepthWriter.SetUniform("CameraPosition", Camera.Current.Transformation.GetPosition());
            DepthWriter.SetUniform("FarPlane", Camera.Current.Far);
            DepthWriter.SetUniform("Time", (float)(DateTime.Now - GLThread.StartTime).TotalMilliseconds / 1000);
            //DepthWriter.SetUniform("Resolution", GLThread.Resolution);
            DepthWriter.SetUniform("LogEnchacer", 0.01f);

            DepthWriter.SetUniform("GeneratorMode", (int)Mode);
            DepthWriter.SetUniform("MaxInstances", MaxInstances);
            //DepthWriter.SetUniform("InitialVelocity", InitialVelocity);
            DepthWriter.SetUniform("Gravity", Gravity);
            //DepthWriter.SetUniform("Bounciness", Bounciness);
            //DepthWriter.SetUniform("Ground", Ground);
            DepthWriter.SetUniform("Orientation", Matrix4.CreateFromQuaternion(Quaternion.Multiply(Camera.Current.Transformation.GetOrientation(), Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.PiOver2))));
            DepthWriter.SetUniform("OrientationOriginal", Matrix4.CreateFromQuaternion(Orientation));
            DepthWriter.SetUniform("PlaneSize", PlaneSize);
            DepthWriter.SetUniform("BoxSize", BoxSize);
            DepthWriter.SetUniform("TimeToLife", TimeToLife);
            DepthWriter.SetUniform("TimeRate", TimeRate);
            DepthWriter.SetUniform("AlphaDecrease", AlphaDecrease);
            DepthWriter.SetUniform("TimeElapsed", (float)(DateTime.Now - StartTime).TotalMilliseconds / 1000.0f);
            //Tex.Use(TextureUnit.Texture0);
            //GL.Disable(EnableCap.CullFace);
            //GL.Disable(EnableCap.DepthTest);
            //GL.DepthFunc(DepthFunction.Always);
            Info3d.DrawInstanced(MaxInstances);
            //
            //GL.Enable(EnableCap.CullFace);
            // GL.CullFace(CullFaceMode.Back);
        }
    }
}