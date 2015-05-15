using System.Collections.Generic;
using System.Linq;
using OpenTK;
using System.Drawing;

namespace VEngine
{
    public static class LightPool
    {
        private static byte InternalCounter = 0;
        private static List<ILight> Lights = new List<ILight>();

        private static List<SimplePointLight> SimpleLights = new List<SimplePointLight>();

        public static void Add(SimplePointLight light)
        {
            SimpleLights.Add(light);
        }
        public static void Remove(SimplePointLight light)
        {
            SimpleLights.Remove(light);
        }
        public static void RemoveAllSimple()
        {
            SimpleLights.Clear();
        }

        public static void MapSimpleLightsToShader(ShaderProgram sp)
        {
            sp.SetUniform("SimpleLightsCount", SimpleLights.Count);
            sp.SetUniformArray("SimpleLightsPos", SimpleLights.Select<SimplePointLight, Vector3>(a => a.Transformation.GetPosition()).ToArray());
            sp.SetUniformArray("SimpleLightsColors", SimpleLights.Select<SimplePointLight, Vector4>(a =>
                new Vector4(a.Color.R / 255.0f, a.Color.G / 255.0f, a.Color.B / 255.0f, a.Color.A / 255.0f)).ToArray());
        }

        public static void Add(ILight light)
        {
            if(!Lights.Contains(light))
                Lights.Add(light);
        }

        public static Vector4[] GetColors()
        {
            return Lights.Select<ILight, Vector4>(a => a.GetColor()).ToArray();
        }

        public static float[] GetFarPlanes()
        {
            return Lights.Select<ILight, float>(a => a.GetFarPlane()).ToArray();
        }
        public static Vector2[] GetRanges()
        {
            return Lights.Select<ILight, Vector2>(a => new Vector2(a.GetMixRange().Start, a.GetMixRange().End)).ToArray();
        }
        public static int[] GetMixModes()
        {
            return Lights.Select<ILight, int>(a => (int)a.GetMixMode()).ToArray();
        }

        public static Matrix4[] GetPMatrices()
        {
            return Lights.Select<ILight, Matrix4>(a => a.GetPMatrix()).ToArray();
        }

        public static Vector3[] GetPositions()
        {
            return Lights.Select<ILight, Vector3>(a => a is ProjectionLight && (a as ProjectionLight).FakePosition != Vector3.Zero ? (a as ProjectionLight).FakePosition : a.GetPosition()).ToArray();
        }

        public static Matrix4[] GetVMatrices()
        {
            return Lights.Select<ILight, Matrix4>(a => a.GetVMatrix()).ToArray();
        }

        public static void MapAll()
        {
            for(int i = 0; i < Lights.Count; i++)
            {
                Lights[i].Map();
            }
            /* if(Lights.Count > InternalCounter)
                 Lights[InternalCounter].Map();
             InternalCounter++;
             if(InternalCounter >= Lights.Count)
                 InternalCounter = 0;*/
        }

        public static void Remove(ILight light)
        {
            if(Lights.Contains(light))
                Lights.Remove(light);
        }

        public static void UseTextures(int index)
        {
            int i = index;
            foreach(var light in Lights)
            {
                light.UseTexture(i);
                i++;
            }
        }
    }
}