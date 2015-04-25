using System.Linq;

namespace VEngine
{
    public class Skybox
    {
        public Skybox(IMaterial material)
        {
            if(Current == null)
                Current = this;
            var info = new Object3dInfo(vertices.ToList(), indices.ToList());
            Mesh = new Mesh3d(info, material);
        }

        public static Skybox Current;

        private static uint[] indices = {
                0, 1, 2, 3, 2, 1
            };

        private static float[] vertices = {
                -1.0f, -1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, -1.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f,
                -1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f
            };

        private Mesh3d Mesh;

        public void Draw()
        {
            var sp = Mesh.MainMaterial.GetShaderProgram();
            sp.Use();
            sp.SetUniform("CameraDirection", Camera.Current.GetDirection());
            Mesh.Draw();
        }

        public void Use()
        {
            Current = this;
        }
    }
}