using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace VEngine
{
    sealed public class Scene
    {
        private List<byte> Buffer;

        private List<Vector4> colors = new List<Vector4>();

        private List<float> fplanes = new List<float>();

        private int ipointer = 0;

        private int LastBufferElements = 0;

        private List<int> mmodes = new List<int>();

        private List<Matrix4> pmats = new List<Matrix4>();
        private List<int> shadowmaplayers = new List<int>();

        private List<Vector3> poss = new List<Vector3>();

        private List<float> ShadowMapsFarPlanes;

        private int SimpleLightsCount = 0;

        private ShaderStorageBuffer SSBO = new ShaderStorageBuffer();

        private List<Matrix4> vmats = new List<Matrix4>();

        private List<ILight> Lights = new List<ILight>();

        private List<IRenderable> Renderables = new List<IRenderable>();

        public Scene()
        {
            SSBO.Type = OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicDraw;
        }

        public void Add(Scene e)
        {
            Lights.AddRange(e.Lights);
            Renderables.AddRange(e.Renderables);
        }

        public void Add(ILight e)
        {
            Lights.Add(e);
            if(e is ProjectionLight)
                Game.Invoke(() =>
                    Game.ShadowMaps.UpdateFromLightsList(
                        Lights
                        .Where((a) => a is ProjectionLight)
                        .Cast<ProjectionLight>().ToList()));
        }

        public void Add(IRenderable e)
        {
            Renderables.Add(e);
        }

        public void Draw()
        {
            // var cpos = Camera.MainDisplayCamera.GetPosition();
            /*Renderables.Sort((a, b) =>
            {
                if(a is ITransformable && b is ITransformable)
                {
                    var e1 = (a as ITransformable).GetPosition();
                    var e2 = (b as ITransformable).GetPosition();
                    var x = (e1 - cpos).Length;
                    var y = (e2 - cpos).Length;
                    return x < y ? -1 : 1;
                }
                else
                    return 0;
            });*/
            for(int i = 0; i < Renderables.Count; i++)
                Renderables[i].Draw();
        }

        public List<IRenderable> GetFlatRenderableList()
        {
            var o = new List<IRenderable>();
            Renderables.ForEach((a) =>
            {
                o.Add(a);
            });
            return o;
        }

        public void MapLights()
        {
            for(int i = 0; i < Lights.Count; i++)
            {
                var e = Lights[i];
                if(e is IShadowMapableLight)
                {
                    (e as IShadowMapableLight).Map();
                }
            }
        }

        public void MapLightsSSBOToShader(ShaderProgram sp)
        {
            SSBO.Use(6);
            ShaderProgram.Current.SetUniform("SimpleLightsCount", SimpleLightsCount);
        }

        public void RecreateSimpleLightsSSBO()
        {
            Buffer = new List<byte>();
            SimpleLightsCount = 0;
            bool update = false;
            foreach(var e in Lights)
            {
                if(e is SimplePointLight)
                {
                    if((e as SimplePointLight).Transformation.HasBeenModified())
                        update = true;
                    Buffer.AddRange(Bytes((e as SimplePointLight).Transformation.GetPosition()));
                    Buffer.AddRange(Bytes((e as SimplePointLight).Transformation.GetOrientation().ToDirection()));
                    Buffer.AddRange(Bytes(e.GetColor(), (e as SimplePointLight).Angle));
                    Buffer.AddRange(Bytes(e.GetColor(), (e as SimplePointLight).Angle));

                    SimpleLightsCount++;
                }
            }
            // if(!update)
            //      Buffer = new List<byte>();
            //  else if(Buffer.Count == 0)
            //       Buffer.Add(0);
            SSBO.MapData(Buffer.ToArray());
        }

        public void Remove(ILight e)
        {
            Lights.Remove(e);
        }

        public void Remove(IRenderable e)
        {
            Renderables.Remove(e);
        }

        public void SetLightingUniforms(ShaderProgram shader)
        {
            // MEMORY LEAK
            pmats = new List<Matrix4>();
            shadowmaplayers = new List<int>();
            vmats = new List<Matrix4>();
            poss = new List<Vector3>();
            fplanes = new List<float>();
            colors = new List<Vector4>();
            mmodes = new List<int>();
            ipointer = 0;

            foreach(var e in Lights)
                if(e is IShadowMapableLight)
                {
                    var l = e as IShadowMapableLight;
                    var p = e as ILight;
                    pmats.Add(l.GetPMatrix());
                    vmats.Add(l.GetVMatrix());
                    shadowmaplayers.Add((l as ProjectionLight).ShadowMapArrayIndex);
                    poss.Add(p.GetPosition());
                    colors.Add(new Vector4(p.GetColor()));
                    ipointer++;
                }
            Game.ShadowMaps.Bind(0);
            shader.SetUniformArray("LightsPs", pmats.ToArray());
            shader.SetUniformArray("LightsVs", vmats.ToArray());
            shader.SetUniformArray("LightsShadowMapsLayer", shadowmaplayers.ToArray());
            shader.SetUniformArray("LightsPos", poss.ToArray());
            shader.SetUniformArray("LightsFarPlane", fplanes.ToArray());
            shader.SetUniformArray("LightsColors", colors.ToArray());
            shader.SetUniform("LightsCount", pmats.Count);
            pmats = new List<Matrix4>();
            vmats = new List<Matrix4>();
            poss = new List<Vector3>();
            fplanes = new List<float>();
            colors = new List<Vector4>();
            mmodes = new List<int>();
        }
        public void SetLightingUniforms(ComputeShader shader)
        {
            // MEMORY LEAK
            pmats = new List<Matrix4>();
            shadowmaplayers = new List<int>();
            vmats = new List<Matrix4>();
            poss = new List<Vector3>();
            fplanes = new List<float>();
            colors = new List<Vector4>();
            mmodes = new List<int>();
            ipointer = 0;

            foreach(var e in Lights)
                if(e is IShadowMapableLight)
                {
                    var l = e as IShadowMapableLight;
                    var p = e as ILight;
                    pmats.Add(l.GetPMatrix());
                    vmats.Add(l.GetVMatrix());
                    shadowmaplayers.Add((l as ProjectionLight).ShadowMapArrayIndex);
                    poss.Add(p.GetPosition());
                    colors.Add(new Vector4(p.GetColor()));
                    ipointer++;
                }
            Game.ShadowMaps.Bind(0);
            shader.SetUniformArray("LightsPs", pmats.ToArray());
            shader.SetUniformArray("LightsVs", vmats.ToArray());
            shader.SetUniformArray("LightsShadowMapsLayer", shadowmaplayers.ToArray());
            shader.SetUniformArray("LightsPos", poss.ToArray());
            shader.SetUniformArray("LightsFarPlane", fplanes.ToArray());
            shader.SetUniformArray("LightsColors", colors.ToArray());
            shader.SetUniform("LightsCount", pmats.Count);
            pmats = new List<Matrix4>();
            vmats = new List<Matrix4>();
            poss = new List<Vector3>();
            fplanes = new List<float>();
            colors = new List<Vector4>();
            mmodes = new List<int>();
        }

        private static List<byte> Bytes(Vector4 vec)
        {
            var b = new List<byte>();
            b.AddRange(BitConverter.GetBytes(vec.X));
            b.AddRange(BitConverter.GetBytes(vec.Y));
            b.AddRange(BitConverter.GetBytes(vec.Z));
            b.AddRange(BitConverter.GetBytes(vec.W));
            return b;
        }

        private static List<byte> Bytes(Vector3 vec, float additional = 0)
        {
            var b = new List<byte>();
            b.AddRange(BitConverter.GetBytes(vec.X));
            b.AddRange(BitConverter.GetBytes(vec.Y));
            b.AddRange(BitConverter.GetBytes(vec.Z));
            b.AddRange(BitConverter.GetBytes(additional));
            return b;
        }

        private static List<byte> Bytes(Matrix4 v)
        {
            var b = new List<byte>();
            b.AddRange(BitConverter.GetBytes(v.Row0.X));
            b.AddRange(BitConverter.GetBytes(v.Row0.Y));
            b.AddRange(BitConverter.GetBytes(v.Row0.Z));
            b.AddRange(BitConverter.GetBytes(v.Row0.W));

            b.AddRange(BitConverter.GetBytes(v.Row1.X));
            b.AddRange(BitConverter.GetBytes(v.Row1.Y));
            b.AddRange(BitConverter.GetBytes(v.Row1.Z));
            b.AddRange(BitConverter.GetBytes(v.Row1.W));

            b.AddRange(BitConverter.GetBytes(v.Row2.X));
            b.AddRange(BitConverter.GetBytes(v.Row2.Y));
            b.AddRange(BitConverter.GetBytes(v.Row2.Z));
            b.AddRange(BitConverter.GetBytes(v.Row2.W));

            b.AddRange(BitConverter.GetBytes(v.Row3.X));
            b.AddRange(BitConverter.GetBytes(v.Row3.Y));
            b.AddRange(BitConverter.GetBytes(v.Row3.Z));
            b.AddRange(BitConverter.GetBytes(v.Row3.W));

            return b;
        }

        private static List<byte> Bytes(float vec)
        {
            var b = new List<byte>();
            b.AddRange(BitConverter.GetBytes(vec));
            return b;
        }

        private static List<byte> Bytes(int vec)
        {
            var b = new List<byte>();
            b.AddRange(BitConverter.GetBytes(vec));
            return b;
        }

        private static Vector3 mul(Matrix4 mat, Vector3 vec)
        {
            return Vector4.Transform(new Vector4(vec, 1.0f), mat).Xyz;
        }
    }
}