using OpenTK;
using System;

namespace VDGTech
{
    public class Mesh3d : IRenderable
    {
        public float Mass = 1.0f, Scale = 1.0f;
        public IMaterial Material;
        public Quaternion Orientation = Quaternion.Identity;
        public Vector3 Position = new Vector3(0, 0, 0);
        private Matrix4 Matrix;
        private Object3dInfo ObjectInfo;

        public Mesh3d(Object3dInfo objectInfo, IMaterial material)
        {
            ObjectInfo = objectInfo;
            Material = material;
        }

        public void Draw(Matrix4 translation)
        {
            if (Camera.Current == null) return;
            ShaderProgram shader = Material.GetShaderProgram();
            Material.Use();
            if (Sun.Current != null) Sun.Current.BindToShader(shader);
            shader.SetUniform("ModelMatrix", translation * Matrix);
            shader.SetUniform("ViewMatrix", Camera.Current.ViewMatrix);
            shader.SetUniform("ProjectionMatrix", Camera.Current.ProjectionMatrix);
            shader.SetUniform("CameraPosition", Camera.Current.Position);
            shader.SetUniform("Time", (float)(DateTime.Now - GLThread.StartTime).TotalMilliseconds / 1000);

            ObjectInfo.Draw();
        }

        public void UpdateMatrix()
        {
            Matrix = Matrix4.CreateFromQuaternion(Orientation) * Matrix4.CreateScale(Scale) * Matrix4.CreateTranslation(Position);
        }
    }
}