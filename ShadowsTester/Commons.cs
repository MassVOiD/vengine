using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using VEngine;

namespace ShadowsTester
{
    class Commons
    {
        public static FreeCamera FreeCam;
        static ProjectionLight RedLight;
        private static ShaderStorageBuffer PickingResult;
        static ComputeShader MousePicker;
        static int MouseX, MouseY;
        static Mesh3d Picked;
        public static FreeCamera SetUpFreeCamera()
        {

            float aspect = GLThread.Resolution.Height > GLThread.Resolution.Width ? GLThread.Resolution.Height / GLThread.Resolution.Width : GLThread.Resolution.Width / GLThread.Resolution.Height;
            var freeCamera = new FreeCamera((float)GLThread.Resolution.Width / (float)GLThread.Resolution.Height, MathHelper.PiOver3/1);
            FreeCam = freeCamera;
            PickingResult = new ShaderStorageBuffer();
            MousePicker = new ComputeShader("MousePicker.compute.glsl");
            return freeCamera;
        }

        public static ProjectionLight AddControllableLight()
        {

            ProjectionLight redConeLight = new ProjectionLight(new Vector3(65, 0, 65), Quaternion.FromAxisAngle(new Vector3(1, 0, -1), MathHelper.Pi / 2), 2048, 2048, MathHelper.PiOver3, 0.1f, 10000.0f);
            RedLight = redConeLight;
            redConeLight.LightColor = new Vector4(1, 1, 1, 25);
            //redConeLight.BuildOrthographicProjection(600, 600, -150, 150);

            LightPool.Add(redConeLight);

            GLThread.OnMouseMove += (o, e) =>
            {
                MouseX = e.X;
                MouseY = e.Y;
            };

            GLThread.OnUpdate += (o, e) =>
            {
                if(GLThread.DisplayAdapter.IsCursorVisible)
                {
                    PickingResult.MapData(Vector4.One);
                    MousePicker.Use();
                    var state = OpenTK.Input.Mouse.GetState();
                    MousePicker.SetUniform("Mouse", new Vector2(MouseX, GLThread.Resolution.Height  - MouseY));
                    PickingResult.Use(0);
                    GL.BindImageTexture(0, GLThread.DisplayAdapter.Pipeline.PostProcessor.MRT.TexId, 0, false, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba8);
                    MousePicker.Dispatch(GLThread.Resolution.Width / 32, GLThread.Resolution.Height / 32, 1);
                    OpenTK.Graphics.OpenGL4.GL.MemoryBarrier(OpenTK.Graphics.OpenGL4.MemoryBarrierFlags.ShaderStorageBarrierBit);
                    byte[] result = PickingResult.Read(0, 4 * 3);
                    Vector3 id = new Vector3(
                        BitConverter.ToSingle(result, 0),
                        BitConverter.ToSingle(result, 4),
                        BitConverter.ToSingle(result, 8)
                    );
                    foreach(var m in World.Root.Children)
                    {
                        if(m is Mesh3d)
                        {
                            if(((m as Mesh3d).MeshColoredID - id).Length < 0.05f)
                            {
                                (m as Mesh3d).Selected = true;
                                Picked = m as Mesh3d;
                            }
                            else
                                (m as Mesh3d).Selected = false;
                        }
                    }
                }
                var kb = OpenTK.Input.Keyboard.GetState();
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

        public static void SetUpInputBehaviours()
        {

            GLThread.OnMouseUp += (o, e) =>
            {
                if(e.Button == OpenTK.Input.MouseButton.Middle)
                {
                    Mesh3d mesh = Camera.Current.RayCastMesh3d();
                    if(mesh != null && mesh.GetCollisionShape() != null)
                    {
                        Console.WriteLine(mesh.GetCollisionShape().ToString());
                        mesh.PhysicalBody.LinearVelocity += (Camera.Current.GetDirection() * 120.0f);

                    }
                }
            };

            MeshLinker.LinkInfo pickedLink = null;
            bool inPickingMode = false;
            GLThread.OnMouseDown += (o, e) =>
            {
                if(e.Button == OpenTK.Input.MouseButton.Right)
                {
                    Mesh3d mesh = Camera.Current.RayCastMesh3d();
                    if(mesh != null && mesh.GetCollisionShape() != null && !mesh.PhysicalBody.IsStaticObject && !inPickingMode)
                    {
                        Console.WriteLine(mesh.GetCollisionShape().ToString());
                        pickedLink = MeshLinker.Link(FreeCam.Cam, mesh, new Vector3(0, 0, -(FreeCam.Cam.Transformation.GetPosition() - mesh.Transformation.GetPosition()).Length),
                            Quaternion.Identity);
                        pickedLink.UpdateRotation = false;
                        inPickingMode = true;
                    }
                }
            };
            GLThread.OnMouseUp += (o, e) =>
            {
                if(e.Button == OpenTK.Input.MouseButton.Right)
                {
                    MeshLinker.Unlink(pickedLink);
                    pickedLink = null;
                    inPickingMode = false;
                }
            };

            GLThread.OnMouseWheel += (o, e) =>
            {
                if(e.Delta != 0 && inPickingMode)
                {
                    pickedLink.Offset.Z -= e.Delta * 3.0f;
                    if(pickedLink.Offset.Z > -5)
                        pickedLink.Offset.Z = -5;
                }
            };
            GLThread.OnMouseWheel += (o, e) =>
            {
                if(!inPickingMode)
                    Camera.Current.LensBlurAmount -= e.Delta / 20.0f;
            };
            World.Root.SimulationSpeed = 1.0f;

            GLThread.OnKeyDown += (o, e) =>
            {
                if(e.Key == OpenTK.Input.Key.T)
                {
                    World.Root.SimulationSpeed = 0.4f;
                }
                if(e.Key == OpenTK.Input.Key.Y)
                {
                    World.Root.SimulationSpeed = 0.10f;
                }

            };
            GLThread.OnKeyUp += (o, e) =>
            {
                if(e.Key == OpenTK.Input.Key.Tab)
                {
                    GLThread.DisplayAdapter.IsCursorVisible = !GLThread.DisplayAdapter.IsCursorVisible;
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
                if(e.Key == OpenTK.Input.Key.Pause)
                {
                    ShaderProgram.RecompileAll();
                    ComputeShader.RecompileAll();
                }
                if(e.Key == OpenTK.Input.Key.LBracket)
                {
                    FreeCam.Cam.Brightness -= 0.1f;
                }
                if(e.Key == OpenTK.Input.Key.RBracket)
                {
                    FreeCam.Cam.Brightness += 0.1f;
                }
                if(e.Key == OpenTK.Input.Key.T)
                {
                    World.Root.SimulationSpeed = 1.0f;
                }
                if(e.Key == OpenTK.Input.Key.Y)
                {
                    World.Root.SimulationSpeed = 1.0f;
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
