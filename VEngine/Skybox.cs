using OpenTK;
using System.Drawing;
using System.Linq;

namespace VDGTech
{
    public class Skybox
    {

        static float[] vertices = {
                -1.0f, -1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, -1.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f,
                -1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f
            };
        static uint[] indices = {
                0, 1, 2, 3, 2, 1
            };

        public static Skybox Current;
        private Mesh3d Mesh;

        public Skybox(IMaterial material)
        {
            if (Current == null) Current = this;
            var info = new Object3dInfo(vertices.ToList(), indices.ToList());
            Mesh = new Mesh3d(info, material);
        }

        public void Use()
        {
            Current = this;
        }
        public void Draw()
        {
            var sp = Mesh.Material.GetShaderProgram();
            sp.Use();
            sp.SetUniform("CameraDirection", Camera.Current.GetDirection());
            Mesh.Draw();
        }
    }
}