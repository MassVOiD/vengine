using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine.UI
{
    public class UIRenderer
    {
        public List<AbsUIElement> Elements;

        public UIRenderer()
        {
            Elements = new List<AbsUIElement>();
        }

        public static Vector2 PixelsToScreenSpace(Vector2 pos)
        {
            return new Vector2(pos.X / GLThread.Resolution.Width, pos.Y / GLThread.Resolution.Height);
        }

        public void DrawAll()
        {
            GL.Disable(EnableCap.DepthTest);
            for(int i = 0; i < Elements.Count; i++)
                Elements[i].Draw();
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
        }
    }
}