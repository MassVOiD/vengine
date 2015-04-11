using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VDGTech
{
    public class ProjectionLight : ILight, ITransformable
    {
        public ProjectionLight(Vector3 position, Quaternion rotation, int mapwidth, int mapheight, float fov, float near, float far)
        {
            FarPlane = far;
            camera = new Camera(position, Vector3.Zero, mapwidth / mapheight, fov, near, far);
            camera.LookAt(Vector3.Zero);
            FBO = new Framebuffer(mapwidth, mapheight, true);
            Shader = ManualShaderMaterial.FromName("ConeLight");
            ViewPort = new Size(mapwidth, mapheight);
        }

        public Camera camera;
        public Framebuffer FBO;
        public Vector4 LightColor = new Vector4(1, 1, 1, 1);
        private float FarPlane;
        private ManualShaderMaterial Shader;
        private Size ViewPort;

        public float CullerMultiplier = 1.0f;
        public float Attenuation = 1.0f;

        public bool IsStatic = false;
        public bool NeedsRefreshing = true;

        public void BuildOrthographicProjection(float width, float height, float near, float far)
        {
            camera.ProjectionMatrix = Matrix4.CreateOrthographic(width, height, near, far);
        }

        public Vector4 GetColor()
        {
            return LightColor;
        }

        public float GetFarPlane()
        {
            return FarPlane;
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

        public void Map()
        {
            if(IsStatic && !NeedsRefreshing)
                return;
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
            Shader.Use();
            ShaderProgram.Lock = true;
            Shader.GetShaderProgram().SetUniform("LightPosition", camera.Transformation.GetPosition());
            Shader.GetShaderProgram().SetUniform("FarPlane", camera.Far);
            Shader.GetShaderProgram().SetUniform("LogEnchacer", 0.01f);
            World.Root.Draw();
            if(Skybox.Current != null)
                Skybox.Current.Draw();
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

        public void UseTexture(int index)
        {
            FBO.UseTexture(index);
        }
    }
}