using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK;
namespace VDGTech.UI
{
    public class UIRenderer
    {
        public List<AbsUIElement> Elements;
        public UIRenderer()
        {
            Elements = new List<AbsUIElement>();
        }

        public void DrawAll()
        {
            GL.Disable(EnableCap.DepthTest);
            for(int i = 0; i < Elements.Count;i++ )
                Elements[i].Draw();
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
        }

        public static Vector2 PixelsToScreenSpace(Vector2 pos)
        {
            return new Vector2(pos.X / GLThread.Resolution.X, pos.Y / GLThread.Resolution.Y);
        }
    }
}