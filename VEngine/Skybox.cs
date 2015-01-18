using OpenTK;
using System.Drawing;

namespace VDGTech
{
    public class Skybox
    {
        public static Skybox Current;
        private Mesh3d Mesh;

        public Skybox(IMaterial material)
        {
            if (Current == null) Current = this;
            var info = Object3dInfo.LoadFromObj(Media.Get("skybox.obj"));
            Mesh = new Mesh3d(info, material);
            Mesh.SetScale(100);
        }

        public void Use()
        {
            Current = this;
        }
        public void Draw()
        {
            Mesh.Draw();
        }
    }
}