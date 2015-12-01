using System;
using System.Collections.Generic;
using OpenTK;

namespace VEngine
{
    public class World
    {
        public static World Root;

        public volatile bool Disposed;

        public Scene RootScene;

        public World()
        {
            RootScene = new Scene();
            if(Root == null)
                Root = this;
        }

        public delegate void MeshCollideDelegate(Mesh3d meshA, Mesh3d meshB, Vector3 collisionPoint, Vector3 normalA);

        public event MeshCollideDelegate MeshCollide;

        public void Draw(bool ignoreMeshWithDisabledDepthTest = false, bool ignoreDisableDepthWriteFlag = false)
        {
            var sp = GenericMaterial.OverrideShaderPack != null ? GenericMaterial.OverrideShaderPack : GenericMaterial.MainShaderPack;
            sp.ProgramsList.ForEach((shader) =>
            {
                if(!shader.Compiled)
                    return;
                shader.Use();

                shader.SetUniform("ViewMatrix", Camera.Current.GetViewMatrix());
                shader.SetUniform("ProjectionMatrix", Camera.Current.GetProjectionMatrix());
                Camera.Current.SetUniforms();

                shader.SetUniform("CameraPosition", Camera.Current.Transformation.GetPosition());
                shader.SetUniform("CameraDirection", Camera.Current.Transformation.GetOrientation().ToDirection());
                shader.SetUniform("CameraTangentUp", Camera.Current.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up));
                shader.SetUniform("CameraTangentLeft", Camera.Current.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Left));
                shader.SetUniform("FarPlane", Camera.Current.Far);
                shader.SetUniform("resolution", new Vector2(GLThread.Resolution.Width, GLThread.Resolution.Height));
                shader.SetUniform("Time", (float)(DateTime.Now - GLThread.StartTime).TotalMilliseconds / 1000);
            });
            RootScene.Draw(Matrix4.Identity);
        }
    }
}