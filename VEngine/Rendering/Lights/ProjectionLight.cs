using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class ProjectionLight : ILight, IShadowMapableLight, ITransformable
    {
        public Camera camera;

        public float CullerMultiplier = 1.0f;

        public float BlurFactor = 1.0f;

        public int ExclusionGroup = -1;

        public Vector3 FakePosition = Vector3.Zero;

        public Framebuffer FBO;
        public int ShadowMapArrayIndex = 0;

        public bool IsStatic = false;

        public Vector3 LightColor = new Vector3(1, 1, 1);

        public bool NeedsRefreshing = true;

        public ProjectionLight(Vector3 position, Quaternion rotation, int mapwidth, int mapheight, float fov, float near, float far)
        {
            camera = new Camera(position, Vector3.Zero, Vector3.UnitY, mapwidth / mapheight, fov, near, far);
            camera.LookAt(Vector3.Zero);
            FBO = new Framebuffer(mapwidth, mapheight, false);
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

        public float GetBlurFactor()
        {
            return BlurFactor;
        }

        public int GetExclusionGroup()
        {
            return ExclusionGroup;
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

            Game.ShaderPool.ChooseShaderDepth().ProgramsList.ForEach((shader) =>
            {
                if(!shader.Compiled)
                    return;
                shader.Use();
                ShaderProgram.Current.SetUniform("LightPosition", camera.Transformation.GetPosition());
                ShaderProgram.Current.SetUniform("LightColor", LightColor);
            });

            GenericMaterial.OverrideShaderPack = Game.ShaderPool.ChooseShaderDepth();
            InternalRenderingState.PassState = InternalRenderingState.State.ShadowMapPass;
            Game.World.SetUniforms(Game.DisplayAdapter.MainRenderer);
            Game.World.Draw();
            InternalRenderingState.PassState = InternalRenderingState.State.Idle;
            GenericMaterial.OverrideShaderPack = null;
            //if(Skybox.Current != null)
            //    Skybox.Current.Draw();
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
        
    }
}