using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class ProjectionLight : ILight, IShadowMapableLight, ITransformable
    {
        public static GenericMaterial.ShaderPack MainShaderPack = new GenericMaterial.ShaderPack();

        public Camera camera;

        public float CullerMultiplier = 1.0f;

        public Vector3 FakePosition = Vector3.Zero;

        public Framebuffer FBO;

        public bool IsStatic = false;

        public Vector3 LightColor = new Vector3(1, 1, 1);

        public bool NeedsRefreshing = true;

        public ProjectionLight(Vector3 position, Quaternion rotation, int mapwidth, int mapheight, float fov, float near, float far)
        {
            camera = new Camera(position, Vector3.Zero, Vector3.UnitY, mapwidth / mapheight, fov, near, far);
            camera.LookAt(Vector3.Zero);
            FBO = new Framebuffer(mapwidth, mapheight, true);
            FBO.DepthInternalFormat = PixelInternalFormat.DepthComponent32f;
            FBO.DepthPixelFormat = PixelFormat.DepthComponent;
            FBO.DepthPixelType = PixelType.Float;
        }

        public void BuildOrthographicProjection(float width, float height, float near, float far)
        {
            camera.SetProjectionMatrix(Matrix4.CreateOrthographic(width, height, near, far));
        }

        public void BuildOrthographicProjection(float left, float right, float bottom, float top, float near, float far)
        {
            camera.SetProjectionMatrix(Matrix4.CreateOrthographicOffCenter(left, right, bottom, top, near, far));
        }

        public Vector3 GetColor()
        {
            return LightColor;
        }

        public Matrix4 GetPMatrix()
        {
            return camera.GetProjectionMatrix();
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
            return camera.GetViewMatrix();
        }

        public void Map()
        {
            if(IsStatic && !NeedsRefreshing)
                return;

            if(camera.Transformation.HasBeenModified())
            {
                camera.Update();
                camera.Transformation.ClearModifiedFlag();
            }
            Camera last = Camera.Current;
            Camera.Current = camera;
            FBO.Use();

            MainShaderPack.ProgramsList.ForEach((shader) =>
            {
                if(!shader.Compiled)
                    return;
                shader.Use();
                ShaderProgram.Current.SetUniform("LightPosition", camera.Transformation.GetPosition());
                ShaderProgram.Current.SetUniform("LightColor", LightColor);
            });

            GenericMaterial.OverrideShaderPack = MainShaderPack;
            Game.World.Draw(false, true);
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
            camera.SetProjectionMatrix(matrix);
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