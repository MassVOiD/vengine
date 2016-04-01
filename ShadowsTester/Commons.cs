using System;
using System.Collections.Generic;
using OpenTK;
using System.Linq;
using OpenTK.Graphics.OpenGL4;
using VEngine;

namespace ShadowsTester
{
    internal class Commons
    {
        public static FreeCamera FreeCam;
        public static int MouseX, MouseY;

        private static Dictionary<int, TransformationManager> CameraSavedViews;

        private static ComputeShader MousePicker;

        public static ITransformable Picked;

        public static Mesh3d PickedMesh;

        private static ShaderStorageBuffer PickingResult;

        private static List<Light> RedLight;

        private static Random rand = new Random();

        public static List<Light> AddControllableLight()
        {
            float fovdegree = 90;
            RedLight = new List<Light>();
            RedLight.Add(new Light(new TransformationManager(new Vector3(0, 5, 0), Quaternion.FromAxisAngle(new Vector3(1, 0, 0), MathHelper.DegreesToRadians(-110))))
            {
                Color = new Vector3(1, 0.84f, 0.93f) * 400,
                ShadowMappingEnabled = true,
                ShadowMapType = Light.ShadowMapTypeEnum.Single
            });
            Picked = RedLight[0];
            /*
            //crazyiness
            var tmp = new List<Light>();
            for(int i = 0; i < 10; i++)
            {
                for(int g = 0; g < 4; g++)
                {
                    var l = new Light(new TransformationManager(new Vector3((i - 5) * 1.6f, 1 + g * 2, 0)));
                    tmp.Add(l);
                    //l.Color = (new Vector3((float)Math.Sin(i), 1.0f - (float)Math.Sin(i), (float)Math.Cos(i)) * 0.5f + new Vector3(0.5f)) * 0.3f;
                    l.Color = new Vector3(1) * (0.05f * (g + 1));
                    l.CutOffDistance = 30.0f;
                    l.StaticShadowMap = true;
                    l.ShadowMapType = Light.ShadowMapTypeEnum.Cubemap;
                    l.ShadowsQuality = ShadowMapQuality.Low;
                    l.ShadowMappingEnabled = true;
                    Game.World.Scene.Add(l);
                }
            }
            int ix = 0;
            Game.OnBeforeDraw += (x, d) =>
            {
                tmp[ix].ShadowMapRefreshNeeded = true;
                ix++;
                if(ix >= tmp.Count)
                    ix = 0;
            };*/

            //RedLight[0].camera.UpdatePerspective(1, MathHelper.DegreesToRadians(90), 0.01f, 100);
            /* RedLight.Add(new ProjectionLight(new Vector3(65, 0, 65), Quaternion.FromAxisAngle(new Vector3(1, 0, -1), MathHelper.DegreesToRadians(fovdegree)), 1, 1, MathHelper.DegreesToRadians(45), 0.1f, 100.0f)
             {
                 LightColor = new Vector3(1, 0.84f, 0.93f) * 25,
                 IsStatic = true
             });
             RedLight.Add(new ProjectionLight(new Vector3(65, 0, 65), Quaternion.FromAxisAngle(new Vector3(1, 0, -1), MathHelper.DegreesToRadians(fovdegree)), 1, 1, MathHelper.DegreesToRadians(45), 0.1f, 100.0f)
             {
                 LightColor = new Vector3(1, 0.84f, 0.93f) * 25,
                 IsStatic = true
             });
             RedLight.Add(new ProjectionLight(new Vector3(65, 0, 65), Quaternion.FromAxisAngle(new Vector3(1, 0, -1), MathHelper.DegreesToRadians(fovdegree)), 1, 1, MathHelper.DegreesToRadians(45), 0.1f, 100.0f)
             {
                 LightColor = new Vector3(1, 0.84f, 0.93f) * 25,
                 IsStatic = true
             });*/
            //redConeLight.BuildOrthographicProjection(600, 600, -150, 150);

            Game.OnKeyUp += (o, e) =>
            {
                if(e.Key == OpenTK.Input.Key.J)
                {
                    fovdegree += 5f;
                    if(fovdegree >= 180)
                        fovdegree = 179;
                    RedLight.ForEach((ax) => ax.Angle = MathHelper.DegreesToRadians(fovdegree));
                }
                if(e.Key == OpenTK.Input.Key.K)
                {
                    fovdegree -= 5f;
                    if(fovdegree <= 10)
                        fovdegree = 10;
                    RedLight.ForEach((ax) => ax.Angle = MathHelper.DegreesToRadians(fovdegree));
                }
            };
            RedLight.ForEach((a) => Game.World.Scene.Add(a));

            Game.OnMouseMove += (o, e) =>
            {
                MouseX = e.X;
                MouseY = e.Y;

                var kb = OpenTK.Input.Keyboard.GetState();
                if(kb.IsKeyDown(OpenTK.Input.Key.T))
                {
                    FreeCam.Freeze = true;
                    if(Picked != null)
                    {
                        Picked.SetPosition(Picked.GetPosition() + FreeCam.Cam.GetOrientation().GetTangent(MathExtensions.TangentDirection.Right) * (float)e.XDelta * -0.01f);
                        Picked.SetPosition(Picked.GetPosition() + FreeCam.Cam.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up) * (float)e.YDelta * -0.01f);
                    }
                }
                else if(kb.IsKeyDown(OpenTK.Input.Key.F))
                {
                    FreeCam.Freeze = true;
                    if(Picked != null)
                    {
                        Picked.SetOrientation(Quaternion.Multiply(Picked.GetOrientation(), Quaternion.FromAxisAngle(FreeCam.Cam.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up), (float)e.XDelta * -0.01f)));
                        Picked.SetOrientation(Quaternion.Multiply(Picked.GetOrientation(), Quaternion.FromAxisAngle(FreeCam.Cam.GetOrientation().GetTangent(MathExtensions.TangentDirection.Left), (float)e.YDelta * -0.01f)));
                    }
                }
                else if(kb.IsKeyDown(OpenTK.Input.Key.C))
                {
                    FreeCam.Freeze = true;
                    if(Picked != null)
                    {
                        Picked.SetScale(Picked.GetScale() + new Vector3((float)e.XDelta * -0.01f));
                    }
                }
                else
                    FreeCam.Freeze = Game.DisplayAdapter.IsCursorVisible;
            };
            Game.OnMouseWheel += (o, e) =>
            {
                Camera.Current.LensBlurAmount -= e.Delta / 2.0f;
            };
            Game.OnAfterDraw += (o, e) =>
            {
                //.ToString(System.Globalization.CultureInfo.InvariantCulture)

                //SettingsController.Instance.UpdatePerformance();
                /*var jpad = OpenTK.Input.GamePad.GetState(0);
                float deadzone = 0.15f;
                if(Picked != null)
                {
                    if(Math.Abs(jpad.ThumbSticks.Right.X) > deadzone)
                    {
                        var ang = Picked.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up);
                        Picked.Rotate(Quaternion.FromAxisAngle(ang, jpad.ThumbSticks.Right.X * 0.01f));
                    }
                    if(Math.Abs(jpad.ThumbSticks.Right.Y) > deadzone)
                    {
                        var ang = Picked.GetOrientation().GetTangent(MathExtensions.TangentDirection.Left);
                        Picked.Rotate(Quaternion.FromAxisAngle(ang, jpad.ThumbSticks.Right.Y * 0.01f));
                    }
                    if(Math.Abs(jpad.Triggers.Left) > deadzone)
                    {
                        var ang = Picked.GetOrientation().ToDirection();
                        Picked.Rotate(Quaternion.FromAxisAngle(ang, jpad.Triggers.Left * 0.01f));
                    }
                    if(Math.Abs(jpad.Triggers.Right) > deadzone)
                    {
                        var ang = Picked.GetOrientation().ToDirection();
                        Picked.Rotate(Quaternion.FromAxisAngle(ang, -jpad.Triggers.Right*0.01f));
                    }
                }*/
                var kb = OpenTK.Input.Keyboard.GetState();
                if(Game.DisplayAdapter.IsCursorVisible)
                {
                    if(!kb.IsKeyDown(OpenTK.Input.Key.LControl))
                    {
                        //Game.DisplayAdapter.Pipeline.PostProcessor.ShowSelected = false;
                    }
                    else
                    {
                        //Game.DisplayAdapter.Pipeline.PostProcessor.ShowSelected = true;
                        PickingResult.MapData(Vector4.One);
                        MousePicker.Use();
                        var state = OpenTK.Input.Mouse.GetState();
                        MousePicker.SetUniform("Mouse", new Vector2(MouseX, Game.Resolution.Height - MouseY));
                        MousePicker.SetUniform("Resolution", new Vector2(Game.Resolution.Width, Game.Resolution.Height));
                        PickingResult.Use(0);
                        GL.ActiveTexture(TextureUnit.Texture29);
                        GL.BindTexture(TextureTarget.Texture2D, Game.DisplayAdapter.MainRenderer.MRT.TexMeshIds);
                        MousePicker.Dispatch(1, 1, 1);
                        OpenTK.Graphics.OpenGL4.GL.MemoryBarrier(OpenTK.Graphics.OpenGL4.MemoryBarrierFlags.ShaderStorageBarrierBit);
                        byte[] result = PickingResult.Read(0, 4);
                        uint id = BitConverter.ToUInt32(result, 0);
                        foreach(var m in Game.World.Scene.GetFlatRenderableList())
                        {
                            if(m is Mesh3d)
                            {
                                foreach(var inst in (m as Mesh3d).GetInstances())
                                {
                                    if(inst.Id == id)
                                    {
                                        Picked = inst;
                                        PickedMesh = (m as Mesh3d);
                                        //SettingsController.Instance.SetMesh(inst);
                                    }
                                }
                            }
                        }
                    }
                }
                /*
                if(kb.IsKeyDown(OpenTK.Input.Key.Plus))
                {
                    var dir = Game.CascadeShadowMaps.GetDirection();
                    dir = Quaternion.Multiply(Quaternion.FromAxisAngle(Vector3.UnitX, -0.01f), dir);
                    Game.CascadeShadowMaps.SetDirection(dir);
                }

                if(kb.IsKeyDown(OpenTK.Input.Key.Minus))
                {
                    var dir = Game.CascadeShadowMaps.GetDirection();
                    dir = Quaternion.Multiply(Quaternion.FromAxisAngle(Vector3.UnitX, 0.01f), dir);
                    Game.CascadeShadowMaps.SetDirection(dir);
                }*/

                if(kb.IsKeyDown(OpenTK.Input.Key.T) && Picked != null)
                {
                    if(kb.IsKeyDown(OpenTK.Input.Key.Keypad4))
                    {
                        Picked.Translate(new Vector3(-0.01f, 0, 0));
                    }
                    if(kb.IsKeyDown(OpenTK.Input.Key.Keypad6))
                    {
                        Picked.Translate(new Vector3(0.01f, 0, 0));
                    }
                    if(kb.IsKeyDown(OpenTK.Input.Key.Keypad8))
                    {
                        Picked.Translate(new Vector3(0, 0, 0.01f));
                    }
                    if(kb.IsKeyDown(OpenTK.Input.Key.Keypad2))
                    {
                        Picked.Translate(new Vector3(0, 0, -0.01f));
                    }
                    if(kb.IsKeyDown(OpenTK.Input.Key.Keypad7))
                    {
                        Picked.Translate(new Vector3(0, 0.01f, 0));
                    }
                    if(kb.IsKeyDown(OpenTK.Input.Key.Keypad1))
                    {
                        Picked.Translate(new Vector3(0, -0.01f, 0));
                    }
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.C) && Picked != null)
                {
                    if(kb.IsKeyDown(OpenTK.Input.Key.Keypad8))
                    {
                        Picked.Scale(1.01f);
                    }
                    if(kb.IsKeyDown(OpenTK.Input.Key.Keypad1))
                    {
                        Picked.Scale(0.99f);
                    }
                }

