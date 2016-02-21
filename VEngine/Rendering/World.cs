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
            foreach(var s in Game.ShaderPool.GetPacks())
            {
                s.ProgramsList.ForEach((shader) =>
                {
                    if(!shader.Compiled)
                        return;
                    shader.Use();

                    shader.SetUniform("VPMatrix", Matrix4.Mult(Camera.Current.GetViewMatrix(), Camera.Current.GetProjectionMatrix()));
                    Camera.Current.SetUniforms();


                    shader.SetUniform("UseVDAO", Game.GraphicsSettings.UseVDAO);
                    shader.SetUniform("UseHBAO", Game.GraphicsSettings.UseHBAO);
                    shader.SetUniform("UseFog", Game.GraphicsSettings.UseFog);
                    shader.SetUniform("Brightness", Camera.MainDisplayCamera.Brightness);
                    shader.SetUniform("VDAOGlobalMultiplier", 1.0f);
                    shader.SetUniform("DisablePostEffects", Renderer.DisablePostEffects);
                    shader.SetUniform("Time", (float)(DateTime.Now - Game.StartTime).TotalMilliseconds / 1000);
                    shader.SetUniform("resolution", new Vector2(renderer.Width, renderer.Height));
                    shader.SetUniform("CameraPosition", Camera.Current.Transformation.GetPosition());
                    shader.SetUniform("CameraDirection", Camera.Current.Transformation.GetOrientation().ToDirection());
                    shader.SetUniform("CameraTangentUp", Camera.Current.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up));
                    shader.SetUniform("CameraTangentLeft", Camera.Current.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Left));
                    shader.SetUniform("CurrentlyRenderedCubeMap", CurrentlyRenderedCubeMap);
                    Game.World.Scene.SetLightingUniforms(shader);
                    //RandomsSSBO.Use(0);
                    renderer.SetCubemapsUniforms();
                    Game.World.Scene.MapLightsSSBOToShader(shader);
               //     Game.DisplayAdapter.MainRenderer.VXGI.UseVoxelsBuffer(3, 4);
                });
            }
        }

        public int CurrentlyRenderedCubeMap = -1;
        public void Draw()
        {
            Scene.Draw();
        }
    }
}