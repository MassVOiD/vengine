using System;
using System.Collections.Generic;
using OpenTK;

namespace VEngine
{
    public class World
    {
        public Scene Scene;
        public Physics Physics;

        public World()
        {
            Scene = new Scene();
            Physics = new Physics();
        }

        public void SetUniforms(Renderer renderer)
        {
            var packs = Game.ShaderPool.GetPacks();
            for(int p=0;p< packs.Length;p++)
            {
                var pack = packs[p];
                for(int i=0;i<pack.ProgramsList.Length;i++)
                {
                    var shader = pack.ProgramsList[i];
                    if(!shader.Compiled)
                        continue;
                    shader.Use();

                    shader.SetUniform("VPMatrix", Matrix4.Mult(Camera.Current.GetViewMatrix(), Camera.Current.GetProjectionMatrix()));
                    Camera.Current.SetUniforms();
                    shader.SetUniform("Time", (float)(DateTime.Now - Game.StartTime).TotalMilliseconds / 1000);
                    shader.SetUniform("resolution", new Vector2(renderer.Width, renderer.Height));
                    shader.SetUniform("CameraPosition", Camera.Current.Transformation.GetPosition());
                    shader.SetUniform("CameraDirection", Camera.Current.Transformation.GetOrientation().ToDirection());
                    shader.SetUniform("CameraTangentUp", Camera.Current.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up));
                    shader.SetUniform("CameraTangentLeft", Camera.Current.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Left));
                }
            }
        }

        public int CurrentlyRenderedCubeMap = -1;
        public void Draw()
        {
            GenericMaterial.UseBuffer(7);
            Scene.Draw();
        }

        public void RunOcclusionQueries()
        {
            GenericMaterial.UseBuffer(7);
            Scene.RunOcclusionQueries();
        }
    }
}