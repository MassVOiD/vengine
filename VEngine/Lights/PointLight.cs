namespace VDGTech
{
    /*public class PointLight : ILight
    {
        private
        public PointLight(Vector3 position, int mapwidth, int mapheight, float near, float far)
        {
            FarPlane = far;
            camera = new Camera(position, Vector3.Zero, mapwidth / mapheight, fov, near, far);
            camera.LookAt(Vector3.Zero);
            FBO = new Framebuffer(mapwidth, mapheight, true);
            Shader = ManualShaderMaterial.FromName("ConeLight");
            ViewPort = new Size(mapwidth, mapheight);
        }

        public void BuildOrthographicProjection(float width, float height, float near, float far)
        {
            camera.ProjectionMatrix = Matrix4.CreateOrthographic(width, height, near, far);
        }

        public void SetProjection(Matrix4 matrix)
        {
            camera.ProjectionMatrix = matrix;
        }

        public Matrix4 GetVMatrix()
        {
            return camera.ViewMatrix;
        }
        public Matrix4 GetPMatrix()
        {
            return camera.ProjectionMatrix;
        }

        public Vector3 GetPosition()
        {
            return camera.Position;
        }
        public Vector4 GetColor()
        {
            return new Vector4(LightColor.R / 255.0f, LightColor.G / 255.0f, LightColor.B / 255.0f, LightColor.A / 255.0f);
        }
        public float GetFarPlane()
        {
            return FarPlane;
        }

        public void SetPosition(Vector3 position, Vector3 lookat)
        {
            camera.Position = position;
            camera.LookAt(lookat);
        }
        public void SetPosition(Vector3 position, Quaternion orientation)
        {
            camera.Position = position;
            camera.Orientation = orientation;
            camera.Update();
        }

        public void Map()
        {
            FBO.Use();
            Camera last = Camera.Current;
            Camera.Current = camera;
            GL.Viewport(0, 0, ViewPort.Width, ViewPort.Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Shader.Use();
            ShaderProgram.Lock = true;
            Shader.GetShaderProgram().SetUniform("LightPosition", camera.Position);
            Shader.GetShaderProgram().SetUniform("FarPlane", camera.Far);
            Shader.GetShaderProgram().SetUniform("LogEnchacer", 0.01f);
            World.Root.Draw();
            ShaderProgram.Lock = false;
            //ParticleSystem.DrawAll(true);
            Camera.Current = last;
        }

        public void UseTexture(int index)
        {
            FBO.UseTexture(index);
        }
    }
    */
}