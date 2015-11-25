using System.Drawing;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class ProjectionLight : ILight, IShadowMapableLight, ITransformable
    {
        public static GenericMaterial.ShaderPack MainShaderPack = new GenericMaterial.ShaderPack("ConeLight.fragment.glsl");
        
        public Camera camera;

        public float CullerMultiplier = 1.0f;

        public Vector3 FakePosition = Vector3.Zero;

        public Framebuffer FBO;

        public bool IsStatic = false;

        public Vector4 LightColor = new Vector4(1, 1, 1, 1);

        public LightMixMode LightMixMode = LightMixMode.Additive;

        public MixRange LightMixRange = new MixRange()
        {
            Start = 0,
            End = 100000.0f
        };

        public bool NeedsRefreshing = true;

        private float FarPlane;

        private Size ViewPort;

        public ProjectionLight(Vector3 position, Quaternion rotation, int mapwidth, int mapheight, float fov, float near, float far)
        {
            FarPlane = far;
            camera = new Camera(position, Vector3.Zero, mapwidth / mapheight, fov, near, far);
            camera.LookAt(Vector3.Zero);
            FBO = new Framebuffer(mapwidth, mapheight, true);
            FBO.DepthInternalFormat = PixelInternalFormat.DepthComponent32f;
            FBO.DepthPixelFormat = PixelFormat.DepthComponent;
            FBO.DepthPixelType = PixelType.Float;
            ViewPort = new Size(mapwidth, mapheight);
        }

        public void BuildOrthographicProjection(float width, float height, float near, float far)
        {
            camera.ProjectionMatrix = Matrix4.CreateOrthographic(width, height, near, far);
            camera.Update();
        }

        public void BuildOrthographicProjection(float left, float right, float bottom, float top, float near, float far)
        {
            camera.ProjectionMatrix = Matrix4.CreateOrthographicOffCenter(left, right, bottom, top, near, far);
            camera.Update();
        }

        public Vector4 GetColor()
        {
            return LightColor;
        }

        public float GetFarPlane()
        {
            return FarPlane;
        }

        public LightMixMode GetMixMode()
        {
            return LightMixMode;
        }

        public MixRange GetMixRange()
        {
            return LightMixRange;
        }

        public Matrix4 GetPMatrix()
        {
            return camera.ProjectionMatrix;
        }

        public Vector3 GetPosition()
        {
            return camera.Transformation.GetPosition();
        }

        public TransformationManager GetTransformationManager()
        {
            return camera.Transformation;
        }

        public Matrix4 GetVMatrix()
        {
            return camera.ViewMatrix;
        }

        public void Map(Matrix4 parentTransformation)
        {
            if(IsStatic && !NeedsRefreshing)
                return;

            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
            FBO.Use();
            if(camera.Transformation.HasBeenModified())
            {
                camera.Update();
                camera.Transformation.ClearModifiedFlag();
            }
            Camera last = Camera.Current;
            Camera.Current = camera;
            GL.Viewport(0, 0, ViewPort.Width, ViewPort.Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            MainShaderPack.ProgramsList.ForEach((shader) =>
            {
                if(!shader.Compiled)
                    return;
                shader.Use();
                ShaderProgram.Current.SetUniform("LightPosition", camera.Transformation.GetPosition());
                ShaderProgram.Current.SetUniform("LightColor", LightColor);
                ShaderProgram.Current.SetUniform("CameraTransformation", parentTransformation);
            });

            GenericMaterial.OverrideShaderPack = MainShaderPack;
            //Shader.GetShaderProgram().SetUniform("FarPlane", Camera.MainDisplayCamera.Far);
            //Shader.GetShaderProgram().SetUniform("LogEnchacer", 0.01f);
            World.Root.Draw(false, true);
            GenericMaterial.OverrideShaderPack = null;
            //if(Skybox.Current != null)
            //    Skybox.Current.Draw();
            ShaderProgram.Lock = false;
            //ParticleSystem.DrawAll(true);
            Camera.Current = last;
            NeedsRefreshing = false;
        }

        public void SetPosition(Vector3 position, Vector3 lookat)
        {
            camera.Transformation.SetPosition(position);
            camera.LookAt(lookat);
        }

        public void SetPosition(Vector3 position, Quaternion orientation)
        {
            camera.Transformation.SetPosition(position);
            camera.Transformation.SetOrientation(orientation);
            camera.Update();
        }

        public void SetProjection(Matrix4 matrix)
        {
            camera.ProjectionMatrix = matrix;
        }

        public void UpdateInverse()
        {
            camera.UpdateInverse();
        }

        public void UseTexture(int index)
        {
            FBO.UseTexture(index);
        }
    }
}