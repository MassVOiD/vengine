using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using VDGTech;
using System.Drawing;

namespace VDGTech
{
    public class FOVLight : ILight
    {
        public Camera camera;
        public Framebuffer FBO;
        ManualShaderMaterial Shader;
        Size ViewPort;
        public FOVLight(Vector3 position, Quaternion rotation, int mapwidth, int mapheight, float fov, float near, float far)
        {
            camera = new Camera(position, Vector3.Zero, mapwidth / mapheight, fov, near, far);
            camera.LookAt(Vector3.Zero);
            FBO = new Framebuffer(mapwidth, mapheight);
            Shader = ManualShaderMaterial.FromName("ConeLight");
            ViewPort = new Size(mapwidth, mapheight);
        }

        public Matrix4 GetVMatrix()
        {
            return camera.ViewMatrix;
        }
        public Matrix4 GetPMatrix()
        {
            return camera.ProjectionMatrix;
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
            World.Root.Draw();
            ShaderProgram.Lock = false;
            Camera.Current = last;
        }

        public void UseTexture(int index)
        {
            FBO.UseTexture(index);
        }

    }
}
