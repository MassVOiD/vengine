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
using VDGTech.Particles;

namespace VDGTech
{
    public class ProjectionLight : ILight
    {
        public Camera camera;
        public Framebuffer FBO;
        ManualShaderMaterial Shader;
        float FarPlane;
        Size ViewPort;
        public ProjectionLight(Vector3 position, Quaternion rotation, int mapwidth, int mapheight, float fov, float near, float far)
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
}