                var rd = new Random();/*
                if(kb.IsKeyDown(OpenTK.Input.Key.Left))
                {
                    var pos = RedLight[0].camera.Transformation.GetPosition();
                    RedLight.ForEach((ax) => ax.camera.Transformation.SetPosition(pos + Vector3.UnitX / 12.0f));
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.Right))
                {
                    var pos = RedLight[0].camera.Transformation.GetPosition();
                    redConeLight.camera.Transformation.SetPosition(pos - Vector3.UnitX / 12.0f);
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.Up))
                {
                    var pos = RedLight[0].camera.Transformation.GetPosition();
                    redConeLight.camera.Transformation.SetPosition(pos + Vector3.UnitZ / 12.0f);
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.Down))
                {
                    var pos = RedLight[0].camera.Transformation.GetPosition();
                    redConeLight.camera.Transformation.SetPosition(pos - Vector3.UnitZ / 12.0f);
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.PageUp))
                {
                    var pos = RedLight[0].camera.Transformation.GetPosition();
                    redConeLight.camera.Transformation.SetPosition(pos + Vector3.UnitY / 12.0f);
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.PageDown))
                {
                    var pos = RedLight[0].camera.Transformation.GetPosition();
                    redConeLight.camera.Transformation.SetPosition(pos - Vector3.UnitY / 12.0f);
                }*/
                /*if(kb.IsKeyDown(OpenTK.Input.Key.U))
                {
                    var quat = Quaternion.FromAxisAngle(sun.Orientation.GetTangent(MathExtensions.TangentDirection.Left), -0.01f);
                    sun.Orientation = Quaternion.Multiply(sun.Orientation, quat);
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.J))
                {
                    var quat = Quaternion.FromAxisAngle(sun.Orientation.GetTangent(MathExtensions.TangentDirection.Left), 0.01f);
                    sun.Orientation = Quaternion.Multiply(sun.Orientation, quat);
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.H))
                {
                    var quat = Quaternion.FromAxisAngle(Vector3.UnitY, -0.01f);
                    sun.Orientation = Quaternion.Multiply(sun.Orientation, quat);
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.K))
                {
                    var quat = Quaternion.FromAxisAngle(Vector3.UnitY, 0.01f);
                    sun.Orientation = Quaternion.Multiply(sun.Orientation, quat);
                }*/
            };
            return RedLight;
        }

        public static FreeCamera SetUpFreeCamera()
        {
            CameraSavedViews = new Dictionary<int, TransformationManager>();
            var freeCamera = new FreeCamera((float)Game.Resolution.Width / (float)Game.Resolution.Height, MathHelper.PiOver3 / 1);
            FreeCam = freeCamera;
            PickingResult = new ShaderStorageBuffer();
            MousePicker = new ComputeShader("MousePicker.compute.glsl");

            return freeCamera;
        }

        public static void SetUpInputBehaviours()
        {
            Game.OnKeyUp += (o, e) =>
            {
                if(e.Key == OpenTK.Input.Key.F1 && !e.Shift)
                    InterpolateCameraFromSaved(0);
                if(e.Key == OpenTK.Input.Key.F2 && !e.Shift)
                    InterpolateCameraFromSaved(1);
                if(e.Key == OpenTK.Input.Key.F3 && !e.Shift)
                    InterpolateCameraFromSaved(2);
                if(e.Key == OpenTK.Input.Key.F4 && !e.Shift)
                    InterpolateCameraFromSaved(3);
                if(e.Key == OpenTK.Input.Key.F5 && !e.Shift)
                    InterpolateCameraFromSaved(4);
                if(e.Key == OpenTK.Input.Key.F6 && !e.Shift)
                    InterpolateCameraFromSaved(5);
                if(e.Key == OpenTK.Input.Key.F7 && !e.Shift)
                    InterpolateCameraFromSaved(6);

                if(e.Key == OpenTK.Input.Key.F1 && e.Shift)
                    SaveCamera(0);
                if(e.Key == OpenTK.Input.Key.F2 && e.Shift)
                    SaveCamera(1);
                if(e.Key == OpenTK.Input.Key.F3 && e.Shift)
                    SaveCamera(2);
                if(e.Key == OpenTK.Input.Key.F4 && e.Shift)
                    SaveCamera(3);
                if(e.Key == OpenTK.Input.Key.F5 && e.Shift)
                    SaveCamera(4);
                if(e.Key == OpenTK.Input.Key.F6 && e.Shift)
                    SaveCamera(5);
                if(e.Key == OpenTK.Input.Key.F7 && e.Shift)
                    SaveCamera(6);

                if(e.Key == OpenTK.Input.Key.Tab)
                {
                    Game.DisplayAdapter.IsCursorVisible = !Game.DisplayAdapter.IsCursorVisible;
                    FreeCam.Freeze = Game.DisplayAdapter.IsCursorVisible;
                }
                if(e.Key == OpenTK.Input.Key.Comma)
                {
                    if(PickedMesh.GetLodLevel(0) != null)
                    {
                        PickedMesh.GetLodLevel(0).Material.Roughness -= 0.05f;
                        if(PickedMesh.GetLodLevel(0).Material.Roughness < 0)
                            PickedMesh.GetLodLevel(0).Material.Roughness = 0;
                    }
                }
                if(e.Key == OpenTK.Input.Key.Period)
                {
                    if(PickedMesh.GetLodLevel(0) != null)
                    {
                        PickedMesh.GetLodLevel(0).Material.Roughness += 0.05f;
                        if(PickedMesh.GetLodLevel(0).Material.Roughness > 1)
                            PickedMesh.GetLodLevel(0).Material.Roughness = 1;
                    }
                }

                if(e.Key == OpenTK.Input.Key.T)
                {
                    /*if(PickedMesh != null)
                    {
                        PickedMesh.GetLodLevel(0).Material.ParallaxHeightMultiplier -= 0.1f;
                        if(PickedMesh.GetLodLevel(0).Material.ParallaxHeightMultiplier <= 0.01f)
                            PickedMesh.GetLodLevel(0).Material.ParallaxHeightMultiplier = 0.01f;
                    }*/
                }
                if(e.Key == OpenTK.Input.Key.Y)
                {
                    if(PickedMesh.GetLodLevel(0) != null)
                    {
                        PickedMesh.GetLodLevel(0).Material.ParallaxHeightMultiplier += 0.1f;
                        if(PickedMesh.GetLodLevel(0).Material.ParallaxHeightMultiplier >= 24)
                            PickedMesh.GetLodLevel(0).Material.ParallaxHeightMultiplier = 24;
                    }
                }

                if(e.Key == OpenTK.Input.Key.Pause)
                {
                    ShaderProgram.RecompileAll();
                    ComputeShader.RecompileAll();
                }
                if(e.Key == OpenTK.Input.Key.R)
                {
                    // Game.DisplayAdapter.Pipeline.PostProcessor.UnbiasedIntegrateRenderMode = !Game.DisplayAdapter.Pipeline.PostProcessor.UnbiasedIntegrateRenderMode;
                }
                if(e.Key == OpenTK.Input.Key.LBracket)
                {
                    FreeCam.Cam.Brightness -= 0.1f;
                }
                if(e.Key == OpenTK.Input.Key.RBracket)
                {
                    FreeCam.Cam.Brightness += 0.1f;
                }
                if(e.Key == OpenTK.Input.Key.F1)
                {
                    RedLight[0].GetTransformationManager().SetPosition(FreeCam.Cam.Transformation.GetPosition() + new Vector3((float)rand.NextDouble() * 2 - 1, (float)rand.NextDouble() * 2 - 1, (float)rand.NextDouble() * 2 - 1) * 0.1f);
                    RedLight[0].GetTransformationManager().SetOrientation(FreeCam.Cam.Transformation.GetOrientation());
                    Picked = RedLight[0];
                }
                if(e.Key == OpenTK.Input.Key.Tilde)
                {
                    // Interpolator.Interpolate<Vector3>(RedLight.GetTransformationManager().Position, RedLight.GetTransformationManager().Position.R, FreeCam.Cam.GetPosition(), 8.0f, Interpolator.Easing.EaseInOut);
                }
                var settings = Game.DisplayAdapter.MainRenderer.GraphicsSettings;
                if(e.Key == OpenTK.Input.Key.Number0)
                    settings.UseVDAO = !settings.UseVDAO;
                if(e.Key == OpenTK.Input.Key.Number9)
                    settings.UseBloom = !settings.UseBloom;
                if(e.Key == OpenTK.Input.Key.Number8)
                    settings.UseDeferred = !settings.UseDeferred;
                if(e.Key == OpenTK.Input.Key.Number7)
                    settings.UseDepth = !settings.UseDepth;
                if(e.Key == OpenTK.Input.Key.Number6)
                    settings.UseFog = !settings.UseFog;
                if(e.Key == OpenTK.Input.Key.Number5)
                    settings.UseCubeMapGI = !settings.UseCubeMapGI;
                if(e.Key == OpenTK.Input.Key.Number4)
                    settings.UseRSM = !settings.UseRSM;
                if(e.Key == OpenTK.Input.Key.Number3)
                    settings.UseVXGI = !settings.UseVXGI;
                if(e.Key == OpenTK.Input.Key.Number2)
                    settings.UseHBAO = !settings.UseHBAO;
            };
        }

        private static void InterpolateCameraFromSaved(int index)
        {
            if(!CameraSavedViews.ContainsKey(index))
                return;
            var pos = CameraSavedViews[index].GetPosition();
            var orient = CameraSavedViews[index].GetOrientation();

            Interpolator.Interpolate<Vector3>(FreeCam.Cam.Transformation.Position, FreeCam.Cam.Transformation.Position.R, pos, 8.0f, Interpolator.Easing.EaseInOut);
            Interpolator.Interpolate<Quaternion>(FreeCam.Cam.Transformation.Orientation, FreeCam.Cam.Transformation.Orientation.R, orient, 8.0f, Interpolator.Easing.EaseInOut);
        }

        private static void SaveCamera(int index)
        {
            CameraSavedViews[index] = FreeCam.Cam.Transformation.Copy();
        }
    }
}