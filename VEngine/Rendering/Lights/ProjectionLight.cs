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
            camera.SetOrientation(rotation);
            FBO = new Framebuffer();
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

            for(int i = 0; i < Game.ShaderPool.ChooseShaderDepth().ProgramsList.Length; i++)
            {
                if(!Game.ShaderPool.ChooseShaderDepth().ProgramsList[i].Compiled)
                    continue;
                Game.ShaderPool.ChooseShaderDepth().ProgramsList[i].Use();
                ShaderProgram.Current.SetUniform("LightPosition", camera.Transformation.GetPosition());
                ShaderProgram.Current.SetUniform("LightColor", LightColor);
            }


            GenericMaterial.OverrideShaderPack = Game.ShaderPool.ChooseShaderDepth();
            Game.World.SetUniforms(Game.DisplayAdapter.MainRenderer);
            InternalRenderingState.PassState = InternalRenderingState.State.ShadowMapPass;
            Game.World.Draw();
            //InternalRenderingState.PassState = InternalRenderingState.State.ForwardTransparentPass;
            //GL.Disable(EnableCap.CullFace);
            //Game.World.Draw();
            //GL.Enable(EnableCap.CullFace);
            InternalRenderingState.PassState = InternalRenderingState.State.Idle;
            GenericMaterial.OverrideShaderPack = null;
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

        public void UpdateInverse()
        {
            camera.UpdateInverse();
        }

    }
}