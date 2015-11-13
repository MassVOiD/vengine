using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using VEngine;

namespace ShadowsTester
{
    internal class Commons
    {
        public static FreeCamera FreeCam;
        private static ComputeShader MousePicker;
        public static int MouseX, MouseY;
        private static Mesh3d Picked;
        private static ShaderStorageBuffer PickingResult;
        private static ProjectionLight RedLight;

        public static ProjectionLight AddControllableLight()
        {
            float fovdegree = 90;
            ProjectionLight redConeLight = new ProjectionLight(new Vector3(65, 0, 65), Quaternion.FromAxisAngle(new Vector3(1, 0, -1), MathHelper.DegreesToRadians(fovdegree)), 2048, 2048, MathHelper.DegreesToRadians(45), 0.1f, 10000.0f);
            RedLight = redConeLight;
            redConeLight.LightColor = new Vector4(1, 1, 1, 395);
            //redConeLight.BuildOrthographicProjection(600, 600, -150, 150);

            GLThread.OnKeyUp += (o, e) =>
            {
                if(e.Key == OpenTK.Input.Key.J)
                {
                    fovdegree += 5f;
                    if(fovdegree >= 180)
                        fovdegree = 179;
                    Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fovdegree), 1, 0.1f, 10000.0f, out redConeLight.camera.ProjectionMatrix);
                }
                if(e.Key == OpenTK.Input.Key.K)
                {
                    fovdegree -= 5f;
                    if(fovdegree <= 10)
                        fovdegree = 10;
                    Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fovdegree), 1, 0.1f, 10000.0f, out redConeLight.camera.ProjectionMatrix);
                }
            };
            World.Root.RootScene.Add(redConeLight);

            GLThread.OnMouseMove += (o, e) =>
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
                    FreeCam.Freeze = GLThread.DisplayAdapter.IsCursorVisible;
            };
            GLThread.OnMouseWheel += (o, e) =>
            {
                Camera.Current.LensBlurAmount -= e.Delta / 2.0f;
            };
            GLThread.OnUpdate += (o, e) =>
            {
                //SettingsController.Instance.UpdatePerformance();
                var jpad = OpenTK.Input.GamePad.GetState(0);
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
                }
                var kb = OpenTK.Input.Keyboard.GetState();
                if(GLThread.DisplayAdapter.IsCursorVisible)
                {
                    if(!kb.IsKeyDown(OpenTK.Input.Key.LControl))
                    {
                        GLThread.DisplayAdapter.Pipeline.PostProcessor.ShowSelected = false;
                    }
                    else
                    {
                        GLThread.DisplayAdapter.Pipeline.PostProcessor.ShowSelected = true;
                        PickingResult.MapData(Vector4.One);
                        MousePicker.Use();
                        var state = OpenTK.Input.Mouse.GetState();
                        MousePicker.SetUniform("Mouse", new Vector2(MouseX, GLThread.Resolution.Height - MouseY));
                        PickingResult.Use(0);
                        GL.BindImageTexture(0, GLThread.DisplayAdapter.Pipeline.PostProcessor.MRT.TexId, 0, false, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba32ui);
                        MousePicker.Dispatch(1, 1, 1);
                        OpenTK.Graphics.OpenGL4.GL.MemoryBarrier(OpenTK.Graphics.OpenGL4.MemoryBarrierFlags.ShaderStorageBarrierBit);
                        byte[] result = PickingResult.Read(0, 4);
                        uint id = BitConverter.ToUInt32(result, 0);
                        foreach(var m in World.Root.RootScene.GetFlatRenderableList())
                        {
                            if(m is Mesh3d)
                            {
                                if((m as Mesh3d).MeshColoredID == id)
                                {
                                    (m as Mesh3d).Selected = true;
                                    Picked = m as Mesh3d;
                                    SettingsController.Instance.SetMesh(Picked);
                                }
                                else
                                    (m as Mesh3d).Selected = false;
                            }
                        }
                    }
                }

                if(kb.IsKeyDown(OpenTK.Input.Key.Left))
                {
                    var pos = redConeLight.camera.Transformation.GetPosition();
                    redConeLight.camera.Transformation.SetPosition(pos + Vector3.UnitX / 12.0f);
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.Right))
                {
                    var pos = redConeLight.camera.Transformation.GetPosition();
                    redConeLight.camera.Transformation.SetPosition(pos - Vector3.UnitX / 12.0f);
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.Up))
                {
                    var pos = redConeLight.camera.Transformation.GetPosition();
                    redConeLight.camera.Transformation.SetPosition(pos + Vector3.UnitZ / 12.0f);
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.Down))
                {
                    var pos = redConeLight.camera.Transformation.GetPosition();
                    redConeLight.camera.Transformation.SetPosition(pos - Vector3.UnitZ / 12.0f);
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.PageUp))
                {
                    var pos = redConeLight.camera.Transformation.GetPosition();
                    redConeLight.camera.Transformation.SetPosition(pos + Vector3.UnitY / 12.0f);
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.PageDown))
                {
                    var pos = redConeLight.camera.Transformation.GetPosition();
                    redConeLight.camera.Transformation.SetPosition(pos - Vector3.UnitY / 12.0f);
                }
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
            return redConeLight;
        }

        public static FreeCamera SetUpFreeCamera()
        {
            CameraSavedViews = new Dictionary<int, TransformationManager>();
            float aspect = GLThread.Resolution.Height > GLThread.Resolution.Width ? GLThread.Resolution.Height / GLThread.Resolution.Width : GLThread.Resolution.Width / GLThread.Resolution.Height;
            var freeCamera = new FreeCamera((float)GLThread.Resolution.Width / (float)GLThread.Resolution.Height, MathHelper.PiOver3 / 1);
            FreeCam = freeCamera;
            PickingResult = new ShaderStorageBuffer();
            MousePicker = new ComputeShader("MousePicker.compute.glsl");
            return freeCamera;
        }

        static Dictionary<int, TransformationManager> CameraSavedViews;

        static void SaveCamera(int index)
        {
            CameraSavedViews[index] = FreeCam.Cam.Transformation.Copy();
        }

        static void InterpolateCameraFromSaved(int index)
        {
            if(!CameraSavedViews.ContainsKey(index))
                return;
            var pos = CameraSavedViews[index].GetPosition();
            var orient = CameraSavedViews[index].GetOrientation();

            Interpolator.Interpolate<Vector3>(FreeCam.Cam.Transformation.Position, FreeCam.Cam.Transformation.Position.R, pos, 8.0f, Interpolator.Easing.EaseInOut);
            Interpolator.Interpolate<Quaternion>(FreeCam.Cam.Transformation.Orientation, FreeCam.Cam.Transformation.Orientation.R, orient, 8.0f, Interpolator.Easing.EaseInOut);
        }

        public static void SetUpInputBehaviours()
        {
           
            GLThread.OnKeyUp += (o, e) =>
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
                    GLThread.DisplayAdapter.IsCursorVisible = !GLThread.DisplayAdapter.IsCursorVisible;
                    FreeCam.Freeze = GLThread.DisplayAdapter.IsCursorVisible;
                }
                if(e.Key == OpenTK.Input.Key.Comma)
                {
                    if(Picked != null)
                    {
                        Picked.MainMaterial.Roughness -= 0.05f;
                        if(Picked.MainMaterial.Roughness < 0)
                            Picked.MainMaterial.Roughness = 0;
                    }
                }
                if(e.Key == OpenTK.Input.Key.Period)
                {
                    if(Picked != null)
                    {
                        Picked.MainMaterial.Roughness += 0.05f;
                        if(Picked.MainMaterial.Roughness > 1)
                            Picked.MainMaterial.Roughness = 1;
                    }
                }
                if(e.Key == OpenTK.Input.Key.G)
                {
                    if(Picked != null)
                    {
                        Picked.MainMaterial.SpecularComponent -= 0.05f;
                        if(Picked.MainMaterial.SpecularComponent < 0)
                            Picked.MainMaterial.SpecularComponent = 0;
                    }
                }
                if(e.Key == OpenTK.Input.Key.H)
                {
                    if(Picked != null)
                    {
                        Picked.MainMaterial.SpecularComponent += 0.05f;
                        if(Picked.MainMaterial.SpecularComponent > 1)
                            Picked.MainMaterial.SpecularComponent = 1;
                    }
                }
                if(e.Key == OpenTK.Input.Key.Semicolon)
                {
                    if(Picked != null)
                    {
                        Picked.MainMaterial.Metalness -= 0.05f;
                        if(Picked.MainMaterial.Metalness < 0)
                            Picked.MainMaterial.Metalness = 0;
                    }
                }
                if(e.Key == OpenTK.Input.Key.Quote)
                {
                    if(Picked != null)
                    {
                        Picked.MainMaterial.Metalness += 0.05f;
                        if(Picked.MainMaterial.Metalness > 1)
                            Picked.MainMaterial.Metalness = 1;
                    }
                }
                if(e.Key == OpenTK.Input.Key.T)
                {
                    if(Picked != null)
                    {
                        Picked.MainMaterial.ParallaxHeightMultiplier -= 0.1f;
                        if(Picked.MainMaterial.ParallaxHeightMultiplier <=0.01f)
                            Picked.MainMaterial.ParallaxHeightMultiplier = 0.01f;
                    }
                }
                if(e.Key == OpenTK.Input.Key.Y)
                {
                    if(Picked != null)
                    {
                        Picked.MainMaterial.ParallaxHeightMultiplier+=0.1f;
                        if(Picked.MainMaterial.ParallaxHeightMultiplier >= 24)
                            Picked.MainMaterial.ParallaxHeightMultiplier = 24;
                    }
                }
                if(e.Key == OpenTK.Input.Key.U)
                {
                    if(Picked != null)
                    {
                        Picked.MainMaterial.ParallaxInstances--;
                        if(Picked.MainMaterial.ParallaxInstances <= 0)
                            Picked.MainMaterial.ParallaxInstances = 0;
                    }
                }
                if(e.Key == OpenTK.Input.Key.I)
                {
                    if(Picked != null)
                    {
                        Picked.MainMaterial.ParallaxInstances++;
                        if(Picked.MainMaterial.ParallaxInstances >= 24)
                            Picked.MainMaterial.ParallaxInstances = 24;
                    }
                }
                if(e.Key == OpenTK.Input.Key.Pause)
                {
                    ShaderProgram.RecompileAll();
                    ComputeShader.RecompileAll();
                }
                if(e.Key == OpenTK.Input.Key.R)
                {
                    GLThread.DisplayAdapter.Pipeline.PostProcessor.UnbiasedIntegrateRenderMode = !GLThread.DisplayAdapter.Pipeline.PostProcessor.UnbiasedIntegrateRenderMode;
                }
                if(e.Key == OpenTK.Input.Key.LBracket)
                {
                    FreeCam.Cam.Brightness -= 0.1f;
                }
                if(e.Key == OpenTK.Input.Key.RBracket)
                {
                    FreeCam.Cam.Brightness += 0.1f;
                }
                if(e.Key == OpenTK.Input.Key.Number1)
                {
                    //redConeLight.SetPosition(freeCamera.Cam.Transformation.GetPosition(), freeCamera.Cam.Transformation.GetPosition() + freeCamera.Cam.Transformation.GetOrientation().ToDirection());
                    RedLight.GetTransformationManager().SetPosition(FreeCam.Cam.Transformation.GetPosition());
                    RedLight.GetTransformationManager().SetOrientation(FreeCam.Cam.Transformation.GetOrientation());
                }
                if(e.Key == OpenTK.Input.Key.Tilde)
                {
                    Interpolator.Interpolate<Vector3>(RedLight.GetTransformationManager().Position, RedLight.GetTransformationManager().Position.R, FreeCam.Cam.GetPosition(), 8.0f, Interpolator.Easing.EaseInOut);
                }
                if(e.Key == OpenTK.Input.Key.Number0)
                    GLThread.GraphicsSettings.UseVDAO = !GLThread.GraphicsSettings.UseVDAO;
                if(e.Key == OpenTK.Input.Key.Number9)
                    GLThread.GraphicsSettings.UseBloom = !GLThread.GraphicsSettings.UseBloom;
                if(e.Key == OpenTK.Input.Key.Number8)
                    GLThread.GraphicsSettings.UseDeferred = !GLThread.GraphicsSettings.UseDeferred;
                if(e.Key == OpenTK.Input.Key.Number7)
                    GLThread.GraphicsSettings.UseDepth = !GLThread.GraphicsSettings.UseDepth;
                if(e.Key == OpenTK.Input.Key.Number6)
                    GLThread.GraphicsSettings.UseFog = !GLThread.GraphicsSettings.UseFog;
                if(e.Key == OpenTK.Input.Key.Number5)
                    GLThread.GraphicsSettings.UseLightPoints = !GLThread.GraphicsSettings.UseLightPoints;
                if(e.Key == OpenTK.Input.Key.Number4)
                    GLThread.GraphicsSettings.UseRSM = !GLThread.GraphicsSettings.UseRSM;
                if(e.Key == OpenTK.Input.Key.Number3)
                    GLThread.GraphicsSettings.UseSSReflections = !GLThread.GraphicsSettings.UseSSReflections;
                if(e.Key == OpenTK.Input.Key.Number2)
                    GLThread.GraphicsSettings.UseHBAO = !GLThread.GraphicsSettings.UseHBAO;
            };
        }
    }
}