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

        public void Draw()
        {
            var sp = GenericMaterial.OverrideShaderPack != null ? GenericMaterial.OverrideShaderPack : Game.ShaderPool.GenericMaterial;
            sp.ProgramsList.ForEach((shader) =>
            {
                if(!shader.Compiled)
                    return;
                shader.Use();

                //shader.SetUniform("ViewMatrix", Camera.Current.GetViewMatrix());
                //shader.SetUniform("ProjectionMatrix", Camera.Current.GetProjectionMatrix());
                shader.SetUniform("VPMatrix", Matrix4.Mult(Camera.Current.GetViewMatrix(), Camera.Current.GetProjectionMatrix()));
                Camera.Current.SetUniforms();

                shader.SetUniform("CameraPosition", Camera.Current.Transformation.GetPosition());
                shader.SetUniform("CameraDirection", Camera.Current.Transformation.GetOrientation().ToDirection());
                shader.SetUniform("CameraTangentUp", Camera.Current.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up));
                shader.SetUniform("CameraTangentLeft", Camera.Current.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Left));
                shader.SetUniform("resolution", new Vector2(Game.Resolution.Width, Game.Resolution.Height));
                shader.SetUniform("UseVDAO", Game.GraphicsSettings.UseVDAO);
                shader.SetUniform("UseHBAO", Game.GraphicsSettings.UseHBAO);
                shader.SetUniform("VDAOGlobalMultiplier", 1.0f);
                shader.SetUniform("Time", (float)(DateTime.Now - Game.StartTime).TotalMilliseconds / 1000);
                Game.World.Scene.SetLightingUniforms(shader);
                //RandomsSSBO.Use(0);
                Game.World.Scene.MapLightsSSBOToShader(shader);
            });
            Scene.Draw();
        }
    }
}