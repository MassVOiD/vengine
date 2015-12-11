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
                shader.SetUniform("resolution", new Vector2(Game.Resolution.Width, Game.Resolution.Height));
                shader.SetUniform("Time", (float)(DateTime.Now - Game.StartTime).TotalMilliseconds / 1000);
            });
            Scene.Draw();
        }
    }
}